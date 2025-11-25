using Alerting.ML.App.Model.Enums;
using Avalonia;
using Avalonia.Controls;
using System;

namespace Alerting.ML.App.Components.TrainingCreation;

public partial class TrainingCreationStepIndicator : UserControl
{
    public static readonly StyledProperty<TrainingCreationStep> IndicatorStepProperty =
        AvaloniaProperty.Register<TrainingCreationStepIndicator, TrainingCreationStep>(nameof(IndicatorStep));

    public static readonly StyledProperty<TrainingCreationStep> CurrentStepProperty =
        AvaloniaProperty.Register<TrainingCreationStepIndicator, TrainingCreationStep>(nameof(CurrentStep));

    public TrainingCreationStep IndicatorStep
    {
        get => GetValue(IndicatorStepProperty);
        set => SetValue(IndicatorStepProperty, value);
    }

    public TrainingCreationStep CurrentStep
    {
        get => GetValue(CurrentStepProperty);
        set => SetValue(CurrentStepProperty, value);
    }

    private void UpdateClasses()
    {
        Classes.Remove("completed");
        Classes.Remove("current");
        Classes.Remove("following");

        if (CurrentStep > IndicatorStep)
        {
            Classes.Add("completed");
        }
        else if (CurrentStep == IndicatorStep)
        {
            Classes.Add("current");
        }
        else
        {
            Classes.Add("following");
        }
    }

    private void UpdateIcon()
    {
        if (CurrentStep > IndicatorStep)
        {
            IconSvg.Path = "avares://Alerting.ML.App/Assets/check-icon.svg";
        }
        else
        {
            IconSvg.Path = IndicatorStep switch
            {
                TrainingCreationStep.Step1 => "avares://Alerting.ML.App/Assets/cloud-icon.svg",
                TrainingCreationStep.Step2 => "avares://Alerting.ML.App/Assets/key-icon.svg",
                TrainingCreationStep.Step3 => "avares://Alerting.ML.App/Assets/datasource-icon.svg",
                TrainingCreationStep.Step4 => "avares://Alerting.ML.App/Assets/alert-icon.svg",
                TrainingCreationStep.Step5 => "avares://Alerting.ML.App/Assets/eye-icon.svg",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    private void UpdateTitle()
    {
        TitleBox.Text = IndicatorStep switch
        {
            TrainingCreationStep.Step1 => "Choose Source",
            TrainingCreationStep.Step2 => "Configure",
            TrainingCreationStep.Step3 => "Select Data",
            TrainingCreationStep.Step4 => "Import Outages",
            TrainingCreationStep.Step5 => "Preview",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void UpdateSubTitle()
    {
        SubtitleBox.Text = IndicatorStep switch
        {
            TrainingCreationStep.Step1 => "Step 1",
            TrainingCreationStep.Step2 => "Step 2",
            TrainingCreationStep.Step3 => "Step 3",
            TrainingCreationStep.Step4 => "Step 4",
            TrainingCreationStep.Step5 => "Step 5",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public TrainingCreationStepIndicator()
    {
        InitializeComponent();
        this.GetObservable(IndicatorStepProperty).Subscribe(step =>
        {
            UpdateClasses();
            UpdateTitle();
            UpdateSubTitle();
            UpdateIcon();
        });
        this.GetObservable(CurrentStepProperty).Subscribe(step =>
        {
            UpdateClasses();
            UpdateIcon();
        });
    }
}