using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Imago;

/// <summary>
/// Creates the screen that shows the given view model.
/// </summary>
/// <param name="services">The services the screen can take as constructor parameters.</param>
/// <param name="viewModel">The view model to show.</param>
/// <returns>The new screen.</returns>
internal delegate Screen ScreenFactory(IServiceProvider services, object viewModel);

/// <summary>
/// Collects which screen shows each view model, before the application is created.
/// </summary>
public sealed class ScreensBuilder
{
    private readonly Dictionary<Type, ScreenFactory> _screens = [];

    internal IReadOnlyDictionary<Type, ScreenFactory> Screens => this._screens;

    internal ScreensBuilder()
    {
    }

    /// <summary>
    /// Shows view models of the given type with screens of the given type.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TScreen">The type of the screen. Its constructor takes the view model and can take services.</typeparam>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentException">Thrown if <typeparamref name="TViewModel"/> already has a screen.</exception>
    public ScreensBuilder Map<TViewModel, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TScreen>()
        where TViewModel : class
        where TScreen : Screen
    {
        this._screens.Add(typeof(TViewModel), (services, viewModel) => ActivatorUtilities.CreateInstance<TScreen>(services, viewModel));
        return this;
    }
}
