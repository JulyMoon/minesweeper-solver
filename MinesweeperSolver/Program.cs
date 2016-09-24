using System;
using System.Threading;

namespace MinesweeperSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Minesweeper Solver";

            var window = Window.GetInstance();
            if (window == null)
                return;

            var solver = new Solver(window);

            while (true)
            {
                Console.Clear();

                window.NewGame();
                solver.Solve();
                
                Console.ReadLine();
            }
        }
    }
}
