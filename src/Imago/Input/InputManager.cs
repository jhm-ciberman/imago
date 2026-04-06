using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using Imago.Support.Numerics;
using NeoVeldrid;
using NeoVeldrid.Sdl2;

namespace Imago.Input;

/// <summary>
/// Provides data for keyboard-related events.
/// </summary>
public class KeyboardEventArgs : HandledEventArgs
{
    /// <summary>
    /// Gets the key that was pressed or released.
    /// </summary>
    public Key Key { get; internal set; } = default;

    /// <summary>
    /// Gets a value indicating whether the key is now in the down state.
    /// </summary>
    public bool IsDown { get; internal set; } = false;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Key: {this.Key}, Handled: {this.Handled}";
    }
}

/// <summary>
/// Provides data for mouse button-related events.
/// </summary>
public class MouseButtonEventArgs : HandledEventArgs
{
    /// <summary>
    /// Gets the mouse button that was pressed or released.
    /// </summary>
    public MouseButton Button { get; internal set; } = default;

    /// <summary>
    /// Gets a value indicating whether the mouse button is now in the down state.
    /// </summary>
    public bool IsDown { get; internal set; } = false;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Button: {this.Button}, IsDown: {this.IsDown}, Handled: {this.Handled}";
    }
}

/// <summary>
/// Provides data for mouse wheel-related events.
/// </summary>
public class MouseWheelEventArgs : HandledEventArgs
{
    /// <summary>
    /// Gets the delta of the mouse wheel scroll. This indicates the amount and direction the wheel was scrolled.
    /// </summary>
    public float WheelDelta { get; internal set; } = 0f;
}

/// <summary>
/// Provides data for text input events.
/// </summary>
public class TextEventArgs : EventArgs
{
    /// <summary>
    /// Gets the characters that were typed.
    /// </summary>
    public IReadOnlyList<char> TypedCharacters { get; internal set; } = [];
}

/// <summary>
/// Captures and manages mouse and keyboard input from a window.
/// </summary>
public class InputManager : IDisposable
{
    /// <summary>
    /// Gets the singleton instance of the input manager.
    /// </summary>
    public static InputManager Instance { get; private set; } = null!;

    /// <summary>
    /// Occurs when a keyboard key is pressed.
    /// </summary>
    public event EventHandler<KeyboardEventArgs>? KeyPressed;

    /// <summary>
    /// Occurs when a keyboard key is released.
    /// </summary>
    public event EventHandler<KeyboardEventArgs>? KeyReleased;

    /// <summary>
    /// Occurs when a mouse button is pressed.
    /// </summary>
    public event EventHandler<MouseButtonEventArgs>? MouseButtonPressed;

    /// <summary>
    /// Occurs when a mouse button is released.
    /// </summary>
    public event EventHandler<MouseButtonEventArgs>? MouseButtonReleased;

    /// <summary>
    /// Occurs when one or more characters are typed.
    /// </summary>
    public event EventHandler<TextEventArgs>? TextEntered;

    /// <summary>
    /// Occurs when the mouse wheel is scrolled.
    /// </summary>
    public event EventHandler<MouseWheelEventArgs>? MouseWheelScrolled;

    /// <summary>
    /// Gets the mouse wheel scroll delta for the current frame.
    /// </summary>
    public float MouseScrollDelta { get; private set; }

    /// <summary>
    /// Gets the complete input snapshot for the current frame.
    /// </summary>
    public InputSnapshot InputSnapshot { get; private set; }

    private readonly Sdl2Window _window;

    private readonly HashSet<Key> _keysDown = new();

    private readonly HashSet<Key> _keysPressedThisFrame = new();

    private readonly HashSet<Key> _keysReleasedThisFrame = new();

    private readonly HashSet<MouseButton> _mouseButtonsDown = new();

    private readonly HashSet<MouseButton> _mouseButtonsPressedThisFrame = new();

    private readonly HashSet<MouseButton> _mouseButtonsReleasedThisFrame = new();

    private IReadOnlyList<char> _charactersTypedThisFrame = Array.Empty<char>();

    private Vector2 _cursorPosition;

    private bool _relativeMouseMode;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputManager"/> class.
    /// </summary>
    /// <param name="window">The window to capture input from.</param>
    /// <exception cref="Exception">Thrown if an InputManager instance has already been created.</exception>
    public InputManager(Sdl2Window window)
    {
        if (Instance != null)
            throw new Exception("Input already initialized");

        Instance = this;
        this._window = window;
        this.InputSnapshot = window.PumpEvents();
        this._window.MouseMove += this.Window_MouseMove;
    }

    /// <summary>
    /// Disposes the input manager and releases all resources.
    /// </summary>
    public void Dispose()
    {
        this._window.MouseMove -= this.Window_MouseMove;
        Instance = null!;
    }

