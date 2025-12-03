using Alerting.ML.Engine.Alert;

namespace Alerting.ML.Sources.Azure;

internal class EvaluationFrequencyParameterAttribute : OneOfParameterAttribute<TimeSpan>
{
    protected override IReadOnlyList<TimeSpan> AllowedValues { get; } = new List<TimeSpan>
    {
        TimeSpan.FromMinutes(minutes: 1),
        TimeSpan.FromMinutes(minutes: 5),
        TimeSpan.FromMinutes(minutes: 10),
        TimeSpan.FromMinutes(minutes: 15),
        TimeSpan.FromMinutes(minutes: 30),
        TimeSpan.FromMinutes(minutes: 45),
        TimeSpan.FromHours(hours: 1),
        TimeSpan.FromHours(hours: 2),
        TimeSpan.FromHours(hours: 3),
        TimeSpan.FromHours(hours: 4),
        TimeSpan.FromHours(hours: 5),
        TimeSpan.FromHours(hours: 6),
        TimeSpan.FromDays(days: 1)
    };
}