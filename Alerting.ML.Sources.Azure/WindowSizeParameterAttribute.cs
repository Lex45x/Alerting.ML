using Alerting.ML.Engine.Alert;

namespace Alerting.ML.Sources.Azure;

internal class WindowSizeParameterAttribute : OneOfParameterAttribute<TimeSpan>
{
    protected override IReadOnlyList<TimeSpan> AllowedValues { get; } = new List<TimeSpan>
    {
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(10),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromMinutes(30),
        TimeSpan.FromMinutes(45),
        TimeSpan.FromHours(1),
        TimeSpan.FromHours(2),
        TimeSpan.FromHours(3),
        TimeSpan.FromHours(4),
        TimeSpan.FromHours(5),
        TimeSpan.FromHours(6),
        TimeSpan.FromDays(1),
        TimeSpan.FromDays(2)
    };
}