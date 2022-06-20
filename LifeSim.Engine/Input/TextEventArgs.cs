using System;
using System.Collections.Generic;

namespace LifeSim.Engine;

public class TextEventArgs : EventArgs
{
    /// <summary>
    /// Gets the characters that were typed.
    /// </summary>
    public IReadOnlyList<char> Characters { get; }

    public TextEventArgs(IReadOnlyList<char> characters)
    {
        Characters = characters;
    }
}