using Alerting.ML.Engine;

namespace Alerting.ML.Sources.Azure;

/// <summary>
/// Contains Azure-specific extensions to <see cref="TrainingBuilder"/>
/// </summary>
public static class TrainingBuilderExtensions
{
    /// <param name="builder"><see cref="TrainingBuilder"/> to modify.</param>
    extension(TrainingBuilder builder)
    {
        /// <summary>
        /// Configures builder to use <see cref="ScheduledQueryRuleAlert"/>
        /// </summary>
        /// <returns>A new instance of TrainingBuilder.</returns>
        public TrainingBuilder WithAzureScheduledQueryRuleAlert()
        {
            return builder.WithAlert(new ScheduledQueryRuleAlert());
        }
    }
}