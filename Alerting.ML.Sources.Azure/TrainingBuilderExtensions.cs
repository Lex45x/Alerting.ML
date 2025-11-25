using Alerting.ML.Engine;

namespace Alerting.ML.Sources.Azure;

public static class TrainingBuilderExtensions
{
    extension(TrainingBuilder builder)
    {
        public TrainingBuilder WithAzureScheduledQueryRuleAlert()
        {
            return builder.WithAlert(new ScheduledQueryRuleAlert());
        }
    }
}