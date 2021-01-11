using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;
using Veldrid.Sdl2;

namespace LifeSim
{
    public static class Input
    {   
        public static void SetInstance(InputInstance instance) => Input._instance = instance;
        private static InputInstance? _instance = null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static InputInstance _GetInstance()
        {
            if (Input._instance == null) {
                throw new System.Exception("Input is not initialized");
            }
            return Input._instance;
        }

        public static bool GetKey(Key key)                        => Input._GetInstance().GetKey(key);
        public static bool GetKeyDown(Key key)                    => Input._GetInstance().GetKeyDown(key);
        public static bool GetMouseButton(MouseButton button)     => Input._GetInstance().GetMouseButton(button);
        public static bool GetMouseButtonDown(MouseButton button) => Input._GetInstance().GetMouseButtonDown(button);

        public static Vector2 mouseDelta      => Input._GetInstance().mouseDelta;
        public static Vector2 mousePosition   => Input._GetInstance().mousePosition;
        public static bool mouseIsLocked      => Input._GetInstance().mouseIsLocked;
        public static void LockMouse()        => Input._GetInstance().LockMouse();
        public static void UnlockMouse()      => Input._GetInstance().UnlockMouse();
    }
}