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
            window.OpenCell(random.Next(window.FieldWidth), random.Next(window.FieldHeight));
        }

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
