using Alerting.ML.Engine;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Sources.Azure;

/// <summary>
///     Contains Azure-specific extensions to <see cref="TrainingBuilder" />
/// </summary>
public static class ConfigurationExtensions
{
    /// <param name="builder"><see cref="TrainingBuilder" /> to modify.</param>
    extension(TrainingBuilder builder)
    {
        /// <summary>
        ///     Configures builder to use <see cref="ScheduledQueryRuleAlert" />
        /// </summary>
        /// <returns>A new instance of TrainingBuilder.</returns>
        public TrainingBuilder WithAzureScheduledQueryRuleAlert()
        {
            return builder.WithAlert(new ScheduledQueryRuleAlert());
        }
    }

    /// <param name="registry">Registry of known configuration types.</param>
    extension(IConfigurationTypeRegistry registry)
    {
        /// <summary>
        /// Registers Azure-specific types for polymorphic deserialization. 
        /// </summary>
        /// <returns></returns>
        public IConfigurationTypeRegistry WithAzureTypes()
        {
            registry.RegisterAlertType<ScheduledQueryRuleAlert>();
            registry.RegisterConfigurationType<ScheduledQueryRuleConfiguration>();
            return registry;
        }
    }
}