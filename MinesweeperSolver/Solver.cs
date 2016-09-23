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

        public void Solve()
        {
            var window = Window.GetInstance();
            if (window == null)
                return;
        }
    }
}
