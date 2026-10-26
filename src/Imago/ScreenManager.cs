using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Imago.Metrics;
using Imago.Support;
using Microsoft.Extensions.DependencyInjection;

namespace Imago;

/// <summary>
/// A screen to show, with the view model it shows.
/// </summary>
/// <param name="CreateViewModel">Creates the view model, or returns null for a screen without one.</param>
/// <param name="CreateScreen">Creates the screen for the given view model.</param>
internal sealed record ScreenRequest(Func<object?> CreateViewModel, Func<object?, Screen> CreateScreen);

/// <summary>
/// Shows one screen at a time on the stage of the application, and owns that screen and its view model.
/// </summary>
/// <remarks>
/// A change of screen happens before the screens are next updated, so a new screen is always updated before it is
/// rendered. If it is requested more than once before that, the last request wins. The old screen and its view model
/// are disposed when the new ones are shown.
/// </remarks>
public sealed class ScreenManager : IScreenNavigator, IDisposable
{
    /// <summary>
    /// Occurs when a new screen has been shown.
    /// </summary>
    public event EventHandler? ScreenChanged;

    /// <summary>
    /// Gets the screen shown now, or null if there is none.
    /// </summary>
    public Screen? Screen => this._application.Stage.Screen;

    /// <summary>
    /// Gets the view model of the screen shown now, or null if the screen was shown without one.
    /// </summary>
    public object? ViewModel { get; private set; }

    private readonly IServiceProvider _services;
    private readonly Application _application;
    private readonly IReadOnlyDictionary<Type, ScreenFactory> _screens;
    private readonly ImagoMetrics _metrics;
    private ScreenRequest? _request;
    private Screen? _requestedScreen;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenManager"/> class.
    /// </summary>
    /// <param name="services">The services that create the screens and the view models.</param>
    /// <param name="application">The application that shows the screens.</param>
    /// <param name="screens">The function that creates the screen of each type of view model.</param>
    internal ScreenManager(
        IServiceProvider services,
        Application application,
        IReadOnlyDictionary<Type, ScreenFactory> screens
    )
    {
        this._services = services;
        this._application = application;
        this._screens = screens;
        this._metrics = application.Metrics.Imago;
    }

    /// <summary>
    /// Shows the given screen in place of the current screen. The screen manager takes ownership of it.
    /// </summary>
    /// <param name="screen">The screen to show.</param>
    public void Show(Screen screen)
    {
        this.Request(new ScreenRequest(() => null, _ => screen), screen);
    }

    /// <summary>
    /// Shows a new screen of the given type in place of the current screen.
    /// </summary>
    /// <typeparam name="TScreen">The type of the screen. Its constructor can take services as parameters.</typeparam>
    /// <param name="args">The constructor parameters of the screen that are not services.</param>
    public void Show<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TScreen>(
        params object[] args
    ) where TScreen : Screen
    {
        this.Request(new ScreenRequest(() => null, _ => ActivatorUtilities.CreateInstance<TScreen>(this._services, args)));
    }

    /// <inheritdoc />
    public void Navigate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>(
        params object[] args
    ) where TViewModel : class
    {
        if (!this._screens.TryGetValue(typeof(TViewModel), out var createScreen))
        {
            throw new InvalidOperationException(
                $"No screen is mapped to the view model '{typeof(TViewModel).Name}'. Map one with {nameof(ApplicationBuilder.WithScreens)}."
            );
        }

        this.Request(new ScreenRequest(
            () => ActivatorUtilities.CreateInstance<TViewModel>(this._services, args),
            viewModel => createScreen(this._services, viewModel!)
        ));
    }

    /// <inheritdoc />
    public void Quit()
    {
        this._application.Quit();
    }

    /// <summary>
    /// Updates the view model of the current screen.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last frame, in seconds.</param>
    internal void Update(float deltaTime)
    {
        using (this._metrics.ScreenViewModelUpdateTime.StartTimer())
        {
            (this.ViewModel as IUpdatable)?.Update(deltaTime);
        }
    }

    /// <summary>
    /// Shows the last requested screen, if any.
    /// </summary>
    internal void ShowRequestedScreen()
    {
        if (this._request is not ScreenRequest request) return;

        this._request = null;
        this._requestedScreen = null;

        // The old view model is gone before the new one is created, so a view model can be a singleton.
        this.Unload();

        var viewModel = request.CreateViewModel();
        var screen = request.CreateScreen(viewModel);
        this.ViewModel = viewModel;
        this._application.Stage.Screen = screen;

        GCUtilities.AgressiveGC();
        this.ScreenChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Request(ScreenRequest request, Screen? screen = null)
    {
        if (this._requestedScreen != screen)
        {
            this._requestedScreen?.Dispose();
        }

        this._request = request;
        this._requestedScreen = screen;
    }

    private void Unload()
    {
        var stage = this._application.Stage;
        var screen = stage.Screen;
        stage.Screen = null;

        (this.ViewModel as IDisposable)?.Dispose();
        this.ViewModel = null;

        screen?.Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.Unload();

        this._requestedScreen?.Dispose();
        this._requestedScreen = null;
        this._request = null;
    }
}
