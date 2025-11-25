using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Sources.Azure;
using Alerting.ML.TimeSeries.Sample;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Metric = Alerting.ML.Engine.Data.Metric;

namespace Alerting.ML.App.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IKnownOutagesProvider knownOutagesProvider;
    private readonly ITimeSeriesProvider timeSeriesProvider;
    private double DefaultMin;
    private double DefaultMax;
    private bool _isDown;
    private double minXThumb;
    private double maxXThumb;
    private readonly List<Metric> metrics;
    private ObservableCollection<DateTimePoint> zoomedMetricSeries = new();
    private ObservableCollection<GenerationSummary> generationStatistics = new();
    private ObservableCollection<IChartElement> zoomedOutageSections = new();
    private readonly IGeneticOptimizer geneticOptimizer;
    private GenerationSummary selectedGeneration;
    private ObservableCollection<string> simulationLog = new();

    public MainViewModel()
    {
        var loggerFactory = LoggerFactory.Create(builder => { builder.AddProvider(new CallbackLoggerProvider(OnLog)); });

        knownOutagesProvider = new SampleOutagesProvider();
        timeSeriesProvider = new SampleTimeSeriesProvider(knownOutagesProvider);
        metrics = timeSeriesProvider.GetTimeSeries().ToArray().ToList();

        geneticOptimizer = new GeneticOptimizer<ScheduledQueryRuleConfiguration>(new ScheduledQueryRuleAlert(),
            timeSeriesProvider, knownOutagesProvider, new DefaultAlertScoreCalculator(),
            new DefaultConfigurationFactory<ScheduledQueryRuleConfiguration>(), loggerFactory.CreateLogger<GeneticOptimizer<ScheduledQueryRuleConfiguration>>());

        var reducedSeries = metrics.Scale(2_000).ToList();

        AveragedMetricSeries =
            reducedSeries.Select(metric => new DateTimePoint(metric.Timestamp, metric.Value)).ToList();

        DefaultMin = reducedSeries[0].Timestamp.Ticks;
        DefaultMax = reducedSeries[0].Timestamp.AddDays(10).Ticks;

        maxXThumb = DefaultMax;
        minXThumb = DefaultMin;

        ResetLowerZoom();

        var chartSections = knownOutagesProvider.GetKnownOutages().Select(outage => new RectangularSection
        {
            Xi = outage.StartTime.Ticks,
            Xj = outage.EndTime.Ticks,
            Fill = new SolidColorPaint(SKColors.Red)
        }).Cast<IChartElement>().ToList();

        var currentSelection = new XamlRectangularSection();

        currentSelection.Fill = new SolidColorPaint(SKColors.DimGray);
        currentSelection.Bind(XamlRectangularSection.XiProperty, new Binding(nameof(MinXThumb)));
        currentSelection.Bind(XamlRectangularSection.XjProperty, new Binding(nameof(MaxXThumb)));

        chartSections.Add(currentSelection);

        OutageSections = chartSections;

        PointerDownCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerDown);
        PointerUpCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerUp);
        PointerMoveCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerMove);
        PointerWheelCommand = ReactiveCommand.Create<PointerWheelEventArgs>(PointerWheel);
        RunSimulationCommand = ReactiveCommand.CreateFromTask(RunSimulation);
        SelectGenerationCommand = ReactiveCommand.Create<ChartPoint>(SelectGeneration);

        this.WhenAnyValue(model => model.MinXThumb, model => model.MaxXThumb)
            .Buffer(TimeSpan.FromMilliseconds(100))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(d => ResetLowerZoom());

        GenerationStatistics
            .ToObservableChangeSet()
            .Subscribe(set => { this.RaisePropertyChanged(nameof(GenerationBest)); });


    }

    private void OnLog(string category, LogLevel logLevel, EventId eventId, object state, Exception? exception, Func<object, Exception?, string> formatter)
    {
        SimulationLog.Add($"[{logLevel}][{category}]: {formatter(state, exception)}");
    }


    public ReactiveCommand<ChartPoint, Unit> SelectGenerationCommand { get; set; }

    private void SelectGeneration(ChartPoint selectedPoint)
    {
        if (selectedPoint == null)
        {
            return;
        }

        SelectedGeneration = GenerationStatistics[selectedPoint.Index];
    }

    private async Task RunSimulation(CancellationToken token)
    {
        await Task.Run(async () =>
        {
            foreach (var summary in geneticOptimizer.Optimize(new OptimizationConfiguration(100, 0.1, 0.3, 100,
                         new AlertScoreConfiguration(0.9, TimeSpan.FromMinutes(5), AlertScorePriority.Precision), 5)))
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    GenerationStatistics.Add(summary);
                    this.RaisePropertyChanged(nameof(GenerationBestScores));
                });
            }
        }, token);
    }

    private void ResetLowerZoom()
    {
        ZoomedMetricSeries = new ObservableCollection<DateTimePoint>(metrics
            .SkipWhile(metric => metric.Timestamp.Ticks < MinXThumb)
            .TakeWhile(metric => metric.Timestamp.Ticks < MaxXThumb)
            .ToList()
            .Scale(500)
            .Select(metric => new DateTimePoint(metric.Timestamp, metric.Value)));

        ZoomedOutageSections = new ObservableCollection<IChartElement>(knownOutagesProvider
            .GetKnownOutages()
            .SkipWhile(outage => outage.EndTime.Ticks < MinXThumb)
            .TakeWhile(outage => outage.StartTime.Ticks < MaxXThumb)
            .Select(outage => new RectangularSection
            {
                Xi = outage.StartTime.Ticks,
                Xj = outage.EndTime.Ticks,
                Fill = new SolidColorPaint(SKColors.Red)
            }));
    }

    public ObservableCollection<string> SimulationLog
    {
        get => simulationLog;
        set => this.RaiseAndSetIfChanged(ref simulationLog, value);
    }

    private void PointerWheel(PointerWheelEventArgs obj)
    {
        var zoomChange = obj.Delta.Y * TimeSpan.FromHours(6).Ticks;

        if (zoomChange < 0 && MaxXThumb - MinXThumb - Math.Abs(2 * zoomChange) < Math.Abs(zoomChange * 2))
        {
            return;
        }

        MinXThumb -= zoomChange;
        MaxXThumb += zoomChange;
    }

    public IReadOnlyList<DateTimePoint> AveragedMetricSeries { get; }

    public GenerationSummary SelectedGeneration
    {
        get => selectedGeneration;
        set
        {
            this.RaiseAndSetIfChanged(ref selectedGeneration, value);
            this.RaisePropertyChanged(nameof(GenerationPrecisionDistribution));
            this.RaisePropertyChanged(nameof(GenerationDetectionLatencyDistribution));
            this.RaisePropertyChanged(nameof(GenerationFalseNegativeRateDistribution));
            this.RaisePropertyChanged(nameof(GenerationOutageCountDistribution));
            this.RaisePropertyChanged(nameof(GenerationScoreDistribution));
        }
    }

    public ObservableCollection<DateTimePoint> ZoomedMetricSeries
    {
        get => zoomedMetricSeries;
        set => this.RaiseAndSetIfChanged(ref zoomedMetricSeries, value);
    }

    public ObservableCollection<GenerationSummary> GenerationStatistics
    {
        get => generationStatistics;
        set => this.RaiseAndSetIfChanged(ref generationStatistics, value);
    }

    public IReadOnlyList<ObservablePoint> GenerationBest => GenerationStatistics.Select(summary => new ObservablePoint
    {
        X = 1 - summary.Best.FalseNegativeRate,
        Y = summary.Best.Precision
    }).ToList();

    public IReadOnlyList<ObservablePoint> GenerationPrecisionDistribution => GenerationStatistics
        .FirstOrDefault(summary => summary.GenerationIndex == SelectedGeneration?.GenerationIndex)
        ?.PrecisionDistribution
        .Select(summary => new ObservablePoint
        {
            X = summary.Value,
            Y = summary.Count
        }).ToList() ?? Enumerable.Empty<ObservablePoint>().ToList();

    public IReadOnlyList<ObservablePoint> GenerationBestScores => GenerationStatistics
        .Select(summary => new ObservablePoint
        {
            X = summary.GenerationIndex,
            Y = summary.Best.Score
        }).ToList();

    public IReadOnlyList<TimeSpanPoint> GenerationDetectionLatencyDistribution => GenerationStatistics
        .FirstOrDefault(summary => summary.GenerationIndex == SelectedGeneration?.GenerationIndex)
        ?.DetectionLatencyDistribution
        .Select(summary => new TimeSpanPoint
        {
            TimeSpan = summary.Value,
            Value = summary.Count
        }).ToList() ?? Enumerable.Empty<TimeSpanPoint>().ToList();

    public IReadOnlyList<ObservablePoint> GenerationFalseNegativeRateDistribution => GenerationStatistics
        .FirstOrDefault(summary => summary.GenerationIndex == SelectedGeneration?.GenerationIndex)
        ?.FalseNegativeRateDistribution
        .Select(summary => new ObservablePoint
        {
            X = summary.Value,
            Y = summary.Count
        }).ToList() ?? Enumerable.Empty<ObservablePoint>().ToList();

    public IReadOnlyList<ObservablePoint> GenerationOutageCountDistribution => GenerationStatistics
        .FirstOrDefault(summary => summary.GenerationIndex == SelectedGeneration?.GenerationIndex)
        ?.OutageCountDistribution
        .Select(summary => new ObservablePoint
        {
            X = summary.Value,
            Y = summary.Count
        }).ToList() ?? Enumerable.Empty<ObservablePoint>().ToList();

    public IReadOnlyList<ObservablePoint> GenerationScoreDistribution => GenerationStatistics
        .FirstOrDefault(summary => summary.GenerationIndex == SelectedGeneration?.GenerationIndex)
        ?.ScoreDistribution
        .Select(summary => new ObservablePoint
        {
            X = summary.Value,
            Y = summary.Count
        }).ToList() ?? Enumerable.Empty<ObservablePoint>().ToList();

    public IReadOnlyList<IChartElement> OutageSections { get; }

    public ObservableCollection<IChartElement> ZoomedOutageSections
    {
        get => zoomedOutageSections;
        set => this.RaiseAndSetIfChanged(ref zoomedOutageSections, value);
    }

    public double MinXThumb
    {
        get => minXThumb;
        set => this.RaiseAndSetIfChanged(ref minXThumb, value);
    }

    public double MaxXThumb
    {
        get => maxXThumb;
        set => this.RaiseAndSetIfChanged(ref maxXThumb, value);
    }

    public ReactiveCommand<PointerCommandArgs, Unit> PointerDownCommand { get; }
    public ReactiveCommand<PointerCommandArgs, Unit> PointerUpCommand { get; }
    public ReactiveCommand<PointerCommandArgs, Unit> PointerMoveCommand { get; }
    public ReactiveCommand<PointerWheelEventArgs, Unit> PointerWheelCommand { get; }
    public ReactiveCommand<Unit, Unit> RunSimulationCommand { get; }

    public void PointerDown(PointerCommandArgs args) =>
        _isDown = true;

    public void PointerMove(PointerCommandArgs args)
    {
        if (!_isDown) return;

        var chart = (ICartesianChartView)args.Chart;
        var positionInData = chart.ScalePixelsToData(args.PointerPosition);

        var currentRange = MaxXThumb - MinXThumb;

        var min = positionInData.X - currentRange / 2;
        var max = positionInData.X + currentRange / 2;

        // optional, use the data bounds as limits for the thumb
        if (min < AveragedMetricSeries[0].DateTime.Ticks)
        {
            min = AveragedMetricSeries[0].DateTime.Ticks;
            max = min + currentRange;
        }

        if (max > AveragedMetricSeries[^1].DateTime.Ticks)
        {
            max = AveragedMetricSeries[^1].DateTime.Ticks;
            min = max - currentRange;
        }

        // update the scroll bar thumb when the user is dragging the chart
        MinXThumb = min;
        MaxXThumb = max;
    }

    public void PointerUp(PointerCommandArgs args) => _isDown = false;
}

