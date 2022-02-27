using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace LifeSim.Engine;

public static class Input
{
    public static void SetInstance(InputInstance instance) => Input._instance = instance;
    private static InputInstance? _instance = null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static InputInstance GetInstance()
    {
        if (Input._instance == null)
        {
            throw new System.Exception("Input is not initialized");
        }
        return Input._instance;
    }

    public static bool GetKey(Key key) => Input.GetInstance().GetKey(key);
    public static bool GetKeyDown(Key key) => Input.GetInstance().GetKeyDown(key);
    public static bool GetKeyUp(Key key) => Input.GetInstance().GetKeyUp(key);

    public static bool GetMouseButton(MouseButton button) => Input.GetInstance().GetMouseButton(button);
    public static bool GetMouseButtonDown(MouseButton button) => Input.GetInstance().GetMouseButtonDown(button);
    public static bool GetMouseButtonUp(MouseButton button) => Input.GetInstance().GetMouseButtonUp(button);

    public static Vector2 MouseDelta => Input.GetInstance().MouseDelta;
    public static Vector2 MousePosition => Input.GetInstance().MousePosition;
    public static bool MouseIsLocked => Input.GetInstance().MouseIsLocked;
    public static void LockMouse() => Input.GetInstance().LockMouse();
    public static void UnlockMouse() => Input.GetInstance().UnlockMouse();
}