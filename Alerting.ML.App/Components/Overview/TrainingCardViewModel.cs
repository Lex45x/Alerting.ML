using System;
using Alerting.ML.App.ViewModels;

namespace Alerting.ML.App.Components.Overview;

public class TrainingCardViewModel : ViewModelBase
{
    public virtual string TrainingSessionName { get; }
    public virtual string TrainingDate { get; }
    public virtual double HighestScore { get; }
    public virtual double LatestProgress { get; }

    public virtual int GenerationsCounter { get; }
}

public class TrainingCardViewModelDesignTime : TrainingCardViewModel
{
    public override string TrainingSessionName => "Azure Scheduled Query Rule";
    public override string TrainingDate => DateTime.UtcNow.ToString("d");
    public override double HighestScore => 0.9;
    public override double LatestProgress => 0.7;
    public override int GenerationsCounter => 121;
}