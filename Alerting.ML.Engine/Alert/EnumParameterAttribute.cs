namespace Alerting.ML.Engine.Alert;

public sealed class EnumParameterAttribute<TEnum> : OneOfParameterAttribute<TEnum> where TEnum : struct, Enum
{
    protected override IReadOnlyList<TEnum> AllowedValues { get; } = Enum.GetValues<TEnum>();
}