namespace Alerting.ML.Engine.Alert;

public interface IConfigurationFactory<T> where T : AlertConfiguration<T>
{
    T Mutate(T value);

    (T, T) Crossover(T first, T second);

    T CreateRandom();
}