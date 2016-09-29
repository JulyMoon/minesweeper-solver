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
        private Window window;
        private static readonly Random random = new Random();
        private static readonly Point[] pointNeighbors =
            { Point.Up, Point.Down, Point.Left, Point.Right, Point.TopLeft, Point.TopRight, Point.BottomLeft, Point.BottomRight };

        public int RisksTaken { get; private set; }
        public int MinesFlagged { get; private set; }

        public Solver(Window window)
        {
            this.window = window;
        }

        public void Solve(bool newGame)
        {
            MinesFlagged = 0;
            RisksTaken = 0;

            if (newGame)
                window.OpenCell(random.Next(window.FieldWidth), random.Next(window.FieldHeight));
            
            while (true)
            {
                window.Update();

                if (window.GameOver)
                    break;

                FlagAllObviousCells();

                bool impact = OpenAllObviousCells();

                if (!impact)
                {
                    TankAlgorithm();
                    RisksTaken++;
                }

                Thread.Sleep(10); // may be not needed
            }
        }

        private void TankAlgorithm()
        {
            var islands = GetIslands(GetPointsToSolve());
            Console.WriteLine($"Found {islands.Count} islands");
            var solution = new Dictionary<Point, double>();
            int i = 0;
            foreach (var island in islands)
            {
                i++;
                Console.WriteLine($"Solving {i}th island: {PointListToStr(island)}");
                var islandSolution = SolveIsland(island);

                if (islandSolution.Count == 0)
                    throw new Exception();

                Console.WriteLine($"{i}th island's solution:\n{SolutionToStr(islandSolution)}");
                solution = solution.Concat(islandSolution).ToDictionary(pair => pair.Key, pair => pair.Value);
            }

            if (islands.Count > 1)
                Console.WriteLine($"Overall solution:\n{SolutionToStr(solution)}");

            var minimalMineChanceCells = new List<Point>();
            var min = Double.MaxValue;
            foreach (var key in solution.Keys)
            {
                if (solution[key] == min)
                {
                    minimalMineChanceCells.Add(key);
                }
                else if (solution[key] < min)
                {
                    min = solution[key];
                    minimalMineChanceCells.Clear();
                    minimalMineChanceCells.Add(key);
                }
            }

            var randomCell = minimalMineChanceCells[random.Next(minimalMineChanceCells.Count)];
            Console.WriteLine($"Out of {minimalMineChanceCells.Count} minimal chance cells I chose {PointToStr(randomCell)}");
            window.OpenCell(randomCell);
        }

        private Dictionary<Point, double> SolveIsland(List<Point> island)
        {
            int totalConfigCount = 0;
            var solution = new Dictionary<Point, int>();
            SolveIsland(ref solution, ref totalConfigCount, new Dictionary<Point, bool?>(), island, 0);
            var result = solution.ToDictionary(pair => pair.Key, pair => (double)pair.Value / totalConfigCount);
            return result;
        }

        private void SolveIsland(ref Dictionary<Point, int> solution, ref int totalConfigCount, Dictionary<Point, bool?> currentConfig, List<Point> island, int currentPoint)
            // the solution consists of the following:
            //   - a list of points and the number of mines that appeared in that point across all configs
            //   - a total number of configs
        {
            var islandPoint = island[currentPoint];
            var neighbors = GetValidNeighbors(islandPoint);

            var notOpenedNeighbors = neighbors.Where(neighbor => window.GetCell(neighbor) != Window.Cell.Opened).ToList();
            var fixedConfigNeighbors = notOpenedNeighbors.Intersect(currentConfig.Where(pair => pair.Value != null).Select(pair => pair.Key).ToList()).ToList();
            var neighborsToSolve = notOpenedNeighbors.Where(neighbor => window.GetCell(neighbor) == Window.Cell.Closed && !fixedConfigNeighbors.Contains(neighbor)).ToList();

            if (neighborsToSolve.Count == 0)
                return;

            int adjustedMineCount = ToInt(window.GetCellContents(islandPoint)) - notOpenedNeighbors.Count(neighbor => window.GetCell(neighbor) == Window.Cell.Flagged || (fixedConfigNeighbors.Contains(neighbor) && (bool)currentConfig[neighbor]));

            if (adjustedMineCount > neighborsToSolve.Count)
                ; // breakpoint

            foreach (var permutation in GetPermutations(adjustedMineCount, neighborsToSolve.Count))
            {
                for (int i = 0; i < neighborsToSolve.Count; i++)
                {
                    if (currentConfig.ContainsKey(neighborsToSolve[i]))
                        currentConfig[neighborsToSolve[i]] = permutation[i];
                    else
                        currentConfig.Add(neighborsToSolve[i], permutation[i]);
                }
                
                if (IsValidIslandConfig(island, currentConfig))
                {
                    if (island.Count == currentPoint + 1)
                    {
                        if (solution.Count == 0)
                            foreach (var point in currentConfig)
                                solution.Add(point.Key, 0);

                        foreach (var point in currentConfig)
                            if ((bool)point.Value)
                                solution[point.Key]++;

                        totalConfigCount++;
                    }
                    else
                        SolveIsland(ref solution, ref totalConfigCount, currentConfig, island, currentPoint + 1);
                }
                else
                {
                    for (int i = 0; i < neighborsToSolve.Count; i++)
                        currentConfig[neighborsToSolve[i]] = null;
                }
            }
        }

        private bool IsValidIslandConfig(List<Point> island, Dictionary<Point, bool?> islandConfig)
            // incomplete configs CAN be valid because here valid only means that no number cell has more mines around it than its number
            // note: island points are number cell points adjacent to islandConfig closed cell points
        {
            foreach (var islandPoint in island)
            {
                var neighbors = GetValidNeighbors(islandPoint).Where(neighbor => window.GetCell(neighbor) != Window.Cell.Opened);
                int minesAround = neighbors.Count(neighbor => window.GetCell(neighbor) == Window.Cell.Flagged || (islandConfig.ContainsKey(neighbor) && (islandConfig[neighbor] ?? false)));

                if (minesAround > ToInt(window.GetCellContents(islandPoint)))
                    return false;
            }
            return true;
        }

        private static List<bool[]> GetPermutations(int trueCount, int count)
        {
            if (trueCount == 0)
                return new List<bool[]> { Enumerable.Repeat(false, count).ToArray() };
            else if (trueCount == count)
                return new List<bool[]> { Enumerable.Repeat(true, count).ToArray() };
            else
            {
                var permutations = new List<bool[]>();
                bool append = true;
                while (true)
                {
                    var a = GetPermutations(append ? trueCount - 1 : trueCount, count - 1);
                    for (int i = 0; i < a.Count; i++)
                        a[i] = new[] { append }.Concat(a[i]).ToArray();
                    permutations = permutations.Concat(a).ToList();

                    if (append)
                        append = false;
                    else
                        break;
                }

                return permutations;
            }
        }

        private List<List<Point>> GetIslands(List<Point> pointsToSolve)
        {
            var islands = new List<List<Point>>();
            foreach (var point in pointsToSolve)
            {
                var islandsThisPointBelongsTo = new HashSet<List<Point>>();
                foreach (var island in islands)
                    foreach (var islandPoint in island)
                    {
                        var pointNeighbors = GetValidNeighbors(point);
                        
                        if (pointNeighbors.Contains(islandPoint))
                        {
                            islandsThisPointBelongsTo.Add(island);
                            break;
                        }

                        foreach (var neighbor in pointNeighbors.Where(neighbor => window.GetCell(neighbor) == Window.Cell.Closed))
                            if (GetValidNeighbors(neighbor).Contains(islandPoint))
                            {
                                islandsThisPointBelongsTo.Add(island);
                                break;
                            }
                    }

                islands = islands.Except(islandsThisPointBelongsTo).ToList();
                islands.Add(islandsThisPointBelongsTo.SelectMany(a => a).Concat(new List<Point> { point }).ToList());
            }
            return islands;
        }

        private List<Point> GetPointsToSolve()
        {
            var pointsToSolve = new List<Point>();
            for (int x = 0; x < window.FieldWidth; x++)
                for (int y = 0; y < window.FieldHeight; y++)
                    if (window.GetCell(x, y) == Window.Cell.Opened && IsNumber(window.GetCellContents(x, y))
                        && GetValidNeighbors(new Point(x, y)).Any(neighbor => window.GetCell(neighbor.X, neighbor.Y) == Window.Cell.Closed))
                    // ^ this is optimizable for sure (dont get all neighbors, get one then check then get another one)
                    {
                        pointsToSolve.Add(new Point(x, y));
                    }
            return pointsToSolve;
        }

        private void FlagAllObviousCells()
        {
            for (int x = 0; x < window.FieldWidth; x++)
                for (int y = 0; y < window.FieldHeight; y++)
                    if (window.GetCell(x, y) == Window.Cell.Opened)
                    {
                        var cc = window.GetCellContents(x, y);
                        if (IsNumber(cc))
                        {
                            var notOpenedNeighbors = GetValidNeighbors(new Point(x, y)).Where(neighbor => window.GetCell(neighbor) != Window.Cell.Opened).ToList();
                            if (notOpenedNeighbors.Count == ToInt(cc))
                                foreach (var neighbor in notOpenedNeighbors.Where(neighbor => window.GetCell(neighbor) == Window.Cell.Closed))
                                {
                                    window.FlagCell(neighbor);
                                    MinesFlagged++;
                                }
                        }
                    }
        }

        private bool OpenAllObviousCells()
        {
            var impact = false;
            for (int x = 0; x < window.FieldWidth; x++)
                for (int y = 0; y < window.FieldHeight; y++)
                    if (window.GetCell(x, y) == Window.Cell.Opened)
                    {
                        var cc = window.GetCellContents(x, y);
                        if (IsNumber(cc))
                        {
                            var p = new Point(x, y);
                            var neighbors = GetValidNeighbors(p);
                            if (neighbors.Count(neighbor => window.GetCell(neighbor) == Window.Cell.Flagged) == ToInt(cc)
                                && neighbors.Any(neighbor => window.GetCell(neighbor) == Window.Cell.Closed))
                            {
                                window.MassOpenCell(p);
                                impact = true;
                            }
                        }
                    }

            return impact;
        }

        private static bool IsNumber(Window.CellContents cc)
            => Window.CellContents.One <= cc && cc <= Window.CellContents.Eight;

        private static int ToInt(Window.CellContents cc)
            => cc - Window.CellContents.Empty;

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

        private static string PointListToStr(List<Point> island)
        {
            string s = "[";
            foreach (var point in island.Take(island.Count - 1))
                s += $"{PointToStr(point)}, ";
            if (island.Count > 0)
                s += $"{PointToStr(island.Last())}]";
            return s;
        }

        private static string PointToStr(Point p) => $"({p.X}, {p.Y})";

        private static string SolutionToStr(Dictionary<Point, double> solution)
        {
            string s = "{\n";
            foreach (var pair in solution)
                s += $"\t{PointToStr(pair.Key)}: {pair.Value * 100}%\n";
            return s + "}";
        }
    }
}
