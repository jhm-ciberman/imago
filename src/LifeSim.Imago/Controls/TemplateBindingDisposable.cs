using System;
using System.ComponentModel;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Wraps a cleanup action that unsubscribes template binding handlers when disposed.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class TemplateBindingDisposable : IDisposable
{
    private Action? _disposeAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateBindingDisposable"/> class.
    /// </summary>
    /// <param name="disposeAction">The action to invoke on disposal.</param>
    public TemplateBindingDisposable(Action disposeAction)
    {
        this._disposeAction = disposeAction;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this._disposeAction?.Invoke();
        this._disposeAction = null;
    }
}
