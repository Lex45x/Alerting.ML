using System;
using System.Reactive;
using System.Reactive.Disposables;
using ReactiveUI;

namespace Alerting.ML.App.ViewModels;

public class ViewModelBase : ReactiveObject, IDisposable
{
    protected readonly CompositeDisposable Disposables = new();

    public ViewModelBase()
    {
        DisposeCommand = ReactiveCommand.Create(Dispose);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Disposables.Dispose();
        }
    }

    public ReactiveCommand<Unit, Unit> DisposeCommand { get; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}