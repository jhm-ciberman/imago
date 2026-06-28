namespace Imago.ScenarioRunner;

/// <summary>
/// Entry point for running scenarios. Call <see cref="For{TGame}"/> to configure a run, then
/// <see cref="ScenarioRunnerBuilder{TGame}.Run(string[])"/>.
/// </summary>
public static class ScenarioRunner
{
    /// <summary>
    /// Begins configuring a scenario run for the given game application.
    /// </summary>
    /// <typeparam name="TGame">The game application the scenarios drive.</typeparam>
    /// <returns>A builder used to configure and run the scenarios.</returns>
    public static ScenarioRunnerBuilder<TGame> For<TGame>() where TGame : Application
    {
        return new ScenarioRunnerBuilder<TGame>();
    }
}
