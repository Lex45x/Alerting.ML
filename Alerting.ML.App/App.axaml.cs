using Alerting.ML.App.Model.Training;
using Alerting.ML.App.ViewModels;
using Alerting.ML.App.Views;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;

namespace Alerting.ML.App;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        this.AttachDeveloperTools();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var trainingOrchestrator = new BackgroundTrainingOrchestrator();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(trainingOrchestrator)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
