using System;
using System.Collections.Generic;
using System.Numerics;
using Support.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace Imago.Input;

/// <summary>
/// Captures and manages the mouse and keyboard input.
/// </summary>
public class InputManager : IDisposable
{
    private static InputManager _instance = null!;

    /// <summary>
    /// Gets the instance of the input.
    /// </summary>
    public static InputManager Current
    {
        protected set => _instance = value;
        get => _instance;
    }

    /// <summary>
    /// Occurs when a key is pressed.
    /// </summary>
    public event EventHandler<KeyEvent>? KeyPressed;

    /// <summary>
    /// Occurs when a key is released.
    /// </summary>
    public event EventHandler<KeyEvent>? KeyReleased;

    /// <summary>
    /// Occurs when a mouse button is pressed.
    /// </summary>
    public event EventHandler<MouseEvent>? MouseButtonPressed;

    /// <summary>
    /// Occurs when a mouse button is released.
    /// </summary>
    public event EventHandler<MouseEvent>? MouseButtonReleased;

    /// <summary>
    /// Occurs when one or more characters are typed.
    /// </summary>
    public event EventHandler<TextEventArgs>? TextEntered;

    /// <summary>
    /// Gets the mouse wheel delta for this frame.
    /// </summary>
    public float MouseWheelDelta { get; private set; }

    /// <summary>
    /// Gets the Input Snapshot for the current frame.
    /// </summary>
    public InputSnapshot InputSnapshot { get; private set; }

    private readonly Sdl2Window _window;

    private readonly HashSet<Key> _currentlyPressedKeys = new();

    private readonly HashSet<Key> _newKeysThisFrame = new();

    private readonly HashSet<Key> _releasedKeysThisFrame = new();

    private readonly HashSet<MouseButton> _currentlyPressedMouseButtons = new();

    private readonly HashSet<MouseButton> _newMouseButtonsThisFrame = new();

    private readonly HashSet<MouseButton> _releasedMouseButtonsThisFrame = new();

    private IReadOnlyList<char> _typedCharactersThisFrame = Array.Empty<char>();

    private Vector2 _mousePosition;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputManager"/> class.
    /// </summary>
    /// <param name="window">The window to capture input from.</param>
    /// <exception cref="Exception">Thrown if the Input instance has already been created.</exception>
    public InputManager(Sdl2Window window)
    {
        if (_instance != null)
            throw new Exception("Input already initialized");

        _instance = this;
        this._window = window;
        this.InputSnapshot = window.PumpEvents();
        this._window.MouseMove += this.Window_MouseMove;
    }

    public void Dispose()
    {
        this._window.MouseMove -= this.Window_MouseMove;
    }

    private void Window_MouseMove(MouseMoveEventArgs args)
    {
        // Why using this event instead of InputSnapshot.MousePosition?
        // Well, there is a bug in which InputSnapshot.MousePosition is updated 2 frames after the mouse is moved with this._window.SetMousePosition().
        // I don't know if it's a bug in Veldrid or if I'm doing something wrong, but this event is a workaround and it works. ¯\_(ツ)_/¯
        this._mousePosition = args.MousePosition;
    }

    /// <summary>
    /// Updates the input state.
    /// </summary>
    public void UpdateFrameInput()
    {
        this.InputSnapshot = this._window.PumpEvents();

        this.MouseWheelDelta = this.InputSnapshot.WheelDelta;

        this._newKeysThisFrame.Clear();
        this._releasedKeysThisFrame.Clear();
        this._newMouseButtonsThisFrame.Clear();
        this._releasedMouseButtonsThisFrame.Clear();

        for (int i = 0; i < this.InputSnapshot.KeyEvents.Count; i++)
        {
            KeyEvent ke = this.InputSnapshot.KeyEvents[i];
            if (ke.Down)
            {
                this.ReleaseKey(ke.Key);
                this.KeyPressed?.Invoke(this, ke);
            }
            else
            {
                this.PressKey(ke.Key);
                this.KeyReleased?.Invoke(this, ke);
            }
        }

        for (int i = 0; i < this.InputSnapshot.MouseEvents.Count; i++)
        {
            MouseEvent me = this.InputSnapshot.MouseEvents[i];
            if (me.Down)
            {
                this.ReleaseMouseButton(me.MouseButton);
                this.MouseButtonPressed?.Invoke(this, me);
            }
            else
            {
                this.PressMouseButton(me.MouseButton);
                this.MouseButtonReleased?.Invoke(this, me);
            }
        }

        this._typedCharactersThisFrame = this.InputSnapshot.KeyCharPresses;

        if (this.TextEntered != null && this._typedCharactersThisFrame.Count > 0)
            this.TextEntered.Invoke(this, new TextEventArgs(this._typedCharactersThisFrame));
    }

