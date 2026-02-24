using System;
using Imago.Controls;

namespace Imago.DevConsole;

/// <summary>
/// A persistent GUI layer that hosts the developer console.
/// </summary>
public class ConsoleLayer : GuiLayer, IDisposable
{
    private readonly DeveloperConsole _console;
    private readonly DeveloperConsoleView _view;

    /// <summary>
    /// Gets the developer console model.
    /// </summary>
    public DeveloperConsole Console => this._console;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleLayer"/> class.
    /// </summary>
    public ConsoleLayer()
    {
        this.ZOrder = 900;
        this.IsVisible = false;

        this._console = new DeveloperConsole();
        this._view = new DeveloperConsoleView(this._console);
        this.Content = this._view;
    }

    /// <summary>
    /// Toggles the visibility of the developer console.
    /// </summary>
    public void Toggle()
    {
        this.IsVisible = !this.IsVisible;
        this.BlocksInputBelow = this.IsVisible;
    }

    /// <summary>
    /// Disposes the resources used by this layer.
    /// </summary>
    public void Dispose()
    {
        this._view.Dispose();
    }
}
