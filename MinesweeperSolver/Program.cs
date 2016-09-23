using System;

namespace MinesweeperSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Minesweeper Solver";
            var solver = new Solver();
            solver.Solve();
            Console.ReadLine();
        }
    }
}
