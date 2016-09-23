using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver
{
    public class Window
    {
        private const int mineSize = 16;
        private const int xFieldAdjust = 15;
        private const int yFieldAdjust = 101;
        private const int widthExcess = 26;
        private const int heightExcess = 112;
        private const string windowTitle = "Minesweeper";

        private bool windowFound;
        private int width, height;
        private Rectangle bounds;
        private Rectangle fieldBounds;
        private Bitmap screenshot;
        private IntPtr handle;
        private int mineCount = 99; // todo

        private Cell[,] cells;
        private CellContents[,] cellContents;
        private double[,] cellBombChance;

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
            screenshot = TakeScreenshot();
            Console.WriteLine($"X: {bounds.X}\nY: {bounds.Y}\nField width: {width}\nField height: {height}");
        }

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

            width = (rect.Right - rect.Left - widthExcess) / mineSize;
            height = (rect.Bottom - rect.Top - heightExcess) / mineSize;
            bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            fieldBounds = new Rectangle(bounds.X + xFieldAdjust, bounds.Y + yFieldAdjust, mineSize * width, mineSize * height);

            cells = new Cell[width, height];
            cellContents = new CellContents[width, height];
            cellBombChance = new double[width, height];

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    cells[i, j] = Cell.Closed;
                    cellContents[i, j] = CellContents.Unknown;
                    cellBombChance[i, j] = -1;
                }
        }

        private Bitmap TakeScreenshot()
        {
            var bmp = new Bitmap(fieldBounds.Width, fieldBounds.Height);
            using (var gfx = Graphics.FromImage(bmp))
                gfx.CopyFromScreen(new Point(fieldBounds.Left, fieldBounds.Top), Point.Empty, fieldBounds.Size);

            bmp.Save(@"C:\Users\foxneSs\Desktop\asd.png", ImageFormat.Png);
            return bmp;
        }

        private void SetMouseOverCell(int x, int y)
        {
            FieldToScreen(ref x, ref y);
            Mouse.SetPosition(x, y);
        }

        private void FieldToScreen(ref int x, ref int y)
        {
            x = fieldBounds.X + mineSize * x + mineSize / 2;
            y = fieldBounds.Y + mineSize * y + mineSize / 2;
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

        public enum CellContents
        {
            Empty, One, Two, Three, Four, Five, Six, Seven, Eight, Bomb, Unknown
        }

        public enum Cell
        {
            Opened, Closed, Flagged
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
