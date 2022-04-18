using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace LifeSim.Engine;

public class InputInstance
{
    private readonly Sdl2Window _window;

    private readonly HashSet<Key> _currentlyPressedKeys = new HashSet<Key>();
    private readonly HashSet<Key> _newKeysThisFrame = new HashSet<Key>();

    private readonly HashSet<Key> _releasedKeysThisFrame = new HashSet<Key>();

    private readonly HashSet<MouseButton> _currentlyPressedMouseButtons = new HashSet<MouseButton>();
    private readonly HashSet<MouseButton> _newMouseButtonsThisFrame = new HashSet<MouseButton>();

    private readonly HashSet<MouseButton> _releasedMouseButtonsThisFrame = new HashSet<MouseButton>();

    private Vector2 _mousePosition;
    private Vector2 _mouseDelta;

    public InputSnapshot InputSnapshot { get; private set; }

    public InputInstance(Sdl2Window window)
    {
        this._window = window;
        this.InputSnapshot = window.PumpEvents();
    }

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
                this.KeyDown(ke.Key);
            }
            else
            {
                this.KeyUp(ke.Key);
            }
        }
        for (int i = 0; i < this.InputSnapshot.MouseEvents.Count; i++)
        {
            MouseEvent me = this.InputSnapshot.MouseEvents[i];
            if (me.Down)
            {
                this.MouseDown(me.MouseButton);
            }
            else
            {
                this.MouseUp(me.MouseButton);
            }
        }

        var newPos = this.InputSnapshot.MousePosition;
        var center = new Vector2(this._window.Width / 2, this._window.Height / 2);
        this._mouseDelta = newPos - center;
        this._mousePosition = newPos;
    }

    public void MoveMouseToCenter()
    {
        var center = new Vector2(this._window.Width / 2, this._window.Height / 2);
        this._window.SetMousePosition(center);
        this._mouseDelta = new Vector2(0, 0);
        this._mousePosition = center;
    }

    public Vector2 MousePosition => this._mousePosition;
    public Vector2 MouseDelta => this._mouseDelta;
    public bool CursorIsVisible
    {
        get => this._window.CursorVisible;
        set => this._window.CursorVisible = value;
    }

    public bool GetKey(Key key)
    {
        return this._currentlyPressedKeys.Contains(key);
    }

    public bool GetKeyDown(Key key)
    {
        return this._newKeysThisFrame.Contains(key);
    }

    public bool GetKeyUp(Key key)
    {
        return this._releasedKeysThisFrame.Contains(key);
    }

    public bool GetMouseButton(MouseButton button)
    {
        return this._currentlyPressedMouseButtons.Contains(button);
    }

    public bool GetMouseButtonDown(MouseButton button)
    {
        return this._newMouseButtonsThisFrame.Contains(button);
    }

    public bool GetMouseButtonUp(MouseButton button)
    {
        return this._releasedMouseButtonsThisFrame.Contains(button);
    }

    private void MouseUp(MouseButton mouseButton)
    {
        this._currentlyPressedMouseButtons.Remove(mouseButton);
        this._newMouseButtonsThisFrame.Remove(mouseButton);
        this._releasedMouseButtonsThisFrame.Add(mouseButton);
    }

    private void MouseDown(MouseButton mouseButton)
    {
        if (this._currentlyPressedMouseButtons.Add(mouseButton))
        {
            this._newMouseButtonsThisFrame.Add(mouseButton);
            this._releasedMouseButtonsThisFrame.Remove(mouseButton);
        }
    }

    private void KeyUp(Key key)
    {
        this._currentlyPressedKeys.Remove(key);
        this._newKeysThisFrame.Remove(key);
        this._releasedKeysThisFrame.Add(key);
    }

    private void KeyDown(Key key)
    {
        if (this._currentlyPressedKeys.Add(key))
        {
            this._newKeysThisFrame.Add(key);
            this._releasedKeysThisFrame.Remove(key);
        }
    }
}