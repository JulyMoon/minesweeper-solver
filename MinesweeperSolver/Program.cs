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

            var games = 0;
            var wins = 0;

            while (true)
            {
                //Console.Clear();

                window.NewGame();
                solver.Solve();
                games++;
                if (window.Win) wins++;
                Console.Title = $"{wins}/{games} ({100d * wins / games}%)";
                Console.WriteLine(window.Win ? "win" : "lose");
                
                //Console.ReadLine();
            }
        }
    }
}
