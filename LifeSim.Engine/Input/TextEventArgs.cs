using System;
using System.Collections.Generic;

namespace LifeSim.Engine.Input;

/// <summary>
/// Provides data for the <see cref="InputManager.TextEntered"/> event.
/// </summary>
public class TextEventArgs : EventArgs
{
    /// <summary>
    /// Gets the characters that were typed.
    /// </summary>
    public IReadOnlyList<char> Characters { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextEventArgs"/> class.
    /// </summary>
    /// <param name="characters">The characters that were typed.</param>
    public TextEventArgs(IReadOnlyList<char> characters)
    {
        this.Characters = characters;
    }
}
