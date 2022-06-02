using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace LifeSim.Engine;

public static class Input
{
    public static void SetInstance(InputInstance instance) => Input._instance = instance;
    private static InputInstance? _instance = null;

    public static InputInstance Instance
    {
        set => _instance = value;
        get
        {
            if (_instance == null)
            {
                throw new System.Exception("Input is not initialized");
            }
            return _instance;
        }
    }

    public static bool GetKey(Key key) => Input.Instance.GetKey(key);
    public static bool GetKeyDown(Key key) => Input.Instance.GetKeyDown(key);
    public static bool GetKeyUp(Key key) => Input.Instance.GetKeyUp(key);

    public static bool GetMouseButton(MouseButton button) => Input.Instance.GetMouseButton(button);
    public static bool GetMouseButtonDown(MouseButton button) => Input.Instance.GetMouseButtonDown(button);
    public static bool GetMouseButtonUp(MouseButton button) => Input.Instance.GetMouseButtonUp(button);

    public static Vector2 MouseDelta => Input.Instance.MouseDelta;
    public static Vector2 MousePosition => Input.Instance.MousePosition;
    public static bool CursorIsVisible
    {
        get => Input.Instance.CursorIsVisible;
        set => Input.Instance.CursorIsVisible = value;
    }
    public static float MouseWheelDelta => Input.Instance.MouseWheelDelta;

    public static void MoveCursorToCenter() => Input.Instance.MoveMouseToCenter();
}