internal static class EnumerableExtensions
{
    public static IEnumerable<Metric> Scale(this IReadOnlyList<Metric> input, int count)
    {
        if (input.Count <= count)
        {
            return input;
        }

        var chunkSize = input.Count / count + (input.Count % count > 0 ? 1 : 0);

        return input.Chunk(chunkSize).Select(metricsChunk =>
            new Metric(metricsChunk[metricsChunk.Length / 2].Timestamp,
                metricsChunk.Average(metric => metric.Value)));
    }
}



public class CallbackLoggerProvider : ILoggerProvider
{
    private readonly LogsSubscriber callback;

    public delegate void LogsSubscriber(string category, LogLevel logLevel, EventId eventId, object state,
        Exception? exception,
        Func<object, Exception?, string> formatter);
    public CallbackLoggerProvider(LogsSubscriber callback)
    {
        this.callback = callback;
    }

    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new ObservableCollectionLogger(categoryName, callback);
    }

    private class ObservableCollectionLogger : ILogger
    {
        private readonly string categoryName;
        private readonly LogsSubscriber logsSubscriber;
        private readonly List<object> stateCollection = new();
        public ObservableCollectionLogger(string categoryName, LogsSubscriber logsSubscriber)
        {
            this.categoryName = categoryName;
            this.logsSubscriber = logsSubscriber;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            logsSubscriber(categoryName, logLevel, eventId, state, exception, (o, ex) => formatter((TState)o, ex));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            stateCollection.Add(state);

            return new StateRemover(stateCollection, state);
        }

        private class StateRemover : IDisposable
        {
            private readonly List<object> stateCollection;
            private readonly object state;

            public StateRemover(List<object> stateCollection, object state)
            {
                this.stateCollection = stateCollection;
                this.state = state;
            }
            public void Dispose()
            {
                stateCollection.Remove(state);
            }
        }
    }
}