    private void Window_MouseMove(MouseMoveEventArgs args)
    {
        if (this._relativeMouseMode)
        {
            return;
        }

        // Why using this event instead of InputSnapshot.MousePosition?
        // Well, there is a bug in which InputSnapshot.MousePosition is updated 2 frames after the mouse is moved with this._window.SetMousePosition().
        // I don't know if it's a bug in Veldrid or if I'm doing something wrong, but this event is a workaround and it works. ¯\_(ツ)_/¯
        this._cursorPosition = args.MousePosition;
    }

    /// <summary>
    /// Updates the input state for the current frame, processing all new events.
    /// </summary>
    public void UpdateState()
    {
        this.InputSnapshot = this._window.PumpEvents();

        this.MouseScrollDelta = this.InputSnapshot.WheelDelta;

        this._keysPressedThisFrame.Clear();
        this._keysReleasedThisFrame.Clear();
        this._mouseButtonsPressedThisFrame.Clear();
        this._mouseButtonsReleasedThisFrame.Clear();

        for (int i = 0; i < this.InputSnapshot.KeyEvents.Count; i++)
        {
            KeyEvent ke = this.InputSnapshot.KeyEvents[i];
            if (ke.Down)
            {
                this.ReleaseKey(ke.Key);
                this.OnKeyPressed(ke.Key);
            }
            else
            {
                this.PressKey(ke.Key);
                this.OnKeyReleased(ke.Key);
            }
        }

        for (int i = 0; i < this.InputSnapshot.MouseEvents.Count; i++)
        {
            MouseEvent me = this.InputSnapshot.MouseEvents[i];
            if (me.Down)
            {
                this.ReleaseMouseButton(me.MouseButton);
                this.OnMouseButtonPressed(me);
            }
            else
            {
                this.PressMouseButton(me.MouseButton);
                this.OnMouseButtonReleased(me);
            }
        }

        this._charactersTypedThisFrame = this.InputSnapshot.KeyCharPresses;

        if (this._charactersTypedThisFrame.Count > 0)
        {
            this.OnTextEntered(this._charactersTypedThisFrame);
        }

        if (this.InputSnapshot.WheelDelta != 0f)
        {
            this.OnMouseWheelScrolled(this.InputSnapshot.WheelDelta);
        }
    }

    /// <summary>
    /// Gets the size of the display used for capturing input.
    /// </summary>
    public Vector2Int DisplaySize => new(this._window.Width, this._window.Height);

    /// <summary>
    /// Moves the mouse cursor to the specified position relative to the window.
    /// </summary>
    /// <param name="position">The new position of the mouse cursor.</param>
    public void SetCursorPosition(Vector2 position)
    {
        position.X = (int)position.X;
        position.Y = (int)position.Y;
        this._window.SetMousePosition(position);
        this._cursorPosition = position;
    }

    /// <summary>
    /// Gets the current mouse position in window client space.
    /// </summary>
    public Vector2 CursorPosition => this._cursorPosition;

    /// <summary>
    /// Gets the mouse movement delta for the current frame. Only meaningful when relative mouse mode is enabled.
    /// </summary>
    public Vector2 MouseDelta => this._window.MouseDelta;

    /// <summary>
    /// Enables or disables relative mouse mode. When enabled, the cursor is hidden and confined to the window,
    /// and only movement deltas are reported. The mode is automatically released when the window loses focus.
    /// </summary>
    /// <param name="enabled">Whether to enable relative mouse mode.</param>
    public void SetRelativeMouseMode(bool enabled)
    {
        this._relativeMouseMode = enabled;
        this._window.CursorRelativeMode = enabled;
    }

    /// <summary>
    /// Gets a value indicating whether the specified key is currently held down.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key is down; otherwise, false.</returns>
    public bool IsKeyDown(Key key)
    {
        return this._keysDown.Contains(key);
    }

    /// <summary>
    /// Gets a value indicating whether the specified key was pressed during the current frame.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key was pressed this frame; otherwise, false.</returns>
    public bool WasKeyPressedThisFrame(Key key)
    {
        return this._keysPressedThisFrame.Contains(key);
    }

    /// <summary>
    /// Gets a value indicating whether the specified key was released during the current frame.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key was released this frame; otherwise, false.</returns>
    public bool WasKeyReleasedThisFrame(Key key)
    {
        return this._keysReleasedThisFrame.Contains(key);
    }

    /// <summary>
    /// Gets a value indicating whether the specified mouse button is currently held down.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>True if the mouse button is down; otherwise, false.</returns>
    public bool IsMouseButtonDown(MouseButton button)
    {
        return this._mouseButtonsDown.Contains(button);
    }

    /// <summary>
    /// Gets a value indicating whether the specified mouse button was pressed during the current frame.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>True if the mouse button was pressed this frame; otherwise, false.</returns>
    public bool WasMouseButtonPressedThisFrame(MouseButton button)
    {
        return this._mouseButtonsPressedThisFrame.Contains(button);
    }

