using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
        
        private int width, height;
        private Rectangle bounds;
        private Rectangle fieldBounds;
        private Bitmap screenshot;
        private IntPtr handle;
        private int bombCount = 99; // todo

        private Cell[,] cells;
        private CellContents[,] cellContents;

        public int FieldWidth => width;
        public int FieldHeight => height;
        public int BombCount => bombCount;

        public bool GameOver
        {
            get
            {
                for (int x = 0; x < FieldWidth; x++)
                    for (int y = 0; y < FieldHeight; y++)
                        if (cells[x, y] == Cell.Opened && cellContents[x, y] == CellContents.Bomb)
                            return true;
                return false;
            }
        }

        public static Window GetInstance()
        {
            var process = GetProcess();

            if (process == null)
            {
                Console.WriteLine($"Window not found!");
                return null;
            }

            Console.WriteLine($"Window found! PID: {process.Id}");
            return new Window(process.MainWindowHandle);
        }

        private Window(IntPtr handle)
        {
            this.handle = handle;
            Initialize();
            Console.WriteLine($"X: {bounds.X}\nY: {bounds.Y}\nField width: {width}\nField height: {height}");
            Thread.Sleep(200);
            NewGame();
            //Thread.Sleep(200);
            Update();
        }

        public void FlagCell(Point p) => FlagCell(p.X, p.Y);
        public void OpenCell(Point p) => OpenCell(p.X, p.Y);
        public void MassOpenCell(Point p) => MassOpenCell(p.X, p.Y);
        public Cell GetCell(Point p) => GetCell(p.X, p.Y);
        public CellContents GetCellContents(Point p) => GetCellContents(p.X, p.Y);

        public void FlagCell(int x, int y)
        {
            SetMouseOverCell(x, y);
            Mouse.RightClick();
            cells[x, y] = Cell.Flagged;
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

        public void Update()
        {
            screenshot = TakeScreenshot();
            ReadField();
        }

        public Cell GetCell(int x, int y) => cells[x, y];

        public CellContents GetCellContents(int x, int y) => cellContents[x, y];

        private void Initialize()
        {
            ShowWindow(handle, SW_SHOWNORMAL);
            ShowWindow(handle, SW_RESTORE);
            BringToFront();

            var rect = new WindowRectangle();
            GetWindowRect(handle, ref rect);

            width = (rect.Right - rect.Left - widthExcess) / mineSize;
            height = (rect.Bottom - rect.Top - heightExcess) / mineSize;
            bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            fieldBounds = new Rectangle(bounds.X + xFieldAdjust, bounds.Y + yFieldAdjust, mineSize * width, mineSize * height);

            cells = new Cell[width, height];
            cellContents = new CellContents[width, height];

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    cells[i, j] = Cell.Closed;
                    cellContents[i, j] = CellContents.Bomb; // Kappa
                }
        }

        private void NewGame()
        {
            Mouse.SetPosition(bounds.X + bounds.Width / 2, bounds.Y + 80);
            Mouse.LeftClick();
        }

        private void ReadField()
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    ReadCell(x, y);
        }

        private void ReadCell(int x, int y)
        {
            int xx = x, yy = y;
            FieldToScreenshot(ref xx, ref yy);
            var color = screenshot.GetPixel(xx + mineSize / 2, yy + mineSize / 2);

            if (color == Color.FromArgb(192, 192, 192))
            {
                if (screenshot.GetPixel(xx, yy) == Color.FromArgb(255, 255, 255))
                    cells[x, y] = Cell.Closed;
                else
                {
                    cells[x, y] = Cell.Opened;
                    cellContents[x, y] = CellContents.Empty;
                }
            }
            else if (color == Color.FromArgb(0, 0, 0))
            {
                if (screenshot.GetPixel(xx, yy) == Color.FromArgb(255, 255, 255))
                    cells[x, y] = Cell.Flagged;
                else
                {
                    cells[x, y] = Cell.Opened;
                    cellContents[x, y] = CellContents.Bomb;
                }
            }
            else
            {
                cells[x, y] = Cell.Opened;

                if (color == Color.FromArgb(0, 0, 255))
                    cellContents[x, y] = CellContents.One;
                else if (color == Color.FromArgb(0, 128, 0))
                    cellContents[x, y] = CellContents.Two;
                else if (color == Color.FromArgb(255, 0, 0))
                    cellContents[x, y] = CellContents.Three;
                else if (color == Color.FromArgb(0, 0, 128))
                    cellContents[x, y] = CellContents.Four;
                else if (color == Color.FromArgb(128, 0, 0))
                    cellContents[x, y] = CellContents.Five;
                else if (color == Color.FromArgb(0, 128, 128))
                    cellContents[x, y] = CellContents.Six;
                else
                {
                    color = screenshot.GetPixel(xx + mineSize / 2, yy + mineSize / 2 + 1);
                    if (color == Color.FromArgb(0, 0, 0))
                        cellContents[x, y] = CellContents.Seven;
                    else if (color == Color.FromArgb(128, 128, 128))
                        cellContents[x, y] = CellContents.Eight;
                    else
                        throw new Exception($"Unexpected color: {color}");
                }
            }
        }

        private Bitmap TakeScreenshot()
        {
            var bmp = new Bitmap(fieldBounds.Width, fieldBounds.Height);
            using (var gfx = Graphics.FromImage(bmp))
                gfx.CopyFromScreen(new System.Drawing.Point(fieldBounds.Left, fieldBounds.Top), System.Drawing.Point.Empty, fieldBounds.Size);

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
            FieldToScreenshot(ref x, ref y);
            x += fieldBounds.X + mineSize / 2;
            y += fieldBounds.Y + mineSize / 2;
        }

        private void FieldToScreenshot(ref int x, ref int y)
        {
            x = mineSize * x;
            y = mineSize * y;
        }

        private void BringToFront()
            => SetForegroundWindow(handle);

        private static Process GetProcess()
        {
            /*foreach (var process in Process.GetProcesses())
                if (process.MainWindowTitle == windowTitle)
                    return process;

            return null;*/

            var process = Process.Start(@"C:\Users\foxneSs\Desktop\Sweeper.exe");
            Thread.Sleep(200);
            return process;
        }

        public enum CellContents
        {
            Empty, One, Two, Three, Four, Five, Six, Seven, Eight, Bomb
        }

        public enum Cell
        {
            Opened, Closed, Flagged
        }

        //[DllImport("user32.dll", CharSet = CharSet.Unicode)]
        //public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

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
