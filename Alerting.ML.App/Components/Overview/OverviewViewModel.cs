using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using Alerting.ML.App.ViewModels;
using Avalonia.Controls;
using ReactiveUI;

namespace Alerting.ML.App.Components.Overview;

public class OverviewViewModel : ViewModelBase
{
    public OverviewViewModel()
    {
        WindowSizeChangedCommand = ReactiveCommand.Create<SizeChangedEventArgs>(WindowSizeChanged);
    }

    private void WindowSizeChanged(SizeChangedEventArgs e)
    {
        EffectiveWidth = e.NewSize.Width;
        EffectiveHeight = e.NewSize.Height;
    }

    private double effectiveWidth;
    private double effectiveHeight;
    public virtual ObservableCollection<TrainingCardViewModel> Cards { get; }
    public ReactiveCommand<SizeChangedEventArgs, Unit> WindowSizeChangedCommand { get; }

    public double EffectiveWidth
    {
        get => effectiveWidth;
        set
        {
            effectiveWidth = value;
            this.RaisePropertyChanged(nameof(CardsColumns));
        }
    }

    public int CardsColumns => Math.Max((int)Math.Floor(EffectiveWidth / 450), 1);
    public int CardsRows => Math.Max((int)Math.Floor(EffectiveHeight / 200), Cards.Count / CardsColumns);

    public double EffectiveHeight
    {
        get => effectiveHeight;
        set
        {
            effectiveHeight = value;
            this.RaisePropertyChanged(nameof(CardsRows));
        }
    }
}

public class OverviewViewModelDesignTime : OverviewViewModel
{
    public override ObservableCollection<TrainingCardViewModel> Cards { get; } = new()
    {
        new TrainingCardViewModelDesignTime(),
        new TrainingCardViewModelDesignTime(),
        new TrainingCardViewModelDesignTime(),
        new TrainingCardViewModelDesignTime(),
        new TrainingCardViewModelDesignTime(),
        new TrainingCardViewModelDesignTime(),
        new TrainingCardViewModelDesignTime(),
        new TrainingCardViewModelDesignTime(),
        new TrainingCardViewModelDesignTime(),
    };
}