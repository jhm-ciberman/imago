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

    private readonly HashSet<MouseButton> _currentlyPressedMouseButtons = new HashSet<MouseButton>();
    private readonly HashSet<MouseButton> _newMouseButtonsThisFrame = new HashSet<MouseButton>();

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
        this._newMouseButtonsThisFrame.Clear();


        for (int i = 0; i < this.InputSnapshot.KeyEvents.Count; i++)
        {
            KeyEvent ke = this.InputSnapshot.KeyEvents[i];
            if (ke.Down)
            {
                this._KeyDown(ke.Key);
            }
            else
            {
                this._KeyUp(ke.Key);
            }
        }
        for (int i = 0; i < this.InputSnapshot.MouseEvents.Count; i++)
        {
            MouseEvent me = this.InputSnapshot.MouseEvents[i];
            if (me.Down)
            {
                this._MouseDown(me.MouseButton);
            }
            else
            {
                this._MouseUp(me.MouseButton);
            }
        }

        var newPos = this.InputSnapshot.MousePosition;
        var center = new Vector2(this._window.Width / 2, this._window.Height / 2);
        this._mouseDelta = newPos - center;
        this._mousePosition = newPos;
        if (this.MouseIsLocked)
        {
            this._window.SetMousePosition(center);
        }
    }

    public void LockMouse()
    {
        var center = new Vector2(this._window.Width / 2, this._window.Height / 2);
        this._window.SetMousePosition(center);
        this._mouseDelta = new Vector2(0, 0);
        this._mousePosition = center;
        this.MouseIsLocked = true;
        this._window.CursorVisible = false;
    }

    public void UnlockMouse()
    {
        this.MouseIsLocked = false;
        this._window.CursorVisible = true;
    }

    public Vector2 MousePosition => this._mousePosition;
    public Vector2 MouseDelta => this._mouseDelta;
    public bool MouseIsLocked { get; private set; } = false;

    public bool GetKey(Key key) => this._currentlyPressedKeys.Contains(key);
    public bool GetKeyDown(Key key) => this._newKeysThisFrame.Contains(key);
    public bool GetMouseButton(MouseButton button) => this._currentlyPressedMouseButtons.Contains(button);
    public bool GetMouseButtonDown(MouseButton button) => this._newMouseButtonsThisFrame.Contains(button);

    private void _MouseUp(MouseButton mouseButton)
    {
        this._currentlyPressedMouseButtons.Remove(mouseButton);
        this._newMouseButtonsThisFrame.Remove(mouseButton);
    }

    private void _MouseDown(MouseButton mouseButton)
    {
        if (this._currentlyPressedMouseButtons.Add(mouseButton))
        {
            this._newMouseButtonsThisFrame.Add(mouseButton);
        }
    }

    private void _KeyUp(Key key)
    {
        this._currentlyPressedKeys.Remove(key);
        this._newKeysThisFrame.Remove(key);
    }

    private void _KeyDown(Key key)
    {
        if (this._currentlyPressedKeys.Add(key))
        {
            this._newKeysThisFrame.Add(key);
        }
    }
}