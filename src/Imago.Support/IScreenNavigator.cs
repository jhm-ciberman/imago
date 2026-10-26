using System;
using System.Diagnostics.CodeAnalysis;

namespace Imago.Support;

/// <summary>
/// Moves the application between screens by view model.
/// </summary>
public interface IScreenNavigator
{
    /// <summary>
    /// Shows the screen of a new view model of the given type, in place of the current screen.
    /// </summary>
    /// <remarks>
    /// The change happens before the screens are next updated. If it is requested more than once before that, the
    /// last request wins.
    /// </remarks>
    /// <typeparam name="TViewModel">The type of the view model. Its constructor can take services as parameters.</typeparam>
    /// <param name="args">The constructor parameters of the view model that are not services.</param>
    /// <exception cref="InvalidOperationException">Thrown if no screen is mapped to <typeparamref name="TViewModel"/>.</exception>
    public void Navigate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>(
        params object[] args
    ) where TViewModel : class;

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void Quit();
}
