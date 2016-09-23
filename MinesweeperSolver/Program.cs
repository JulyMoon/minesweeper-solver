using System;

namespace MinesweeperSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Minesweeper Solver";
            var solver = Solver.GetInstance();
            solver?.Solve();

            Console.WriteLine("I'm done");
            Console.ReadLine();
        }
    }
}
