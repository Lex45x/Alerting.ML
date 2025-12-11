using System.CommandLine;
using System.Runtime.CompilerServices;
using Alerting.ML.Console;
using Alerting.ML.Engine;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Optimizer.Events;
using Alerting.ML.Engine.Storage;
using Alerting.ML.Sources.Azure;
using Alerting.ML.Sources.Csv;
using Alerting.ML.TimeSeries.Sample;
using Action = Alerting.ML.Console.Action;

KnownTypeInfoResolver.Instance.WithAzureTypes();

var trainingBuilder = TrainingBuilder.Create();

var eventStoreOption = new Option<DirectoryInfo>("--event-store")
{
    Description = "Configures a folder for JSON event store."
};

var requiredEventStoreOption = new Option<DirectoryInfo>("--event-store")
{
    Description = "Configures a folder for JSON event store.",
    Required = true
};

var alertOption = new Option<AlertType>("--alert")
{
    Description = "Configures a type of alert to optimize",
    Required = true
};

alertOption.Action = new Action(result =>
{
    var requiredValue = result.GetRequiredValue(alertOption);

    switch (requiredValue)
    {
        case AlertType.AzureScheduledQueryRule:
            trainingBuilder = trainingBuilder.WithAzureScheduledQueryRuleAlert();
            break;
        default:
            return 1;
    }

    return 0;
});

requiredEventStoreOption.Action = new Action(result =>
{
    var directoryInfo = result.GetValue(requiredEventStoreOption);

    if (directoryInfo != null)
    {
        trainingBuilder = trainingBuilder.WithCustomEventStore(new JsonFileEventStore(directoryInfo.FullName));
    }

    return 0;
});

eventStoreOption.Action = new Action(result =>
{
    var directoryInfo = result.GetValue(eventStoreOption);

    if (directoryInfo != null)
    {
        trainingBuilder = trainingBuilder.WithCustomEventStore(new JsonFileEventStore(directoryInfo.FullName));
    }

    return 0;
});

var csvOutages = new Option<FileInfo>("--csv-outages")
{
    Description = "Sets known outages from CSV file",
    Required = true
};

csvOutages.Action = new TaskAction(async result =>
{
    var requiredValue = result.GetRequiredValue(csvOutages);

    trainingBuilder = trainingBuilder.WithCsvOutagesProvider(requiredValue.FullName);

    var validationResult = await trainingBuilder.KnownOutagesProvider!.ImportAndValidate();

    if (validationResult.IsValid)
    {
        return 0;
    }

    Console.WriteLine($"Invalid Outages CSV file!");
    foreach (var validationResultError in validationResult.Errors)
    {
        Console.WriteLine(validationResultError.ErrorMessage);
    }

    return 1;

});

var csvTimeSeries = new Option<FileInfo>("--csv-time-series")
{
    Description = "Sets time-series from CSV file",
    Required = true,
};

csvTimeSeries.Action = new TaskAction(async result =>
{
    var requiredValue = result.GetRequiredValue(csvTimeSeries);

    trainingBuilder = trainingBuilder.WithCsvTimeSeriesProvider(requiredValue.FullName);

    var validationResult = await trainingBuilder.TimeSeriesProvider!.ImportAndValidate();

    if (validationResult.IsValid)
    {
        return 0;
    }

    Console.WriteLine($"Invalid Time-Series CSV file!");
    foreach (var validationResultError in validationResult.Errors)
    {
        Console.WriteLine(validationResultError.ErrorMessage);
    }

    return 1;

});


var requiredIdOption = new Option<string>("--id")
{
    Description = "Id or unique start of an Id of training from Event Store."
};

var runCommand = new Command("run")
{
    Description = "Runs existing training session from event store",
    Options =
    {
        requiredEventStoreOption,
        requiredIdOption
    }
};

runCommand.SetAction(Run);

var listCommand = new Command("list")
{
    Description = "Lists existing training sessions in the eventStore",
    Options =
    {
        requiredEventStoreOption
    }
};

listCommand.SetAction(ListExisting);

var createCommand = new Command("create")
{
    Description = "Creates and starts new optimization",
    Options =
    {
        eventStoreOption,
        csvOutages,
        csvTimeSeries,
        alertOption
    }
};

createCommand.SetAction(CreateAndRun);


var rootCommand = new RootCommand
{
    Description = "Command line interface that performs same tasks as Alerting.ML Desktop app",
    Subcommands =
    {
        runCommand,
        listCommand,
        createCommand
    }
};

var parseResult = rootCommand.Parse(args);

await parseResult.InvokeAsync();

if (trainingBuilder.EventStore is IDisposable disposableStore)
{
    disposableStore.Dispose();
}

async Task CreateAndRun(ParseResult obj)
{
    var geneticOptimizer = trainingBuilder.Build();

    var fullSummary = new TrainingFullSummary();
    
    foreach (var @event in geneticOptimizer.Optimize(OptimizationConfiguration.Default, CancellationToken.None))
    {
        fullSummary.Apply(@event);
    }

    Console.WriteLine(fullSummary.ToString());
}

async Task Run(ParseResult obj)
{
    var eventStore = trainingBuilder.EventStore!;

    var id = obj.GetRequiredValue(requiredIdOption);

    var aggregateId = await eventStore.GetExistingAggregates().Where(guid => guid.ToString().StartsWith(id)).SingleAsync();

    var initializationEvent = await eventStore.GetAll(aggregateId, CancellationToken.None).FirstAsync();

    if (initializationEvent.GetType().GetGenericTypeDefinition() != typeof(StateInitializedEvent<>))
    {
        Console.WriteLine($"Aggregate {aggregateId} has broken event sequence. Unable to find first initialization event.");
    }

    var configurationType = initializationEvent.GetType().GetGenericArguments()[0];

    var geneticOptimizer = trainingBuilder.CreateEmpty(configurationType);

    var fullSummary = new TrainingFullSummary();

    await foreach (var @event in geneticOptimizer.Hydrate(aggregateId))
    {
        fullSummary.Apply(@event);
    }

    foreach (var @event in geneticOptimizer.Optimize(OptimizationConfiguration.Default, CancellationToken.None))
    {
        fullSummary.Apply(@event);
    }

    Console.WriteLine(fullSummary.ToString());
}

async Task ListExisting(ParseResult obj)
{
    var eventStore = trainingBuilder.EventStore!;

    var summaries = new List<TrainingShortSummary>();

    await foreach (var existingAggregate in eventStore.GetExistingAggregates())
    {
        var summary = new TrainingShortSummary();
        await foreach (var @event in eventStore.GetAll(existingAggregate, CancellationToken.None))
        {
            summary.Apply(@event);
        }
        summaries.Add(summary);
    }

    if (summaries.Count == 0)
    {
        Console.WriteLine("Event store is empty!");
    }

    foreach (var summary in summaries)
    {
        Console.WriteLine(summary.ToString());
    }
}