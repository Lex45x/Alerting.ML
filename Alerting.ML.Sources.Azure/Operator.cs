namespace Alerting.ML.Sources.Azure;

/// <summary>
/// Corresponds to Azure Scheduled Query Rule configuration. See: <a href="https://learn.microsoft.com/en-us/azure/templates/microsoft.insights/scheduledqueryrules?pivots=deployment-language-bicep#condition">Azure Scheduled Query Rule Bicep</a>
/// </summary>
public enum Operator
{
    // Self-explanatory enum member names.
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    Equals = 0,
    GreaterThan = 1,
    GreaterThanOrEqual = 2,
    LessThan = 3,
    LessThanOrEqual = 4,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}