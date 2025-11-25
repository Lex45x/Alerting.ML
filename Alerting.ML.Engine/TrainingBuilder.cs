using System.Reflection;
using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Scoring;
using Microsoft.Extensions.Logging;

namespace Alerting.ML.Engine;

public class TrainingBuilder(
    ILoggerFactory loggerFactory,
    ITimeSeriesProvider? timeSeriesProvider = null,
    IKnownOutagesProvider? knownOutagesProvider = null,
    IAlertScoreCalculator? alertScoreCalculator = null,
    IConfigurationFactory? configurationFactory = null,
    IAlert? alert = null,
    Type? alertConfigurationType = null)
{
    private static readonly MethodInfo GenericBuildInfo;

    static TrainingBuilder()
    {
        GenericBuildInfo =
            typeof(TrainingBuilder).GetMethod("GenericBuild", BindingFlags.Instance | BindingFlags.NonPublic, []) ??
            throw new InvalidOperationException("Unable to find generic build private method.");
    }

    public TrainingBuilder WithTimeSeriesProvider(ITimeSeriesProvider provider)
    {
        return new TrainingBuilder(loggerFactory, provider, knownOutagesProvider, alertScoreCalculator,
            configurationFactory, alert, alertConfigurationType);
    }

    public TrainingBuilder WithAlert<T>(IAlert<T> alert) where T : AlertConfiguration<T>
    {
        CheckConfigurationType(typeof(T));

        return new TrainingBuilder(loggerFactory, timeSeriesProvider, knownOutagesProvider, alertScoreCalculator,
            configurationFactory, alert, typeof(T));
    }

    public TrainingBuilder WithKnownOutagesProvider(IKnownOutagesProvider provider)
    {
        return new TrainingBuilder(loggerFactory, timeSeriesProvider, provider, alertScoreCalculator,
            configurationFactory, alert, alertConfigurationType);
    }

    public TrainingBuilder WithCustomAlertScoreCalculator(IAlertScoreCalculator calculator)
    {
        return new TrainingBuilder(loggerFactory, timeSeriesProvider, knownOutagesProvider, calculator,
            configurationFactory, alert, alertConfigurationType);
    }

    public TrainingBuilder WithCustomConfigurationFactory<T>(IConfigurationFactory<T> factory)
        where T : AlertConfiguration<T>
    {
        CheckConfigurationType(typeof(T));
        return new TrainingBuilder(loggerFactory, timeSeriesProvider, knownOutagesProvider, alertScoreCalculator,
            factory, alert, typeof(T));
    }

    // ReSharper disable once UnusedMember.Local
    // Used via Reflection
    private GeneticOptimizer<T> GenericBuild<T>() where T : AlertConfiguration<T>, new()
    {
        return new GeneticOptimizer<T>(alert as IAlert<T> ?? throw new InvalidOperationException(),
            timeSeriesProvider ?? throw new InvalidOperationException(),
            knownOutagesProvider ?? throw new InvalidOperationException(),
            alertScoreCalculator ?? new DefaultAlertScoreCalculator(),
            configurationFactory as IConfigurationFactory<T> ?? new DefaultConfigurationFactory<T>(),
            loggerFactory.CreateLogger<GeneticOptimizer<T>>());
    }

    private void CheckConfigurationType(Type incomingType)
    {
        if (alertConfigurationType == null)
        {
            return;
        }

        if (alertConfigurationType == incomingType)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Current TrainingBuilder is already configured to work with {alertConfigurationType}. Create a new builder for {incomingType}.");
    }

    public IGeneticOptimizer Build()
    {
        var configurationType = alertConfigurationType
                                ?? throw new ArgumentNullException(nameof(alertConfigurationType));

        return (IGeneticOptimizer)GenericBuildInfo.MakeGenericMethod(configurationType).Invoke(this, [])!;
    }
}