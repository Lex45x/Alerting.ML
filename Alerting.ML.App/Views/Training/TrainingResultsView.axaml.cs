using Avalonia.Controls;
using ReactiveUI.Avalonia;

namespace Alerting.ML.App.Views.Training;

public partial class TrainingResultsView : ReactiveUserControl<TrainingResultsViewModel>
{
    public TrainingResultsView()
    {
        InitializeComponent();
    }
}