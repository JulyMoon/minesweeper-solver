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
            window.OpenCell(15, 7);
        }
    }
}
