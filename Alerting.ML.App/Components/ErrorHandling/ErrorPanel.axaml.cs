using Alerting.ML.App.Components.Overview;
using Avalonia;
using Avalonia.Controls;
using System.Windows.Input;
using FluentValidation.Results;

namespace Alerting.ML.App.Components.ErrorHandling;

public partial class ErrorPanel : UserControl
{
    public static readonly StyledProperty<ValidationResult?> ValidationResultProperty =
        AvaloniaProperty.Register<ErrorPanel, ValidationResult?>(nameof(ValidationResult));

    public ValidationResult? ValidationResult
    {
        get => GetValue(ValidationResultProperty);
        set => SetValue(ValidationResultProperty, value);
    }

    public ErrorPanel()
    {
        InitializeComponent();
    }
}