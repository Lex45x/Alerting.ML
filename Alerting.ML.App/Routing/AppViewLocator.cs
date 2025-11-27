using Alerting.ML.App.Components.TrainingCreation.Outages;
using Alerting.ML.App.Components.TrainingCreation.Preview;

namespace Alerting.ML.App.Routing;

using Components.TrainingCreation;
using Components.TrainingCreation.Csv;
using ReactiveUI;
using System;
using Views.Overview;
using Views.TrainingCreation;

public class AppViewLocator : IViewLocator
{
    public IViewFor ResolveView<T>(T? viewModel, string? contract = null)
    {
        return viewModel switch
        {
            OverviewViewModel => new OverviewView { DataContext = viewModel },
            TrainingCreationViewModel => new TrainingCreationView() { DataContext = viewModel },
            TrainingCreationFirstStepViewModel => new TrainingCreationFirstStepView { DataContext = viewModel },
            TrainingCreationCsvSecondStepViewModel => new TrainingCreationCsvSecondStepView { DataContext = viewModel },
            TrainingCreationFourthStepViewModel => new TrainingCreationFourthStepView { DataContext = viewModel },
            TrainingCreationFifthStepViewModel => new TrainingCreationFifthStepView { DataContext = viewModel },
            _ => throw new ArgumentOutOfRangeException(nameof(viewModel), $"Unsupported ViewModel type {viewModel?.GetType().ToString() ?? "null"}")
        };
    }
}