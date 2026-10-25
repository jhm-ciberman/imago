using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Imago.Scenarios;
using Imago.Support.Numerics;
using NeoVeldrid;

namespace Imago.Gui.Scenarios;

/// <summary>
/// A GUI control found in a running <see cref="Imago.Scenarios.Scenario"/>, which a script can act on through the
/// real input path. Finds on a locator search inside its control.
/// </summary>
/// <typeparam name="T">The type of the control.</typeparam>
public sealed class GuiLocator<T> : GuiFinder where T : Control
{
    internal GuiLocator(Scenario scenario, T control)
        : base(scenario)
    {
        this.Control = control;
    }

    /// <summary>
    /// Gets the control that was found.
    /// </summary>
    public T Control { get; }

    /// <summary>
    /// Gets the bounds of the control on screen, in window pixels.
    /// </summary>
    public Rect ScreenBounds
    {
        get
        {
            var layer = this.Control.Layer!;
            var min = layer.ViewportToWindow(this.Control.Position);
            var max = layer.ViewportToWindow(this.Control.Position + this.Control.ActualSize);
            return new Rect(min, max - min);
        }
    }

    /// <summary>
    /// Gets the center of the control on screen, in window pixels.
    /// </summary>
    public Vector2 ScreenCenter
    {
        get
        {
            var bounds = this.ScreenBounds;
            return bounds.Position + bounds.Size / 2f;
        }
    }

    /// <summary>
    /// Returns a locator for the same control as the given type.
    /// </summary>
    /// <typeparam name="TOther">The type the control is expected to be.</typeparam>
    /// <returns>A locator for the same control.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the control is not of the given type.</exception>
    public GuiLocator<TOther> As<TOther>() where TOther : Control
    {
        if (this.Control is TOther other)
        {
            return new GuiLocator<TOther>(this.Scenario, other);
        }

        throw new InvalidOperationException($"Expected a {typeof(TOther).Name}, but {this} is a {this.Control.GetType().Name}.");
    }

    /// <summary>
    /// Clicks the center of the control with the scenario's mouse.
    /// </summary>
    /// <param name="button">The button to click with.</param>
    /// <returns>A task that completes once the button has been released.</returns>
    public Task Click(MouseButton button = MouseButton.Left)
    {
        return this.Scenario.Mouse.Click(this.ScreenCenter, button);
    }

    /// <summary>
    /// Moves the scenario's mouse over the center of the control.
    /// </summary>
    /// <returns>A task that completes once the game has seen the mouse over the control.</returns>
    public Task Hover()
    {
        this.Scenario.Mouse.Move(this.ScreenCenter);
        return this.Scenario.NextFrame();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return string.IsNullOrEmpty(this.Control.Name) ? this.Control.GetType().Name : this.Control.Name;
    }

    private protected override IEnumerable<Visual> GetRoots()
    {
        return this.Control.VisualChildren;
    }

    private protected override string Describe(string step)
    {
        return $"{step} inside {this}";
    }
}
