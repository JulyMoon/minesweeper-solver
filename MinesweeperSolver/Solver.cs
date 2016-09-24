using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinesweeperSolver
{
    public class Solver
    {
        /*private void PrintCell(int x, int y)
        {
            var cell = cells[x, y];
            var s = $"cells[{x}, {y}] = {cell} | ";
            if (cell == Cell.Opened)
                s += $"cellContents[{x}, {y}] = {cellContents[x, y]}";
            else
                s += $"cellBombChance[{x}, {y}] = {cellBombChance[x, y]}";
            Console.WriteLine(s);
        }*/

        private double[,] cellBombChance;
        private Window window;
        private static readonly Random random = new Random();
        private static readonly Point[] pointNeighbors =
            { Point.Up, Point.Down, Point.Left, Point.Right, Point.TopLeft, Point.TopRight, Point.BottomLeft, Point.BottomRight };

        public static Solver GetInstance()
        {
            var window = Window.GetInstance();
            if (window == null)
                return null;

            return new Solver(window);
        }

        private Solver(Window window)
        {
            this.window = window;

            cellBombChance = new double[window.FieldWidth, window.FieldHeight];
            for (int x = 0; x < window.FieldWidth; x++)
                for (int y = 0; y < window.FieldHeight; y++)
                {
                    cellBombChance[x, y] = -1;
                }
        }

        public void Solve()
        {
            Console.WriteLine("Starting to solve...");
            SimpleAlgorithm();
            Console.WriteLine(window.Win ? "Yay! I won :D" : "Oops... I blew up :O");
        }

        private void SimpleAlgorithm()
        {
            while (!window.GameOver)
            {
                var change = false;
                for (int x = 0; x < window.FieldWidth; x++)
                    for (int y = 0; y < window.FieldHeight; y++)
                        if (window.GetCell(x, y) == Window.Cell.Opened)
                        {
                            var cc = window.GetCellContents(x, y);
                            if (IsNumber(cc))
                            {
                                var notOpenedNeighbors = GetValidNeighbors(new Point(x, y)).Where(neighbor => window.GetCell(neighbor) != Window.Cell.Opened).ToList();
                                if (notOpenedNeighbors.Count == ToInt(cc))
                                    foreach (var neighbor in notOpenedNeighbors.Where(neighbor => window.GetCell(neighbor) == Window.Cell.Closed))
                                    {
                                        window.FlagCell(neighbor);
                                        change = true;
                                    }
                            }
                        }

                if (change)
                {
                    change = false;
                    for (int x = 0; x < window.FieldWidth; x++)
                        for (int y = 0; y < window.FieldHeight; y++)
                            if (window.GetCell(x, y) == Window.Cell.Opened)
                            {
                                var cc = window.GetCellContents(x, y);
                                var p = new Point(x, y);
                                var neighbors = GetValidNeighbors(p);
                                if (IsNumber(cc) && neighbors.Where(neighbor => window.GetCell(neighbor) == Window.Cell.Flagged).Count() == ToInt(cc)
                                    && neighbors.Any(neighbor => window.GetCell(neighbor) == Window.Cell.Closed))
                                {
                                    window.MassOpenCell(p);
                                    change = true;
                                }
                            }
                }

                if (!change)
                {
                    int xx, yy;
                    bool bad;
                    do
                    {
                        xx = random.Next(window.FieldWidth);
                        yy = random.Next(window.FieldHeight);

                        bad = window.GetCell(xx, yy) != Window.Cell.Closed;
                    } while (bad);
                    window.OpenCell(xx, yy);
                }

                window.Update();
            }
        }

        private static bool IsNumber(Window.CellContents cc)
            => Window.CellContents.One <= cc && cc <= Window.CellContents.Eight;

        private static int ToInt(Window.CellContents cc)
            => cc - Window.CellContents.Empty;

        /*private void UpdateChances()
        {
            for (int x = 0; x < window.FieldWidth; x++)
                for (int y = 0; y < window.FieldHeight; y++)
                    if (window.GetCell(x, y) == Window.Cell.Closed)
                    {

                    }
        }*/

        private bool IsValid(Point p)
            => p.X >= 0 && p.X < window.FieldWidth && p.Y >= 0 && p.Y < window.FieldHeight;

        private List<Point> GetValidNeighbors(Point p)
        {
            var neighbors = new List<Point>();
            foreach (var neighbor in pointNeighbors)
            {
                var n = p + neighbor;
                if (IsValid(n))
                    neighbors.Add(n);
            }
            return neighbors;
        }
    }
}
