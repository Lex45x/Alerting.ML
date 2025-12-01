# Alerting.ML

Alerting.ML is a toolkit for training and tuning alert rules against historical time-series and outage data. It uses a genetic algorithm to evolve alert rule parameters and score them against known outages.

## v0.1 (active work)
- Objective: end-to-end PoC that optimizes Azure Scheduled Query Rule-style alerts against offline data and outputs best-fit configurations.
- Engine: genetic optimizer + scoring (`TrainingBuilder`, `GeneticOptimizerStateMachine`, `DefaultAlertScoreCalculator`), with pluggable event store plumbing.
- Data: synthetic sample providers plus CSV loaders for metrics/outages with validation and training-builder extensions.
- Alert sources: Azure scheduled query rule implementation (`ScheduledQueryRuleAlert`, `ScheduledQueryRuleConfiguration`) with parameter annotations for mutation/crossover.
- Apps: console sample that emits sample outages/time-series (optimizer wiring scaffolded in `Program.cs`); Avalonia desktop shell (`Alerting.ML.App` + `Alerting.ML.App.Desktop`) for future UI-driven training.
- Tests: coverage for engine and CSV sources (`Alerting.ML.Engine.Tests`, `Alerting.ML.Source.Csv.Tests`).

## Repository layout
- `Alerting.ML.Engine`: core abstractions (alerts/config factories), genetic optimizer, scoring, event store.
- `Alerting.ML.Sources.Azure`: Azure Scheduled Query Rule implementation and helpers.
- `Alerting.ML.Sources.Csv`: CSV providers for time-series/outage data and builder extensions.
- `Alerting.ML.TimeSeries.Sample`: synthetic data for experiments.
- `Alerting.ML.Console`: sample console app producing CSV outputs and containing optimizer scaffold.
- `Alerting.ML.App` / `Alerting.ML.App.Desktop`: Avalonia UI shell (desktop entrypoint) for interactive training.
- Tests: `Alerting.ML.Engine.Tests`, `Alerting.ML.Source.Csv.Tests`.

## How to try v0.1 locally
- Prerequisite: .NET 10 (net10.0) SDK and Avalonia UI dependencies.
- Build solution: `dotnet build Alerting.ML.sln`
- Run console sample: `dotnet run --project Alerting.ML.Console`
- Generated files: `outages_1.csv` and `timeseries_1.csv` contain the sample dataset that can feed the CSV providers.

## Next steps for the milestone
- Wire Avalonia UI to the training pipeline and surface optimizer progress.
- Expand scoring metrics and persistence through a durable `IEventStore`.
- Add more data connectors beyond CSV/sample (e.g., direct Azure Metrics/Logs).
- Harden validation around CSV imports and configuration mutation ranges.
