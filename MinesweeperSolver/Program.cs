using System;
using System.Diagnostics;
using System.Threading;

namespace MinesweeperSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Minesweeper Solver";

            foreach (var pcs in Process.GetProcesses())
                if (pcs.MainWindowTitle == Console.Title && Process.GetCurrentProcess().Id != pcs.Id)
                {
                    pcs.Kill();
                    return;
                }

            var window = Window.GetInstance();
            var solver = new Solver(window);
            var games = 0;
            var wins = 0;

            while (true)
            {
                Console.Clear();
                window.NewGame();
                solver.Solve(true);
                games++;
                if (window.Win) wins++;
                Console.Title = $"{wins}/{games} ({(double)wins / games:P4})";
                Console.WriteLine($"{(window.Win ? "win " : "lose")} ({solver.RisksTaken}, {solver.MinesFlagged})");
            }
        }
    }
}
