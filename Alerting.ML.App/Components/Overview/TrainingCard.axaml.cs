using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Alerting.ML.App.Components.Overview;

public partial class TrainingCard : UserControl
{
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<TrainingCard, ICommand?>(nameof(Command));


    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public TrainingCard()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (Command?.CanExecute(DataContext) == true)
            Command.Execute(DataContext);
    }
}