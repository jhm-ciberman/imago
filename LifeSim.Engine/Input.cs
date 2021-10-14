using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace LifeSim.Engine
{
    public static class Input
    {
        public static void SetInstance(InputInstance instance) => Input._instance = instance;
        private static InputInstance? _instance = null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static InputInstance _GetInstance()
        {
            if (Input._instance == null)
            {
                throw new System.Exception("Input is not initialized");
            }
            return Input._instance;
        }

        public static bool GetKey(Key key) => Input._GetInstance().GetKey(key);
        public static bool GetKeyDown(Key key) => Input._GetInstance().GetKeyDown(key);
        public static bool GetMouseButton(MouseButton button) => Input._GetInstance().GetMouseButton(button);
        public static bool GetMouseButtonDown(MouseButton button) => Input._GetInstance().GetMouseButtonDown(button);

        public static Vector2 MouseDelta => Input._GetInstance().MouseDelta;
        public static Vector2 MousePosition => Input._GetInstance().MousePosition;
        public static bool MouseIsLocked => Input._GetInstance().MouseIsLocked;
        public static void LockMouse() => Input._GetInstance().LockMouse();
        public static void UnlockMouse() => Input._GetInstance().UnlockMouse();
    }
}