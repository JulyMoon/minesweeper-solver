using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MinesweeperSolver
{
    public static class Mouse
    {
        private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        private const uint MOUSEEVENTF_LEFTUP = 0x04;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const uint MOUSEEVENTF_RIGHTUP = 0x10;

        private static uint LeftClickFlags = MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP;
        private static uint RightClickFlags = MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP;

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern long SetCursorPos(int x, int y);

        private static void Click(uint flags)
            => mouse_event(flags, 0, 0, 0, UIntPtr.Zero);

        private static void DoubleClickDelay()
            => Thread.Sleep(150);

        public static void SetPosition(int x, int y)
            => SetCursorPos(x, y);

        public static void LeftClick()
            => Click(LeftClickFlags);

        public static void RightClick()
            => Click(RightClickFlags);

        public static void DoubleButtonClick()
        {
            Click(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_RIGHTDOWN);
            Thread.Sleep(100);
            Click(MOUSEEVENTF_LEFTUP | MOUSEEVENTF_RIGHTUP);
        }

        public static void LeftDoubleClick()
        {
            LeftClick();
            DoubleClickDelay();
            LeftClick();
        }
    }
}
