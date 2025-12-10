using System.Reflection;
using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine;

/// <summary>
///     Enables fluent API for creating GeneticOptimizer. Calling public methods that return <see cref="TrainingBuilder" />
///     will create a new instance with applied change.
/// </summary>
public class TrainingBuilder
{
    private static readonly MethodInfo GenericBuildInfo;
    private static readonly MethodInfo GenericBuildEmptyInfo;
    private readonly IConfigurationTypeRegistry typeRegistry;

    static TrainingBuilder()
    {
        GenericBuildInfo =
            typeof(TrainingBuilder).GetMethod("GenericBuild", BindingFlags.Instance | BindingFlags.NonPublic, []) ??
            throw new InvalidOperationException("Unable to find generic build private method.");

        GenericBuildEmptyInfo =
            typeof(TrainingBuilder).GetMethod("GenericBuildEmpty", BindingFlags.Instance | BindingFlags.NonPublic,
                []) ??
            throw new InvalidOperationException("Unable to find generic build private method.");
    }

    /// <summary>
    ///     Enables fluent API for creating GeneticOptimizer. Calling public methods that return <see cref="TrainingBuilder" />
    ///     will create a new instance with applied change.
    /// </summary>
    /// <param name="timeSeriesProvider">Source of the time series.</param>
    /// <param name="knownOutagesProvider">Source of known outages.</param>
    /// <param name="alertScoreCalculator">Defines alert score calculation. </param>
    /// <param name="configurationFactory">Allows manipulation with <paramref name="alertConfigurationType" /></param>
    /// <param name="alert">Alert to be evaluated.</param>
    /// <param name="alertConfigurationType">Underlying configuration type.</param>
    /// <param name="eventStore"></param>
    /// <param name="typeRegistry"></param>
    private TrainingBuilder(ITimeSeriesProvider? timeSeriesProvider,
        IKnownOutagesProvider? knownOutagesProvider,
        IAlertScoreCalculator? alertScoreCalculator,
        IConfigurationFactory? configurationFactory,
        IAlert? alert,
        Type? alertConfigurationType,
        IEventStore? eventStore, IConfigurationTypeRegistry? typeRegistry)
    {
        this.typeRegistry = typeRegistry ?? KnownTypeInfoResolver.Instance;
        EventStore = eventStore;
        TimeSeriesProvider = timeSeriesProvider;
        KnownOutagesProvider = knownOutagesProvider;
        AlertScoreCalculator = alertScoreCalculator;
        ConfigurationFactory = configurationFactory;
        Alert = alert;
        AlertConfigurationType = alertConfigurationType;
    }

    /// <summary>
    ///     Event store used to save optimization progress.
    /// </summary>
    public IEventStore? EventStore { get; }

    /// <summary>
    ///     Source of the time series.
    /// </summary>
    public ITimeSeriesProvider? TimeSeriesProvider { get; }

    /// <summary>
    ///     Source of known outages.
    /// </summary>
    public IKnownOutagesProvider? KnownOutagesProvider { get; }

    /// <summary>
    ///     Defines alert score calculation.
    /// </summary>
    public IAlertScoreCalculator? AlertScoreCalculator { get; }

    /// <summary>
    ///     Allows manipulation with <see cref="AlertConfiguration" />
    /// </summary>
    public IConfigurationFactory? ConfigurationFactory { get; }

    /// <summary>
    ///     Alert to be evaluated.
    /// </summary>
    public IAlert? Alert { get; }

    /// <summary>
    ///     Configuration type used by <see cref="IAlert" />
    /// </summary>
    public Type? AlertConfigurationType { get; }

    /// <summary>
    ///     Creates an uninitialized instance of <see cref="TrainingBuilder" />
    /// </summary>
    /// <returns></returns>
    public static TrainingBuilder Create()
    {
        return new TrainingBuilder(timeSeriesProvider: null, knownOutagesProvider: null, alertScoreCalculator: null,
            configurationFactory: null, alert: null, alertConfigurationType: null, eventStore: null,
            typeRegistry: null);
    }

    /// <summary>
    ///     Configures <see cref="TrainingBuilder" /> with <paramref name="provider" />
    /// </summary>
    /// <returns>New instance of TrainingBuilder with updated value.</returns>
    public TrainingBuilder WithTimeSeriesProvider(ITimeSeriesProvider provider)
    {
        return new TrainingBuilder(provider, KnownOutagesProvider, AlertScoreCalculator,
            ConfigurationFactory, Alert, AlertConfigurationType, EventStore, typeRegistry);
    }

    /// <summary>
    ///     Configures <see cref="TrainingBuilder" /> with <paramref name="alertInstance" />
    /// </summary>
    /// <returns>New instance of TrainingBuilder with updated value.</returns>
    public TrainingBuilder WithAlert<T>(IAlert<T> alertInstance) where T : AlertConfiguration
    {
        typeRegistry.RegisterConfigurationType<T>();
        typeRegistry.RegisterAlertType(alertInstance.GetType());
        CheckConfigurationType(typeof(T));

        return new TrainingBuilder(TimeSeriesProvider, KnownOutagesProvider, AlertScoreCalculator,
            ConfigurationFactory, alertInstance, typeof(T), EventStore, typeRegistry);
    }

