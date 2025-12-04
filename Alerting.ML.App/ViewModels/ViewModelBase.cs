using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;

namespace Alerting.ML.App.ViewModels;

public abstract class ViewModelBase : ReactiveObject, IDisposable
{
    protected readonly CompositeDisposable Disposables = new();

    protected ViewModelBase()
    {
        DisposeCommand = ReactiveCommand.Create(Dispose);
    }

    public ReactiveCommand<Unit, Unit> DisposeCommand { get; }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Disposables.Dispose();
        }
    }
}