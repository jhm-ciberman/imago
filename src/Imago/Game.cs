using System;

namespace Imago;

/// <summary>
/// The base class for the game that an <see cref="Application"/> runs.
/// </summary>
/// <remarks>
/// The application creates the game after every module has booted, so the constructor can build the scene and
/// take services as parameters. Pick the game with <see cref="Application.Configure{TGame}"/>.
/// </remarks>
public abstract class Game
{
    /// <summary>
    /// Gets the application that runs this game.
    /// </summary>
    public Application Application { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Game"/> class.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if no application exists yet.</exception>
    protected Game()
    {
        this.Application = Application.Instance
            ?? throw new InvalidOperationException("A game can only be created by its application.");
    }

    /// <summary>
    /// Called each frame before the stage update. Override to implement frame logic.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last frame in seconds.</param>
    protected internal virtual void Update(float deltaTime)
    {
        //
    }

    /// <summary>
    /// Called each frame after the stage update but before rendering.
    /// Override to flush visual changes or perform post-update logic.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last frame in seconds.</param>
    protected internal virtual void PostUpdate(float deltaTime)
    {
        //
    }
}
