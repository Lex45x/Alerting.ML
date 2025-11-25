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
        //todo: configure
        var loggerFactory = LoggerFactory.Create(builder => { });

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(loggerFactory)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
