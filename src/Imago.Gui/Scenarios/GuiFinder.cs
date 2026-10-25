using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Imago.Scenarios;

namespace Imago.Gui.Scenarios;

/// <summary>
/// Finds GUI controls in a running <see cref="Imago.Scenarios.Scenario"/>, so a script can act on them through the
/// real input path.
/// </summary>
/// <remarks>
/// Only visible controls that are laid out count. A find needs exactly one match and throws otherwise. A wait gives
/// the match time to appear first. Names are not unique, so narrow a find by chaining another find on the result.
/// </remarks>
public abstract class GuiFinder
{
    private protected GuiFinder(Scenario scenario)
    {
        this.Scenario = scenario;
    }

    private protected Scenario Scenario { get; }

    /// <summary>
    /// Finds the only visible control that matches the given predicate.
    /// </summary>
    /// <param name="predicate">The condition a control must meet.</param>
    /// <returns>A locator for the control.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is not exactly one match.</exception>
    public GuiLocator<Control> Find(Func<Control, bool> predicate)
    {
        return this.FindSingle(predicate, "Find(predicate)");
    }

    /// <summary>
    /// Finds the only visible control of the given type that matches the given predicate.
    /// </summary>
    /// <typeparam name="T">The type of control to look for.</typeparam>
    /// <param name="predicate">The condition a control must meet, or null to accept any control of the type.</param>
    /// <returns>A locator for the control.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is not exactly one match.</exception>
    public GuiLocator<T> Find<T>(Func<T, bool>? predicate = null) where T : Control
    {
        return this.FindSingle(predicate, DescribeFind<T>("Find", predicate));
    }

    /// <summary>
    /// Finds the only visible control with the given name.
    /// </summary>
    /// <param name="name">The name to look for.</param>
    /// <returns>A locator for the control.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is not exactly one match.</exception>
    public GuiLocator<Control> FindByName(string name)
    {
        return this.FindSingle(IsNamed<Control>(name), $"FindByName(\"{name}\")");
    }

    /// <summary>
    /// Finds the only visible control of the given type with the given name.
    /// </summary>
    /// <typeparam name="T">The type of control to look for.</typeparam>
    /// <param name="name">The name to look for.</param>
    /// <returns>A locator for the control.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is not exactly one match.</exception>
    public GuiLocator<T> FindByName<T>(string name) where T : Control
    {
        return this.FindSingle(IsNamed<T>(name), $"FindByName<{typeof(T).Name}>(\"{name}\")");
    }

    /// <summary>
    /// Finds the only visible text block that shows exactly the given text.
    /// </summary>
    /// <param name="text">The text to look for.</param>
    /// <returns>A locator for the text block.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is not exactly one match.</exception>
    public GuiLocator<TextBlock> FindByText(string text)
    {
        return this.FindSingle(HasText(text), $"FindByText(\"{text}\")");
    }

    /// <summary>
    /// Finds every visible control that matches the given predicate.
    /// </summary>
    /// <param name="predicate">The condition a control must meet.</param>
    /// <returns>A locator for each control, in stacking order. Empty if there are none.</returns>
    public IReadOnlyList<GuiLocator<Control>> FindMany(Func<Control, bool> predicate)
    {
        return this.FindAll(predicate);
    }

    /// <summary>
    /// Finds every visible control of the given type that matches the given predicate.
    /// </summary>
    /// <typeparam name="T">The type of control to look for.</typeparam>
    /// <param name="predicate">The condition a control must meet, or null to accept any control of the type.</param>
    /// <returns>A locator for each control, in stacking order. Empty if there are none.</returns>
    public IReadOnlyList<GuiLocator<T>> FindMany<T>(Func<T, bool>? predicate = null) where T : Control
    {
        return this.FindAll(predicate);
    }

    /// <summary>
    /// Finds every visible control with the given name.
    /// </summary>
    /// <param name="name">The name to look for.</param>
    /// <returns>A locator for each control, in stacking order. Empty if there are none.</returns>
    public IReadOnlyList<GuiLocator<Control>> FindManyByName(string name)
    {
        return this.FindAll(IsNamed<Control>(name));
    }

    /// <summary>
    /// Finds every visible control of the given type with the given name.
    /// </summary>
    /// <typeparam name="T">The type of control to look for.</typeparam>
    /// <param name="name">The name to look for.</param>
    /// <returns>A locator for each control, in stacking order. Empty if there are none.</returns>
    public IReadOnlyList<GuiLocator<T>> FindManyByName<T>(string name) where T : Control
    {
        return this.FindAll(IsNamed<T>(name));
    }

    /// <summary>
    /// Finds every visible text block that shows exactly the given text.
    /// </summary>
    /// <param name="text">The text to look for.</param>
    /// <returns>A locator for each text block, in stacking order. Empty if there are none.</returns>
    public IReadOnlyList<GuiLocator<TextBlock>> FindManyByText(string text)
    {
        return this.FindAll(HasText(text));
    }

