using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using LifeSim.Support.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace LifeSim.Imago.Input;

public class KeyboardEventArgs : HandledEventArgs
{
    /// <summary>
    /// Gets the key that was pressed or released.
    /// </summary>
    public Key Key { get; internal set; } = default;

    /// <summary>
    /// Gets the new state of the key. True if the key is now pressed, false if it is released.
    /// </summary>
    public bool IsDown { get; internal set; } = false;

    public override string ToString()
    {
        return $"Key: {this.Key}, Handled: {this.Handled}";
    }
}

public class MouseButtonEventArgs : HandledEventArgs
{
    /// <summary>
    /// Gets the mouse button that was pressed or released.
    /// </summary>
    public MouseButton Button { get; internal set; } = default;

    /// <summary>
    /// Gets the new state of the mouse button. True if the button is now pressed, false if it is released.
    /// </summary>
    public bool IsDown { get; internal set; } = false;

    public override string ToString()
    {
        return $"Button: {this.Button}, IsDown: {this.IsDown}, Handled: {this.Handled}";
    }
}

public class MouseWheelEventArgs : HandledEventArgs
{
    /// <summary>
    /// Gets the delta of the mouse wheel scroll. This how much the wheel was scrolled.
    /// </summary>
    public float WheelDelta { get; internal set; } = 0f;
}

public class TextEventArgs : EventArgs
{
    /// <summary>
    /// Gets the characters that were typed.
    /// </summary>
    public IReadOnlyList<char> TypedCharacters { get; internal set; } = [];
}

/// <summary>
/// Captures and manages the mouse and keyboard input.
/// </summary>
public class InputManager : IDisposable
{
    /// <summary>
    /// Gets the instance of the input.
    /// </summary>
    public static InputManager Instance { get; private set; } = null!;

    /// <summary>
    /// Occurs when a key is pressed.
    /// </summary>
    public event EventHandler<KeyboardEventArgs>? KeyPressed;

    /// <summary>
    /// Occurs when a key is released.
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
    /// Gets the mouse wheel delta for this frame.
    /// </summary>
    public float MouseScrollDelta { get; private set; }

    /// <summary>
    /// Gets the Input Snapshot for the current frame.
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

    /// <summary>
    /// Initializes a new instance of the <see cref="InputManager"/> class.
    /// </summary>
    /// <param name="window">The window to capture input from.</param>
    /// <exception cref="Exception">Thrown if the Input instance has already been created.</exception>
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
        // Why using this event instead of InputSnapshot.MousePosition?
        // Well, there is a bug in which InputSnapshot.MousePosition is updated 2 frames after the mouse is moved with this._window.SetMousePosition().
        // I don't know if it's a bug in Veldrid or if I'm doing something wrong, but this event is a workaround and it works. ¯\_(ツ)_/¯
        this._cursorPosition = args.MousePosition;
    }

    /// <summary>
    /// Updates the input state.
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
    /// Gets the current mouse position in window space.
    /// </summary>
    public Vector2 CursorPosition => this._cursorPosition;

    /// <summary>
    /// Gets whether the specified key is currently pressed.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>Whether the specified key is currently pressed.</returns>
    public bool IsKeyDown(Key key)
    {
        return this._keysDown.Contains(key);
    }

    /// <summary>
    /// Gets whether the specified key was pressed this frame.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>Whether the specified key was pressed this frame.</returns>
    public bool WasKeyPressedThisFrame(Key key)
    {
        return this._keysPressedThisFrame.Contains(key);
    }

    /// <summary>
    /// Gets whether the specified key was released this frame.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>Whether the specified key was released this frame.</returns>
    public bool WasKeyReleasedThisFrame(Key key)
    {
        return this._keysReleasedThisFrame.Contains(key);
    }

    /// <summary>
    /// Gets whether the specified mouse button is currently pressed.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>Whether the specified mouse button is currently pressed.</returns>
    public bool IsMouseButtonDown(MouseButton button)
    {
        return this._mouseButtonsDown.Contains(button);
    }

    /// <summary>
    /// Gets whether the specified mouse button was pressed this frame.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>Whether the specified mouse button was pressed this frame.</returns>
    public bool WasMouseButtonPressedThisFrame(MouseButton button)
    {
        return this._mouseButtonsPressedThisFrame.Contains(button);
    }

    /// <summary>
    /// Gets whether the specified mouse button was released this frame.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>Whether the specified mouse button was released this frame.</returns>
    public bool WasMouseButtonReleasedThisFrame(MouseButton button)
    {
        return this._mouseButtonsReleasedThisFrame.Contains(button);
    }

    /// <summary>
    /// Simulates pressing the specified mouse button.
    /// </summary>
    /// <param name="mouseButton">The mouse button to press.</param>
    public void PressMouseButton(MouseButton mouseButton)
    {
        this._mouseButtonsDown.Remove(mouseButton);
        this._mouseButtonsPressedThisFrame.Remove(mouseButton);
        this._mouseButtonsReleasedThisFrame.Add(mouseButton);
    }

    /// <summary>
    /// Simulates releasing the specified mouse button.
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
    /// Simulates pressing the specified key.
    /// </summary>
    /// <param name="key">The key to press.</param>
    public void PressKey(Key key)
    {
        this._keysDown.Remove(key);
        this._keysPressedThisFrame.Remove(key);
        this._keysReleasedThisFrame.Add(key);
    }

    /// <summary>
    /// Simulates releasing the specified key.
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
    /// Gets a read-only list of the currently pressed characters.
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