    /// <summary>
    /// Gets a value indicating whether the specified mouse button was released during the current frame.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>True if the mouse button was released this frame; otherwise, false.</returns>
    public bool WasMouseButtonReleasedThisFrame(MouseButton button)
    {
        return this._mouseButtonsReleasedThisFrame.Contains(button);
    }

    /// <summary>
    /// Manually injects a mouse button press event.
    /// </summary>
    /// <param name="mouseButton">The mouse button to press.</param>
    public void PressMouseButton(MouseButton mouseButton)
    {
        this._mouseButtonsDown.Remove(mouseButton);
        this._mouseButtonsPressedThisFrame.Remove(mouseButton);
        this._mouseButtonsReleasedThisFrame.Add(mouseButton);
    }

    /// <summary>
    /// Manually injects a mouse button release event.
    /// </summary>
    /// <param name="mouseButton">The mouse button to release.</param>
    public void ReleaseMouseButton(MouseButton mouseButton)
    {
        if (this._mouseButtonsDown.Add(mouseButton))
        {
            this._mouseButtonsPressedThisFrame.Add(mouseButton);
            this._mouseButtonsReleasedThisFrame.Remove(mouseButton);
        }
    }

    /// <summary>
    /// Manually injects a key press event.
    /// </summary>
    /// <param name="key">The key to press.</param>
    public void PressKey(Key key)
    {
        this._keysDown.Remove(key);
        this._keysPressedThisFrame.Remove(key);
        this._keysReleasedThisFrame.Add(key);
    }

    /// <summary>
    /// Manually injects a key release event.
    /// </summary>
    /// <param name="key">The key to release.</param>
    public void ReleaseKey(Key key)
    {
        if (this._keysDown.Add(key))
        {
            this._keysPressedThisFrame.Add(key);
            this._keysReleasedThisFrame.Remove(key);
        }
    }

    /// <summary>
    /// Gets a read-only list of characters typed during the current frame.
    /// </summary>
    public IReadOnlyList<char> TypedCharacters => this._charactersTypedThisFrame;

    private readonly KeyboardEventArgs _keyboardEventArgs = new KeyboardEventArgs();
    private readonly MouseButtonEventArgs _mouseButtonEventArgs = new MouseButtonEventArgs();
    private readonly MouseWheelEventArgs _mouseWheelEventArgs = new MouseWheelEventArgs();
    private readonly TextEventArgs _textEventArgs = new TextEventArgs();

    /// <summary>
    /// Raises the <see cref="KeyPressed"/> event.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    protected void OnKeyPressed(Key key)
    {
        this._keyboardEventArgs.Key = key;
        this._keyboardEventArgs.IsDown = true;
        this._keyboardEventArgs.Handled = false;
        this.KeyPressed?.Invoke(this, this._keyboardEventArgs);
    }

    /// <summary>
    /// Raises the <see cref="KeyReleased"/> event.
    /// </summary>
    /// <param name="key">The key that was released.</param>
    protected void OnKeyReleased(Key key)
    {
        this._keyboardEventArgs.Key = key;
        this._keyboardEventArgs.IsDown = false;
        this._keyboardEventArgs.Handled = false;
        this.KeyReleased?.Invoke(this, this._keyboardEventArgs);
    }

    /// <summary>
    /// Raises the <see cref="MouseButtonPressed"/> event.
    /// </summary>
    /// <param name="mouseEvent">The mouse event that was pressed.</param>
    protected void OnMouseButtonPressed(MouseEvent mouseEvent)
    {
        this._mouseButtonEventArgs.Button = mouseEvent.MouseButton;
        this._mouseButtonEventArgs.Handled = false;
        this.MouseButtonPressed?.Invoke(this, this._mouseButtonEventArgs);
    }

    /// <summary>
    /// Raises the <see cref="MouseButtonReleased"/> event.
    /// </summary>
    /// <param name="mouseEvent">The original mouse event.</param>
    protected virtual void OnMouseButtonReleased(MouseEvent mouseEvent)
    {
        this._mouseButtonEventArgs.Button = mouseEvent.MouseButton;
        this._mouseButtonEventArgs.Handled = false;
        this.MouseButtonReleased?.Invoke(this, this._mouseButtonEventArgs);
    }

    /// <summary>
    /// Raises the <see cref="TextEntered"/> event.
    /// </summary>
    /// <param name="characters">The characters that were typed.</param>
    protected void OnTextEntered(IReadOnlyList<char> characters)
    {
        this._textEventArgs.TypedCharacters = characters;
        this.TextEntered?.Invoke(this, this._textEventArgs);
    }

    /// <summary>
    /// Raises the <see cref="MouseWheelScrolled"/> event.
    /// </summary>
    /// <param name="delta">The amount the mouse wheel was scrolled.</param>
    protected void OnMouseWheelScrolled(float delta)
    {
        this._mouseWheelEventArgs.WheelDelta = delta;
        this.MouseWheelScrolled?.Invoke(this, this._mouseWheelEventArgs);
    }
}
