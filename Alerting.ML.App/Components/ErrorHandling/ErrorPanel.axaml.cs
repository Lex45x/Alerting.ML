using Avalonia;
using Avalonia.Controls;
using FluentValidation.Results;

namespace Alerting.ML.App.Components.ErrorHandling;

public partial class ErrorPanel : UserControl
{
    public static readonly StyledProperty<ValidationResult?> ValidationResultProperty =
        AvaloniaProperty.Register<ErrorPanel, ValidationResult?>(nameof(ValidationResult));

    public ErrorPanel()
    {
        InitializeComponent();
    }

    public ValidationResult? ValidationResult
    {
        get => GetValue(ValidationResultProperty);
        set => SetValue(ValidationResultProperty, value);
    }
}