    /// <summary>
    /// Gets the size of the display used for capturing input.
    /// </summary>
    public Vector2Int DisplaySize => new(this._window.Width, this._window.Height);

    /// <summary>
    /// Moves the mouse cursor to the specified position.
    /// </summary>
    /// <param name="position">The new position of the mouse cursor.</param>
    public void MoveMouseTo(Vector2 position)
    {
        position.X = (int)position.X;
        position.Y = (int)position.Y;
        this._window.SetMousePosition(position);
        this._mousePosition = position;
    }

    /// <summary>
    /// Gets the current mouse position.
    /// </summary>
    public Vector2 MousePosition => this._mousePosition;

    /// <summary>
    /// Gets or sets whether the mouse is currently visible.
    /// </summary>
    public bool CursorIsVisible
    {
        get => this._window.CursorVisible;
        set => this._window.CursorVisible = value;
    }

    /// <summary>
    /// Gets whether the specified key is currently pressed.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>Whether the specified key is currently pressed.</returns>
    public bool GetKey(Key key)
    {
        return this._currentlyPressedKeys.Contains(key);
    }

    /// <summary>
    /// Gets whether the specified key was pressed this frame.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>Whether the specified key was pressed this frame.</returns>
    public bool GetKeyDown(Key key)
    {
        return this._newKeysThisFrame.Contains(key);
    }

    /// <summary>
    /// Gets whether the specified key was released this frame.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>Whether the specified key was released this frame.</returns>
    public bool GetKeyUp(Key key)
    {
        return this._releasedKeysThisFrame.Contains(key);
    }

    /// <summary>
    /// Gets whether the specified mouse button is currently pressed.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>Whether the specified mouse button is currently pressed.</returns>
    public bool GetMouseButton(MouseButton button)
    {
        return this._currentlyPressedMouseButtons.Contains(button);
    }

    /// <summary>
    /// Gets whether the specified mouse button was pressed this frame.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>Whether the specified mouse button was pressed this frame.</returns>
    public bool GetMouseButtonDown(MouseButton button)
    {
        return this._newMouseButtonsThisFrame.Contains(button);
    }

    /// <summary>
    /// Gets whether the specified mouse button was released this frame.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>Whether the specified mouse button was released this frame.</returns>
    public bool GetMouseButtonUp(MouseButton button)
    {
        return this._releasedMouseButtonsThisFrame.Contains(button);
    }

    /// <summary>
    /// Simulates pressing the specified mouse button.
    /// </summary>
    /// <param name="mouseButton">The mouse button to press.</param>
    public void PressMouseButton(MouseButton mouseButton)
    {
        this._currentlyPressedMouseButtons.Remove(mouseButton);
        this._newMouseButtonsThisFrame.Remove(mouseButton);
        this._releasedMouseButtonsThisFrame.Add(mouseButton);
    }

    /// <summary>
    /// Simulates releasing the specified mouse button.
    /// </summary>
    /// <param name="mouseButton">The mouse button to release.</param>
    public void ReleaseMouseButton(MouseButton mouseButton)
    {
        if (this._currentlyPressedMouseButtons.Add(mouseButton))
        {
            this._newMouseButtonsThisFrame.Add(mouseButton);
            this._releasedMouseButtonsThisFrame.Remove(mouseButton);
        }
    }

    /// <summary>
    /// Simulates pressing the specified key.
    /// </summary>
    /// <param name="key">The key to press.</param>
    public void PressKey(Key key)
    {
        this._currentlyPressedKeys.Remove(key);
        this._newKeysThisFrame.Remove(key);
        this._releasedKeysThisFrame.Add(key);
    }

    /// <summary>
    /// Simulates releasing the specified key.
    /// </summary>
    /// <param name="key">The key to release.</param>
    public void ReleaseKey(Key key)
    {
        if (this._currentlyPressedKeys.Add(key))
        {
            this._newKeysThisFrame.Add(key);
            this._releasedKeysThisFrame.Remove(key);
        }
    }

    /// <summary>
    /// Gets a read-only list of the currently pressed characters.
    /// </summary>
    public IReadOnlyList<char> TypedCharacters => this._typedCharactersThisFrame;
}
