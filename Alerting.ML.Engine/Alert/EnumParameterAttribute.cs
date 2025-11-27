namespace Alerting.ML.Engine.Alert;

/// <summary>
/// Defines an Enum-type property with possible values being values of the enum.
/// </summary>
/// <typeparam name="TEnum"></typeparam>
public sealed class EnumParameterAttribute<TEnum> : OneOfParameterAttribute<TEnum> where TEnum : struct, Enum
{
    /// <inheritdoc />
    protected override IReadOnlyList<TEnum> AllowedValues { get; } = Enum.GetValues<TEnum>();
}