    /// <summary>
    ///     Configures <see cref="TrainingBuilder" /> with <paramref name="provider" />
    /// </summary>
    /// <returns>New instance of TrainingBuilder with updated value.</returns>
    public TrainingBuilder WithKnownOutagesProvider(IKnownOutagesProvider provider)
    {
        return new TrainingBuilder(TimeSeriesProvider, provider, AlertScoreCalculator,
            ConfigurationFactory, Alert, AlertConfigurationType, EventStore, typeRegistry);
    }

    /// <summary>
    ///     Overrides <see cref="TrainingBuilder" /> with <paramref name="calculator" /> instead of
    ///     <see cref="DefaultAlertScoreCalculator" />
    /// </summary>
    /// <returns>New instance of TrainingBuilder with updated value.</returns>
    public TrainingBuilder WithCustomAlertScoreCalculator<T>(T calculator) where T : IAlertScoreCalculator
    {
        typeRegistry.RegisterScoreCalculatorType<T>();
        return new TrainingBuilder(TimeSeriesProvider, KnownOutagesProvider, calculator,
            ConfigurationFactory, Alert, AlertConfigurationType, EventStore, typeRegistry);
    }

    /// <summary>
    ///     Overrides <see cref="TrainingBuilder" /> with <paramref name="factory" /> instead of
    ///     <see cref="DefaultConfigurationFactory{T}" />
    /// </summary>
    /// <returns>New instance of TrainingBuilder with updated value.</returns>
    public TrainingBuilder WithCustomConfigurationFactory<T>(IConfigurationFactory<T> factory)
        where T : AlertConfiguration
    {
        typeRegistry.RegisterConfigurationType<T>();
        typeRegistry.RegisterConfigurationFactoryType(factory.GetType());
        CheckConfigurationType(typeof(T));
        return new TrainingBuilder(TimeSeriesProvider, KnownOutagesProvider, AlertScoreCalculator,
            factory, Alert, typeof(T), EventStore, typeRegistry);
    }

    /// <summary>
    ///     Overrides <see cref="TrainingBuilder" /> with <paramref name="eventStore" /> instead of
    ///     <see cref="InMemoryEventStore" />
    /// </summary>
    /// <returns>New instance of TrainingBuilder with updated value.</returns>
    public TrainingBuilder WithCustomEventStore(IEventStore eventStore)
    {
        return new TrainingBuilder(TimeSeriesProvider, KnownOutagesProvider, AlertScoreCalculator,
            ConfigurationFactory, Alert, AlertConfigurationType, eventStore, typeRegistry);
    }

    // ReSharper disable once UnusedMember.Local
    // Used via Reflection
    private IGeneticOptimizer GenericBuild<T>() where T : AlertConfiguration, new()
    {
        return new GeneticOptimizerStateMachine<T>(Alert as IAlert<T> ?? throw new ArgumentNullException(nameof(Alert)),
            TimeSeriesProvider?.GetTimeSeries() ?? throw new ArgumentNullException(nameof(TimeSeriesProvider)),
            KnownOutagesProvider?.GetKnownOutages() ?? throw new ArgumentNullException(nameof(KnownOutagesProvider)),
            AlertScoreCalculator ?? new DefaultAlertScoreCalculator(),
            ConfigurationFactory as IConfigurationFactory<T> ?? new DefaultConfigurationFactory<T>(TimeSeriesProvider.Statistics),
            EventStore ?? new InMemoryEventStore());
    }

    // ReSharper disable once UnusedMember.Local
    // Used via Reflection
    private IGeneticOptimizer GenericBuildEmpty<T>() where T : AlertConfiguration, new()
    {
        return new GeneticOptimizerStateMachine<T>(EventStore ?? new InMemoryEventStore());
    }

    private void CheckConfigurationType(Type incomingType)
    {
        if (AlertConfigurationType == null)
        {
            return;
        }

        if (AlertConfigurationType == incomingType)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Current TrainingBuilder is already configured to work with {AlertConfigurationType}. Create a new builder for {incomingType}.");
    }

    /// <summary>
    ///     Creates a new <see cref="IGeneticOptimizer" /> instance according to configured values.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">
    ///     Indicates that one of the mandatory properties (<see cref="Alert" />,
    ///     <see cref="TimeSeriesProvider" />, <see cref="KnownOutagesProvider" />) was not configured.
    /// </exception>
    public IGeneticOptimizer Build()
    {
        var configurationType = AlertConfigurationType
                                ?? throw new ArgumentNullException(nameof(AlertConfigurationType));

        return (IGeneticOptimizer)GenericBuildInfo.MakeGenericMethod(configurationType).Invoke(this, [])!;
    }

    /// <summary>
    ///     Creates a new <see cref="IGeneticOptimizer" /> instance without any configuration;
    /// </summary>
    /// <param name="configurationType"></param>
    /// <returns></returns>
    public IGeneticOptimizer CreateEmpty(Type configurationType)
    {
        return (IGeneticOptimizer)GenericBuildEmptyInfo.MakeGenericMethod(configurationType).Invoke(this, [])!;
    }
}