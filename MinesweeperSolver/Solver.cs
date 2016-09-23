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
    class Solver
    {
        public void Solve()
        {
            var window = new Window();
            if (!window.WindowFound)
                return;

            Thread.Sleep(100);
            window.OpenCell(0, 0);
            Thread.Sleep(10000);
            Mouse.DoubleButtonClick();
        }
    }
}
