using System.Threading.Tasks;
using Alerting.ML.App.Model.Training;
using Alerting.ML.App.ViewModels;
using Alerting.ML.App.Views;
using Alerting.ML.Engine.Storage;
using Alerting.ML.Sources.Azure;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Alerting.ML.App;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        this.AttachDeveloperTools();
    }


    public override void OnFrameworkInitializationCompleted()
    {
        // registering all configuration types supported by App.
        KnownTypeInfoResolver.Instance.WithAzureTypes();
        var eventStore = new JsonFileEventStore("./event-store-v1");
        var trainingOrchestrator = new BackgroundTrainingOrchestrator(eventStore);

        //todo: this has to be awaited.
        Task.Run(trainingOrchestrator.ImportFromEventStore);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownRequested += (sender, args) => eventStore.Dispose();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(trainingOrchestrator)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}