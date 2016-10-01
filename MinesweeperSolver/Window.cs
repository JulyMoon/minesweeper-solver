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

        private const int x1 = 6, y1 = 2, // these are red numbers' part coordinates
                          x2 = 2, y2 = 7, // relative to their top-left corner
                          x3 = 10, y3 = 7,
                          x4 = 6, y4 = 11,
                          x5 = 2, y5 = 17,
                          x6 = 10, y6 = 17,
                          x7 = 6, y7 = 20;

        private const int redNumberX = 19;
        private const int redNumberY = 63;
        private const int redNumberWidth = 13;

        private Rectangle bounds;
        private Rectangle fieldBounds;
        private Bitmap screenshot;
        private IntPtr handle;

        private Cell[,] cells;
        private CellContents[,] cellContents;

        public int FieldWidth { get; private set; }
        public int FieldHeight { get; private set; }
        public int MineCount { get; private set; } = -1;
        public bool GameOver { get; private set; }
        public bool Win { get; private set; }

        public static Window GetInstance()
        {
            var process = GetProcess();

            if (process == null)
            {
                Console.WriteLine($"Window not found!");
                return null;
            }

            Console.WriteLine($"Window found!");
            return new Window(process.MainWindowHandle);
        }

        private Window(IntPtr handle)
        {
            this.handle = handle;
            Initialize();
            Console.WriteLine($"W: {FieldWidth}, H: {FieldHeight}");
            Thread.Sleep(200);
        }

        public void FlagCell(Point p) => FlagCell(p.X, p.Y);
        public void OpenCell(Point p) => OpenCell(p.X, p.Y);
        public void MassOpenCell(Point p) => ChordCell(p.X, p.Y);
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

        public void ChordCell(int x, int y)
        {
            SetMouseOverCell(x, y);
            Mouse.DoubleButtonClick();
        }

        public void Update()
        {
            screenshot = TakeScreenshot(false);
            ReadScreenshot();
        }

        public void NewGame()
        {
            BringToFront();
            GameOver = false;
            Mouse.SetPosition(bounds.X + bounds.Width / 2, bounds.Y + 80);
            Mouse.LeftClick();
            Thread.Sleep(10);
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

            FieldWidth = (rect.Right - rect.Left - widthExcess) / mineSize;
            FieldHeight = (rect.Bottom - rect.Top - heightExcess) / mineSize;
            bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            fieldBounds = new Rectangle(bounds.X + xFieldAdjust, bounds.Y + yFieldAdjust, mineSize * FieldWidth, mineSize * FieldHeight);

            cells = new Cell[FieldWidth, FieldHeight];
            cellContents = new CellContents[FieldWidth, FieldHeight];

            for (int i = 0; i < FieldWidth; i++)
                for (int j = 0; j < FieldHeight; j++)
                {
                    cells[i, j] = Cell.Closed;
                    cellContents[i, j] = CellContents.Mine; // Kappa
                }
        }

        private void ReadScreenshot()
        {
            if (MineCount == -1)
            {
                ReadMineCount();
                Console.WriteLine($"Mines: {MineCount}");
            }

            ReadGameover();

            for (int x = 0; x < FieldWidth; x++)
                for (int y = 0; y < FieldHeight; y++)
                    ReadCell(x, y);
        }

        private void ReadGameover()
        {
            if (screenshot.GetPixel(bounds.Width / 2 - 2, 72) == Color.FromArgb(0, 0, 0))
            {
                GameOver = true;
                Win = screenshot.GetPixel(bounds.Width / 2 - 4, 73) == Color.FromArgb(0, 0, 0);
            }
        }

        private void ReadMineCount()
        {
            var number = ReadRedNumber(redNumberX, redNumberY) +
                         ReadRedNumber(redNumberX + redNumberWidth, redNumberY) +
                         ReadRedNumber(redNumberX + redNumberWidth * 2, redNumberY);

            MineCount = Int32.Parse(number);
        }

        private string ReadRedNumber(int x, int y)
        {
            bool part1 = IsRed(x + x1, y + y1),
                 part2 = IsRed(x + x2, y + y2),
                 part3 = IsRed(x + x3, y + y3),
                 part4 = IsRed(x + x4, y + y4),
                 part5 = IsRed(x + x5, y + y5),
                 part6 = IsRed(x + x6, y + y6),
                 part7 = IsRed(x + x7, y + y7);

            // dayum this is some ugly shit LUL :^)

            if (part1 && part2 && part3 && !part4 && part5 && part6 && part7) return "0";
            if (!part1 && !part2 && part3 && !part4 && !part5 && part6 && !part7) return "1";
            if (part1 && !part2 && part3 && part4 && part5 && !part6 && part7) return "2";
            if (part1 && !part2 && part3 && part4 && !part5 && part6 && part7) return "3";
            if (!part1 && part2 && part3 && part4 && !part5 && part6 && !part7) return "4";
            if (part1 && part2 && !part3 && part4 && !part5 && part6 && part7) return "5";
            if (part1 && part2 && !part3 && part4 && part5 && part6 && part7) return "6";
            if (part1 && !part2 && part3 && !part4 && !part5 && part6 && !part7) return "7";
            if (part1 && part2 && part3 && part4 && part5 && part6 && part7) return "8";
            if (part1 && part2 && part3 && part4 && !part5 && part6 && part7) return "9";

            throw new Exception("Couldn't recognize the number");
        }

        private bool IsRed(int x, int y)
            => screenshot.GetPixel(x, y) == Color.FromArgb(255, 0, 0);

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

                    if (screenshot.GetPixel(xx + mineSize / 2, yy + mineSize / 2 - 4) == Color.FromArgb(0, 0, 0))
                        cellContents[x, y] = CellContents.Seven;
                    else
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
                    cellContents[x, y] = CellContents.Mine;
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
                    color = screenshot.GetPixel(xx + mineSize / 2, yy + mineSize / 2 - 1);
                    if (color == Color.FromArgb(128, 128, 128))
                        cellContents[x, y] = CellContents.Eight;
                    else
                    {
                        Environment.Exit(0);
                        //throw new Exception($"Unexpected color: {color}");
                    }
                }
            }
        }

        public void SaveScreenshot()
            => screenshot.Save(@"C:\Users\foxneSs\Desktop\asd.png", ImageFormat.Png);

        private Bitmap TakeScreenshot(bool save)
        {
            var bmp = new Bitmap(bounds.Width, bounds.Height);
            using (var gfx = Graphics.FromImage(bmp))
                gfx.CopyFromScreen(new System.Drawing.Point(bounds.Left, bounds.Top), System.Drawing.Point.Empty, bounds.Size);

            if (save)
            {
                bool exception;
                do
                {
                    exception = false;

                    try
                    {
                        bmp.Save(@"C:\Users\foxneSs\Desktop\asd.png", ImageFormat.Png);
                    }
                    catch
                    {
                        exception = true;
                        Console.WriteLine("EXCEPTION");
                    }
                } while (exception);
            }
            
            return bmp;
        }

        private void SetMouseOverCell(int x, int y)
        {
            FieldToScreen(ref x, ref y);
            Mouse.SetPosition(x, y);
        }

        private void FieldToScreen(ref int x, ref int y)
        {
            x = fieldBounds.X + (mineSize * x) + mineSize / 2;
            y = fieldBounds.Y + (mineSize * y) + mineSize / 2;
        }

        private void FieldToScreenshot(ref int x, ref int y)
        {
            x = xFieldAdjust + mineSize * x;
            y = yFieldAdjust + mineSize * y;
        }

        private void BringToFront()
            => SetForegroundWindow(handle);

        private static Process GetProcess()
        {
            foreach (var pcs in Process.GetProcesses())
                if (pcs.MainWindowTitle == windowTitle)
                    pcs.Kill();

            var process = Process.Start(@"C:\Users\foxneSs\Desktop\Sweeper.exe");
            Thread.Sleep(200);
            return process;
        }

        public enum CellContents
        {
            Empty, One, Two, Three, Four, Five, Six, Seven, Eight, Mine
        }

        public enum Cell
        {
            Opened, Closed, Flagged
        }

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
