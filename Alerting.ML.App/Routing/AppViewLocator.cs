using System;
using Alerting.ML.App.Components.TrainingCreation;
using Alerting.ML.App.Components.TrainingCreation.Csv;
using Alerting.ML.App.Components.TrainingCreation.Outages;
using Alerting.ML.App.Components.TrainingCreation.Preview;
using Alerting.ML.App.Views.Overview;
using Alerting.ML.App.Views.Training;
using Alerting.ML.App.Views.TrainingCreation;
using ReactiveUI;

namespace Alerting.ML.App.Routing;

public class AppViewLocator : IViewLocator
{
    public IViewFor ResolveView<T>(T? viewModel, string? contract = null)
    {
        return viewModel switch
        {
            OverviewViewModel => new OverviewView { DataContext = viewModel },
            TrainingCreationViewModel => new TrainingCreationView { DataContext = viewModel },
            TrainingCreationFirstStepViewModel => new TrainingCreationFirstStepView { DataContext = viewModel },
            TrainingCreationCsvSecondStepViewModel => new TrainingCreationCsvSecondStepView { DataContext = viewModel },
            TrainingCreationFourthStepViewModel => new TrainingCreationFourthStepView { DataContext = viewModel },
            TrainingCreationFifthStepViewModel => new TrainingCreationFifthStepView { DataContext = viewModel },
            TrainingViewModel => new TrainingView { DataContext = viewModel },
            _ => throw new ArgumentOutOfRangeException(nameof(viewModel),
                $"Unsupported ViewModel type {viewModel?.GetType().ToString() ?? "null"}")
        };
    }
}