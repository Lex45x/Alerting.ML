using System;
using Alerting.ML.App.Components.Overview;
using Alerting.ML.App.Views.Overview;
using Alerting.ML.App.Views.TrainingCreation;
using ReactiveUI;

namespace Alerting.ML.App.Routing;

public class AppViewLocator : IViewLocator
{
    public IViewFor? ResolveView<T>(T? viewModel, string? contract = null)
    {
        return viewModel switch
        {
            OverviewViewModel => new OverviewView { DataContext = viewModel },
            TrainingCreationFirstStepViewModel => new TrainingCreationFirstStepView { DataContext = viewModel },
            _ => throw new ArgumentOutOfRangeException(nameof(viewModel), $"Unsupported ViewModel type {typeof(T)}")
        };
    }
}