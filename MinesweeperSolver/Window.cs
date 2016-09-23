using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver
{
    public class Window
    {
        private const int mineSize = 16;
        private const int xFieldAdjust = 22;
        private const int yFieldAdjust = 108;
        private const int widthExcess = 26;
        private const int heightExcess = 112;
        private const string windowTitle = "Minesweeper";

        private bool windowFound;
        private int width, height;
        private IntPtr handle;
        private int wx, wy;
        private int mineCount = 99; // todo
        
        public bool WindowFound => windowFound;
        public int FieldWidth => width;
        public int FieldHeight => height;
        public int MineCount => mineCount;

        public Window()
        {
            var process = GetProcess();

            if (process == null)
            {
                Console.WriteLine($"Window not found!");
                return;
            }
            else
            {
                windowFound = true;
                Console.WriteLine($"Window found! PID: {process.Id}");
            }

            handle = process.MainWindowHandle;
            Initialize();
            BringToFront();
            Console.WriteLine($"X: {wx}\nY: {wy}\nField width: {width}\nField height: {height}");
        }
        
        private void SetMouseOverCell(int x, int y)
            => Mouse.SetPosition(wx + xFieldAdjust + mineSize * x, wy + yFieldAdjust + mineSize * y);

        public void OpenCell(int x, int y)
        {
            SetMouseOverCell(x, y);
            Mouse.LeftClick();
        }

        public void MassOpenCell(int x, int y)
        {
            SetMouseOverCell(x, y);
            Mouse.DoubleButtonClick();
        }

        private void Initialize()
        {
            var rect = new WindowRectangle();
            GetWindowRect(handle, ref rect);

            wx = rect.Left;
            wy = rect.Top;
            width = (rect.Right - rect.Left - widthExcess) / mineSize;
            height = (rect.Bottom - rect.Top - heightExcess) / mineSize;
        }

        private void BringToFront()
            => SetForegroundWindow(handle);

        private Process GetProcess()
        {
            foreach (var process in Process.GetProcesses())
                if (process.MainWindowTitle == windowTitle)
                    return process;

            return null;
        }

        //[DllImport("user32.dll", CharSet = CharSet.Unicode)]
        //public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, ref WindowRectangle rectangle);

        private struct WindowRectangle
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }
    }
}
