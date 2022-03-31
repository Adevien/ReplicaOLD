using System;

namespace Replica.Structures {

    [Serializable]
    public struct NetworkInput {
        public uint Buttons;

        public bool IsUp(ushort button) => IsDown(button) == false;

        public bool IsDown(ushort button) => (Buttons & button) == button;

        public void Reset() => Buttons = 0;
    }

    public static class Key {
        public const byte BTN_FORWARD = 1 << 1;
        public const byte BTN_BACKWARD = 1 << 2;
        public const byte BTN_LEFTWARD = 1 << 3;
        public const byte BTN_RIGHTWARD = 1 << 4;
        public const byte BTN_UPWARD = 1 << 5;
    }
}