    /// <summary>
    /// Waits until there is exactly one visible control that matches the given predicate.
    /// </summary>
    /// <param name="predicate">The condition a control must meet.</param>
    /// <returns>A task whose result is a locator for the control.</returns>
    /// <exception cref="TimeoutException">Thrown if there is not exactly one match in time.</exception>
    public Task<GuiLocator<Control>> WaitFor(Func<Control, bool> predicate)
    {
        return this.WaitForSingle(predicate, "WaitFor(predicate)");
    }

    /// <summary>
    /// Waits until there is exactly one visible control of the given type that matches the given predicate.
    /// </summary>
    /// <typeparam name="T">The type of control to look for.</typeparam>
    /// <param name="predicate">The condition a control must meet, or null to accept any control of the type.</param>
    /// <returns>A task whose result is a locator for the control.</returns>
    /// <exception cref="TimeoutException">Thrown if there is not exactly one match in time.</exception>
    public Task<GuiLocator<T>> WaitFor<T>(Func<T, bool>? predicate = null) where T : Control
    {
        return this.WaitForSingle(predicate, DescribeFind<T>("WaitFor", predicate));
    }

    /// <summary>
    /// Waits until there is exactly one visible control with the given name.
    /// </summary>
    /// <param name="name">The name to look for.</param>
    /// <returns>A task whose result is a locator for the control.</returns>
    /// <exception cref="TimeoutException">Thrown if there is not exactly one match in time.</exception>
    public Task<GuiLocator<Control>> WaitForName(string name)
    {
        return this.WaitForSingle(IsNamed<Control>(name), $"WaitForName(\"{name}\")");
    }

    /// <summary>
    /// Waits until there is exactly one visible control of the given type with the given name.
    /// </summary>
    /// <typeparam name="T">The type of control to look for.</typeparam>
    /// <param name="name">The name to look for.</param>
    /// <returns>A task whose result is a locator for the control.</returns>
    /// <exception cref="TimeoutException">Thrown if there is not exactly one match in time.</exception>
    public Task<GuiLocator<T>> WaitForName<T>(string name) where T : Control
    {
        return this.WaitForSingle(IsNamed<T>(name), $"WaitForName<{typeof(T).Name}>(\"{name}\")");
    }

    /// <summary>
    /// Waits until there is exactly one visible text block that shows exactly the given text.
    /// </summary>
    /// <param name="text">The text to look for.</param>
    /// <returns>A task whose result is a locator for the text block.</returns>
    /// <exception cref="TimeoutException">Thrown if there is not exactly one match in time.</exception>
    public Task<GuiLocator<TextBlock>> WaitForText(string text)
    {
        return this.WaitForSingle(HasText(text), $"WaitForText(\"{text}\")");
    }

    private protected abstract IEnumerable<Visual> GetRoots();

    private protected abstract string Describe(string step);

    private static Func<T, bool> IsNamed<T>(string name) where T : Control
    {
        return control => control.Name == name;
    }

    private static Func<TextBlock, bool> HasText(string text)
    {
        return block => block.Text == text;
    }

    private static string DescribeFind<T>(string method, Func<T, bool>? predicate)
    {
        return predicate == null ? $"{method}<{typeof(T).Name}>()" : $"{method}<{typeof(T).Name}>(predicate)";
    }

    private GuiLocator<T> FindSingle<T>(Func<T, bool>? predicate, string step) where T : Control
    {
        var matches = this.Collect(predicate);
        if (matches.Count != 1)
        {
            throw new InvalidOperationException($"{this.Describe(step)} found {matches.Count} controls, not exactly one.");
        }

        return new GuiLocator<T>(this.Scenario, matches[0]);
    }

    private IReadOnlyList<GuiLocator<T>> FindAll<T>(Func<T, bool>? predicate) where T : Control
    {
        var matches = this.Collect(predicate);
        var locators = new List<GuiLocator<T>>(matches.Count);
        foreach (var control in matches)
        {
            locators.Add(new GuiLocator<T>(this.Scenario, control));
        }

        return locators;
    }

    private async Task<GuiLocator<T>> WaitForSingle<T>(Func<T, bool>? predicate, string step) where T : Control
    {
        try
        {
            await this.Scenario.WaitUntil(() => this.Collect(predicate).Count == 1, $"exactly one control for {this.Describe(step)}");
        }
        catch (TimeoutException)
        {
            throw new TimeoutException($"{this.Describe(step)} found {this.Collect(predicate).Count} controls, not exactly one.");
        }

        return new GuiLocator<T>(this.Scenario, this.Collect(predicate)[0]);
    }

    private List<T> Collect<T>(Func<T, bool>? predicate) where T : Control
    {
        var matches = new List<T>();
        foreach (var root in this.GetRoots())
        {
            CollectMatches(root, predicate, matches);
        }

        return matches;
    }

    private static void CollectMatches<T>(Visual visual, Func<T, bool>? predicate, List<T> matches) where T : Control
    {
        // A control that is not laid out yet has no position on screen, so it cannot be acted on.
        if (visual.Visibility != Visibility.Visible || !visual.IsArrangeValid)
        {
            return;
        }

        if (visual is T control && (predicate == null || predicate(control)))
        {
            matches.Add(control);
        }

        foreach (var child in visual.VisualChildren)
        {
            CollectMatches(child, predicate, matches);
        }
    }
}
