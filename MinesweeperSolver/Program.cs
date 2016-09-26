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

                    /*foreach (var pcs2 in Process.GetProcesses())
                        if (pcs2.MainWindowTitle == "Minesweeper")
                        {
                            pcs2.Kill();
                            break;
                        }*/

                    return;
                }

            var window = Window.GetInstance();
            if (window == null)
                return;

            var solver = new Solver(window);

            var games = 0;
            var wins = 0;

            while (true)
            {
                window.NewGame();
                solver.Solve(true);
                games++;
                if (window.Win) wins++;
                Console.Title = $"{wins}/{games} ({100d * wins / games}%)";
                Console.WriteLine($"{(window.Win ? "win " : "lose")} ({solver.RisksTaken}, {solver.MinesFlagged})");
            }
        }
    }
}
