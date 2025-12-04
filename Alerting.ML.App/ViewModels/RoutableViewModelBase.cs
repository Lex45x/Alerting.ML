using System;
using System.Reactive.Linq;
using ReactiveUI;

namespace Alerting.ML.App.ViewModels;

public abstract class RoutableViewModelBase : ViewModelBase, IRoutableViewModel
{
    protected RoutableViewModelBase(IScreen hostScreen)
    {
        HostScreen = hostScreen;
        IsOnTopOfNavigation = hostScreen?.Router.CurrentViewModel.Select(model => model?.GetType() == GetType());
    }
    protected IObservable<bool> IsOnTopOfNavigation { get; }
    public abstract string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
}