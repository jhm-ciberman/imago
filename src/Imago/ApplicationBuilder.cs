using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using NeoVeldrid;

namespace Imago;

/// <summary>
/// Collects the settings of an <see cref="Application"/> before it is created.
/// </summary>
/// <remarks>
/// Every method returns this builder, so calls can be chained. Nothing is created until <see cref="Build"/> or
/// <see cref="Run"/> is called. A later call replaces the value set by an earlier one, except for modules and
/// services, which are added.
/// </remarks>
public class ApplicationBuilder
{
    private readonly List<Module> _modules = [];
    private readonly List<Action<IServiceCollection>> _serviceRegistrations = [];

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    internal Type GameType { get; }

    internal string Title { get; private set; } = "Application";

    internal int WindowWidth { get; private set; } = 1024;

    internal int WindowHeight { get; private set; } = 600;

    internal WindowState WindowState { get; private set; } = Application.IsDebug ? WindowState.Maximized : WindowState.Normal;

    internal GuiSizeMode GuiSizeMode { get; private set; } = GuiSizeMode.Native;

    internal GraphicsBackend? Backend { get; private set; }

    internal bool IsHeadless { get; private set; }

    internal IReadOnlyList<Module> Modules => this._modules;

    internal IReadOnlyList<Action<IServiceCollection>> ServiceRegistrations => this._serviceRegistrations;

    internal ApplicationBuilder([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type gameType)
    {
        this.GameType = gameType;
    }

    /// <summary>
    /// Sets the title of the window.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <returns>This builder.</returns>
    public ApplicationBuilder WithTitle(string title)
    {
        this.Title = title;
        return this;
    }

    /// <summary>
    /// Sets the size of the window.
    /// </summary>
    /// <param name="width">The width, in pixels.</param>
    /// <param name="height">The height, in pixels.</param>
    /// <returns>This builder.</returns>
    public ApplicationBuilder WithWindowSize(int width, int height)
    {
        this.WindowWidth = width;
        this.WindowHeight = height;
        return this;
    }

    /// <summary>
    /// Sets the state the window is created in, such as maximized or fullscreen.
    /// </summary>
    /// <param name="state">The window state.</param>
    /// <returns>This builder.</returns>
    public ApplicationBuilder WithWindowState(WindowState state)
    {
        this.WindowState = state;
        return this;
    }

    /// <summary>
    /// Sets the GUI sizing mode. Use it to opt in to a fixed virtual resolution for pixel art or retro-style rendering.
    /// </summary>
    /// <param name="mode">The GUI sizing mode.</param>
    /// <returns>This builder.</returns>
    public ApplicationBuilder WithGuiSizeMode(GuiSizeMode mode)
    {
        this.GuiSizeMode = mode;
        return this;
    }

    /// <summary>
    /// Sets the graphics backend.
    /// </summary>
    /// <param name="backend">The graphics backend, or null for the default backend.</param>
    /// <returns>This builder.</returns>
    public ApplicationBuilder WithBackend(GraphicsBackend? backend)
    {
        this.Backend = backend;
        return this;
    }

    /// <summary>
    /// Adds modules to the application.
    /// </summary>
    /// <param name="modules">The modules to add.</param>
    /// <returns>This builder.</returns>
    public ApplicationBuilder WithModules(IEnumerable<Module> modules)
    {
        this._modules.AddRange(modules);
        return this;
    }

    /// <summary>
    /// Adds services to the application. The game can take them as constructor parameters.
    /// </summary>
    /// <param name="register">The callback that adds the services.</param>
    /// <returns>This builder.</returns>
    public ApplicationBuilder WithServices(Action<IServiceCollection> register)
    {
        this._serviceRegistrations.Add(register);
        return this;
    }

    /// <summary>
    /// Sets whether the window is created hidden.
    /// </summary>
    /// <remarks>
    /// A hidden window still provides a render context, so offscreen rendering and capture work without a window
    /// ever appearing. The window is hidden, not absent: a windowing system is still required (an X or Wayland
    /// server on Linux, or Xvfb), so this is not true display-less rendering.
    /// </remarks>
    /// <param name="isHeadless">Whether the window is created hidden, whatever the window state is.</param>
    /// <returns>This builder.</returns>
    public ApplicationBuilder WithHeadless(bool isHeadless)
    {
        this.IsHeadless = isHeadless;
        return this;
    }

    /// <summary>
    /// Creates the application and its game.
    /// </summary>
    /// <returns>The application, ready to run.</returns>
    /// <exception cref="InvalidOperationException">Thrown if an application instance already exists.</exception>
    public Application Build()
    {
        return new Application(this);
    }

    /// <summary>
    /// Creates the application and runs its main loop. Returns once the application has been quit and fully disposed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if an application instance already exists.</exception>
    public void Run()
    {
        this.Build().Run();
    }
}
