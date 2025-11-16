using System.Numerics;

namespace Alerting.ML.Engine.Data;

public interface ITimeSeriesProvider
{
    Metric[] GetTimeSeries();
}