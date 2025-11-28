using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Scoring;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine;

/// <summary>
/// Enables fluent API for creating GeneticOptimizer. Calling public methods that return <see cref="TrainingBuilder"/> will create a new instance with applied change.
/// </summary>
/// <param name="timeSeriesProvider">Source of the time series.</param>
/// <param name="knownOutagesProvider">Source of known outages.</param>
/// <param name="alertScoreCalculator">Defines alert score calculation. </param>
/// <param name="configurationFactory">Allows manipulation with <paramref name="alertConfigurationType"/></param>
/// <param name="alert">Alert to be evaluated.</param>
/// <param name="alertConfigurationType">Underlying configuration type.</param>
public class TrainingBuilder(
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

    /// <summary>
    /// Source of the time series.
    /// </summary>
    public ITimeSeriesProvider? TimeSeriesProvider => timeSeriesProvider;

    /// <summary>
    /// Source of known outages.
    /// </summary>
    public IKnownOutagesProvider? KnownOutagesProvider => knownOutagesProvider;

    /// <summary>
    /// Defines alert score calculation.
    /// </summary>
    public IAlertScoreCalculator? AlertScoreCalculator => alertScoreCalculator;

    /// <summary>
    /// Allows manipulation with <see cref="AlertConfiguration"/>
    /// </summary>
    public IConfigurationFactory? ConfigurationFactory => configurationFactory;

    /// <summary>
    /// Alert to be evaluated.
    /// </summary>
    public IAlert? Alert => alert;

    /// <summary>
    /// Configures <see cref="TrainingBuilder"/> with <paramref name="provider"/>
    /// </summary>
    /// <returns>New instance of TrainingBuilder with updated value.</returns>
    public TrainingBuilder WithTimeSeriesProvider(ITimeSeriesProvider provider)
    {
        return new TrainingBuilder(provider, KnownOutagesProvider, AlertScoreCalculator,
            ConfigurationFactory, Alert, alertConfigurationType);
    }

    /// <summary>
    /// Configures <see cref="TrainingBuilder"/> with <paramref name="alertInstance"/>
    /// </summary>
    /// <returns>New instance of TrainingBuilder with updated value.</returns>
    public TrainingBuilder WithAlert<T>(IAlert<T> alertInstance) where T : AlertConfiguration
    {
        CheckConfigurationType(typeof(T));

        return new TrainingBuilder(TimeSeriesProvider, KnownOutagesProvider, AlertScoreCalculator,
            ConfigurationFactory, alertInstance, typeof(T));
    }

    /// <summary>
    /// Configures <see cref="TrainingBuilder"/> with <paramref name="provider"/>
    /// </summary>
    /// <returns>New instance of TrainingBuilder with updated value.</returns>
    public TrainingBuilder WithKnownOutagesProvider(IKnownOutagesProvider provider)
    {
        return new TrainingBuilder(TimeSeriesProvider, provider, AlertScoreCalculator,
            ConfigurationFactory, Alert, alertConfigurationType);
    }

    /// <summary>
    /// Overrides <see cref="TrainingBuilder"/> with <paramref name="calculator"/> instead of <see cref="DefaultAlertScoreCalculator"/>
    /// </summary>
    /// <returns>New instance of TrainingBuilder with updated value.</returns>
    public TrainingBuilder WithCustomAlertScoreCalculator(IAlertScoreCalculator calculator)
    {
        return new TrainingBuilder(TimeSeriesProvider, KnownOutagesProvider, calculator,
            ConfigurationFactory, Alert, alertConfigurationType);
    }

    /// <summary>
    /// Overrides <see cref="TrainingBuilder"/> with <paramref name="factory"/> instead of <see cref="DefaultConfigurationFactory{T}"/>
    /// </summary>
    /// <returns>New instance of TrainingBuilder with updated value.</returns>
    public TrainingBuilder WithCustomConfigurationFactory<T>(IConfigurationFactory<T> factory)
        where T : AlertConfiguration
    {
        CheckConfigurationType(typeof(T));
        return new TrainingBuilder(TimeSeriesProvider, KnownOutagesProvider, AlertScoreCalculator,
            factory, Alert, typeof(T));
    }

    // ReSharper disable once UnusedMember.Local
    // Used via Reflection
    private IGeneticOptimizer GenericBuild<T>() where T : AlertConfiguration, new()
    {
        return new GeneticOptimizerStateMachine<T>(Alert as IAlert<T> ?? throw new ArgumentNullException(nameof(Alert)),
            TimeSeriesProvider ?? throw new ArgumentNullException(nameof(TimeSeriesProvider)),
            KnownOutagesProvider ?? throw new ArgumentNullException(nameof(KnownOutagesProvider)),
            AlertScoreCalculator ?? new DefaultAlertScoreCalculator(),
            ConfigurationFactory as IConfigurationFactory<T> ?? new DefaultConfigurationFactory<T>(), new InMemoryEventStore(), OptimizationConfiguration.Default);
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

    /// <summary>
    /// Creates a new <see cref="IGeneticOptimizer"/> instance according to configured values.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">Indicates that one of the mandatory properties (<see cref="Alert"/>, <see cref="TimeSeriesProvider"/>, <see cref="KnownOutagesProvider"/>) was not configured.</exception>
    public IGeneticOptimizer Build()
    {
        var configurationType = alertConfigurationType
                                ?? throw new ArgumentNullException(nameof(alertConfigurationType));

        return (IGeneticOptimizer)GenericBuildInfo.MakeGenericMethod(configurationType).Invoke(this, [])!;
    }
}