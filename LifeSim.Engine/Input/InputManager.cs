using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace LifeSim.Engine.Input;

/// <summary>
/// Captures and manages the mouse and keyboard input.
/// </summary>
public class InputManager
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
    /// Event that is raised when a key is pressed.
    /// </summary>
    public event EventHandler<KeyEvent>? KeyPressed;

    /// <summary>
    /// Event that is raised when a key is released.
    /// </summary>
    public event EventHandler<KeyEvent>? KeyReleased;

    /// <summary>
    /// Event that is raised when a mouse button is pressed.
    /// </summary>
    public event EventHandler<MouseEvent>? MouseButtonPressed;

    /// <summary>
    /// Event that is raised when a mouse button is released.
    /// </summary>
    public event EventHandler<MouseEvent>? MouseButtonReleased;

    /// <summary>
    /// Event that is raised when one or more characters are typed.
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

    private Vector2 _mouseDelta;

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
    }

    /// <summary>
    /// Updates the input state.
    /// </summary>
    public void UpdateFrameInput()
    {
        this.InputSnapshot = this._window.PumpEvents(); //For next frame
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

        var newPos = this.InputSnapshot.MousePosition;
        var center = new Vector2(this._window.Width / 2, this._window.Height / 2);
        this._mouseDelta = newPos - center;
        this._mousePosition = newPos;

        this.MouseWheelDelta = this.InputSnapshot.WheelDelta;
    }

    /// <summary>
    /// Moves the mouse cursor to the center of the screen.
    /// </summary>
    public void MoveMouseToCenter()
    {
        var center = new Vector2(this._window.Width / 2, this._window.Height / 2);
        this._window.SetMousePosition(center);
        this._mouseDelta = new Vector2(0, 0);
        this._mousePosition = center;
    }

    /// <summary>
    /// Gets the current mouse position.
    /// </summary>
    public Vector2 MousePosition => this._mousePosition;

    /// <summary>
    /// Gets the mouse delta since the last frame.
    /// </summary>
    public Vector2 MouseDelta => this._mouseDelta;

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
