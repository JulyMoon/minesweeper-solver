﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// todo: 1. don't solve islands you already solved in the past
//       2. when given a choice between cells with equal chances to blow up choose the one whose opening might reveal more info
//          (the once closer to the center maybe?)

namespace MinesweeperSolver
{
    public class Solver
    {
        private Window window;
        private static readonly Random random = new Random();
        private static readonly Point[] pointNeighbors =
            { Point.Up, Point.Down, Point.Left, Point.Right, Point.TopLeft, Point.TopRight, Point.BottomLeft, Point.BottomRight };

        public int RisksTaken { get; private set; }
        public int MinesLeft { get; private set; }

        public Solver(Window window)
        {
            this.window = window;
        }

        public void Solve(bool newGame)
        {
            MinesLeft = window.MineCount;
            RisksTaken = 0;

            if (newGame)
                window.OpenCell(window.FieldWidth / 2, window.FieldHeight / 2);
            
            while (true)
            {
                window.Update();
                if (window.GameOver)
                    break;

                bool newFlags = FlagAllObviousCells();
                bool newOpenedCells = OpenAllObviousCells();

                if (!newOpenedCells)
                    SolveIslands();
            }
        }

        private void SolveIslands()
        {
            List<Point> nonBorderCells;
            var borderCells = GetBorderCells();
            if (borderCells.Count == 0)
            {
                nonBorderCells = GetNonBorderCells();
                double chanceToBlowUp = (double)MinesLeft / nonBorderCells.Count;
                if (chanceToBlowUp > 0)
                {
                    Console.WriteLine($"Opening a non-border cell with a {chanceToBlowUp:P2} chance to blow up\n");
                    RisksTaken++;
                    OpenOneCellRandomly(nonBorderCells);
                }
                else
                    OpenCells(nonBorderCells);

                return;
            }

            var islands = GetIslands(borderCells);
            Console.WriteLine($"Found {islands.Count} island{(islands.Count > 1 ? "s" : "")}");
            var allIslandConfigs = new List<List<Dictionary<Point, bool>>>();

            int i = 0;
            foreach (var island in islands)
            {
                Console.WriteLine($"Solving the {(++i).WithSuffix()} island out of {islands.Count} with a size of {island.Count}");
                allIslandConfigs.Add(GetAllPossibleIslandConfigs(island));
                Console.WriteLine($"Calculated all the possible configs of the {i.WithSuffix()} island");
            }

            var cellMineChances = GetMineChances(allIslandConfigs);

            Console.Write("Flagging all obvious cells...");
            bool newFlags = FlagAllObviousCells(cellMineChances);
            Console.WriteLine($" Done with{(newFlags ? "" : " no")} impact");

            Console.Write("Opening all obvious cells...");
            bool newOpenedCells = OpenAllObviousCells(cellMineChances);
            Console.WriteLine($" Done with{(newOpenedCells ? " impact\n" : " no impact")}");

            if (newOpenedCells)
                return;

            double nonBorderCellMineChance = -1;
            nonBorderCells = GetNonBorderCells();
            if (nonBorderCells.Count > 0)
            {
                int lowestBorderCellMineCount = GetTheLowestPossibleMineCountUsed(allIslandConfigs);
                int minesLeftForNonBorderCells = MinesLeft - lowestBorderCellMineCount;
                nonBorderCellMineChance = (double)minesLeftForNonBorderCells / (window.FieldWidth * window.FieldHeight);

                if (nonBorderCellMineChance == 0)
                {
                    OpenCells(nonBorderCells);
                    return;
                }
            }

            double borderMinChance;
            var minimalMineChanceBorderCells = GetMinimalMineChanceCells(cellMineChances, out borderMinChance);

            if (nonBorderCells.Count > 0 && nonBorderCellMineChance <= borderMinChance)
            {
                Console.WriteLine($"Opening a non-border cell with a {nonBorderCellMineChance:P2} chance to blow up\n");
                OpenOneCellRandomly(nonBorderCells);
            }
            else
            {
                Console.WriteLine($"Opening a border cell with a {borderMinChance:P2} chance to blow up\n");
                OpenOneCellRandomly(minimalMineChanceBorderCells);
            }

            RisksTaken++;
        }

        private int GetTheLowestPossibleMineCountUsed(List<List<Dictionary<Point, bool>>> multipleIslandConfigs)
        {
            int count = 0;
            foreach (var islandConfigs in multipleIslandConfigs)
            {
                int min = Int32.MaxValue;
                foreach (var config in islandConfigs)
                {
                    int configMineCount = config.Count(pair => pair.Value);
                    if (configMineCount < min)
                        min = configMineCount;
                }
                count += min;
            }
            return count;
        }

        private Dictionary<Point, double> GetMineChances(List<Dictionary<Point, bool>> islandConfigs)
        {
            var cellMineConfigCount = islandConfigs.First().ToDictionary(pair => pair.Key, pair => 0);

            foreach (var config in islandConfigs)
                foreach (var pair in config)
                    if (pair.Value)
                        cellMineConfigCount[pair.Key]++;

            return cellMineConfigCount.ToDictionary(pair => pair.Key, pair => (double)pair.Value / islandConfigs.Count);
        }

        private Dictionary<Point, double> GetMineChances(List<List<Dictionary<Point, bool>>> multipleIslandConfigs)
            => multipleIslandConfigs.SelectMany(oneIslandConfigs => GetMineChances(oneIslandConfigs)).ToDictionary(pair => pair.Key, pair => pair.Value);

        private static List<Point> GetMinimalMineChanceCells(Dictionary<Point, double> solution, out double minimalChance)
        {
            var minimalMineChanceCells = new List<Point>();
            minimalChance = Double.MaxValue;
            foreach (var key in solution.Keys)
            {
                if (solution[key] == minimalChance)
                    minimalMineChanceCells.Add(key);
                else if (solution[key] < minimalChance)
                {
                    minimalChance = solution[key];
                    minimalMineChanceCells.Clear();
                    minimalMineChanceCells.Add(key);
                }
            }
            return minimalMineChanceCells;
        }

        private void OpenOneCellRandomly(List<Point> cells)
            => window.OpenCell(cells[random.Next(cells.Count)]);

        private void OpenCells(List<Point> cells)
            => cells.ForEach(point => window.OpenCell(point));

        private List<Dictionary<Point, bool>> GetAllPossibleIslandConfigs(List<Point> island)
        {
            var configs = new List<Dictionary<Point, bool>>();
            GetAllPossibleIslandConfigs(ref configs, new Dictionary<Point, bool?>(), island, 0, MinesLeft);
            return configs;
        }

        private void GetAllPossibleIslandConfigs(ref List<Dictionary<Point, bool>> configs, Dictionary<Point, bool?> currentConfig, List<Point> island, int currentPoint, int availableMineCount)
        {
            var islandPoint = island[currentPoint];

            var notOpenedNeighbors = GetValidNeighbors(islandPoint).Where(neighbor => window.GetCell(neighbor) != Window.Cell.Opened).ToList();
            var fixedConfigNeighbors = notOpenedNeighbors.Intersect(currentConfig.Where(pair => pair.Value != null).Select(pair => pair.Key).ToList()).ToList();
            var neighborsToSolve = notOpenedNeighbors.Where(neighbor => window.GetCell(neighbor) == Window.Cell.Closed && !fixedConfigNeighbors.Contains(neighbor)).ToList();

            if (neighborsToSolve.Count == 0)
            {
                if (island.Count == currentPoint + 1)
                    configs.Add(currentConfig.ToDictionary(pair => pair.Key, pair => (bool)pair.Value));
                else
                    GetAllPossibleIslandConfigs(ref configs, currentConfig, island, currentPoint + 1, availableMineCount);
                return;
            }

            int adjustedMineCount = ToInt(window.GetCellContents(islandPoint)) - notOpenedNeighbors.Count(neighbor => window.GetCell(neighbor) == Window.Cell.Flagged || (fixedConfigNeighbors.Contains(neighbor) && (bool)currentConfig[neighbor]));

            if (adjustedMineCount > availableMineCount)
                return;

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
                        configs.Add(currentConfig.ToDictionary(pair => pair.Key, pair => (bool)pair.Value));
                    else
                        GetAllPossibleIslandConfigs(ref configs, currentConfig, island, currentPoint + 1, availableMineCount - adjustedMineCount);
                }
            }

            for (int i = 0; i < neighborsToSolve.Count; i++)
                currentConfig[neighborsToSolve[i]] = null;
        }

        private bool IsValidIslandConfig(List<Point> island, Dictionary<Point, bool?> islandConfig)
            // incomplete configs CAN be valid because here valid only means that no number cell has more mines around it than its number
            // note: island points are number cell points adjacent to islandConfig closed cell points
        {
            foreach (var islandPoint in island)
            {
                var notOpenedNeighbors = GetValidNeighbors(islandPoint).Where(neighbor => window.GetCell(neighbor) != Window.Cell.Opened);
                var fixedConfigNeighbors = notOpenedNeighbors.Intersect(islandConfig.Where(pair => pair.Value != null).Select(pair => pair.Key).ToList()).ToList();
                var neighborsToSolve = notOpenedNeighbors.Where(neighbor => window.GetCell(neighbor) == Window.Cell.Closed && !fixedConfigNeighbors.Contains(neighbor)).ToList();

                int minesAround = notOpenedNeighbors.Count(neighbor => window.GetCell(neighbor) == Window.Cell.Flagged || (fixedConfigNeighbors.Contains(neighbor) && (bool)islandConfig[neighbor]));

                int cellMines = ToInt(window.GetCellContents(islandPoint));
                if (minesAround > cellMines)
                    return false;

                if (cellMines - minesAround > neighborsToSolve.Count)
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
                        bool pointBelongsToIsland = false;
                        var pointNeighbors = GetValidNeighbors(point);
                        
                        if (pointNeighbors.Contains(islandPoint))
                            pointBelongsToIsland = true;

                        if (!pointBelongsToIsland)
                            foreach (var neighbor in pointNeighbors.Where(neighbor => window.GetCell(neighbor) == Window.Cell.Closed))
                                if (GetValidNeighbors(neighbor).Contains(islandPoint))
                                {
                                    pointBelongsToIsland = true;
                                    break;
                                }

                        if (pointBelongsToIsland)
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

        private List<Point> GetBorderCells() // a border cell is a cell next to a number cell
        {
            var borderCells = new List<Point>();
            for (int x = 0; x < window.FieldWidth; x++)
                for (int y = 0; y < window.FieldHeight; y++)
                    if (window.GetCell(x, y) == Window.Cell.Opened && IsNumber(window.GetCellContents(x, y))
                        && GetValidNeighbors(new Point(x, y)).Any(neighbor => window.GetCell(neighbor.X, neighbor.Y) == Window.Cell.Closed))
                    // ^ this is optimizable for sure (dont get all neighbors, get one then check then get another one)
                    {
                        borderCells.Add(new Point(x, y));
                    }
            return borderCells;
        }

        private List<Point> GetNonBorderCells()
        {
            var nonBorderCells = new List<Point>();
            for (int x = 0; x < window.FieldWidth; x++)
                for (int y = 0; y < window.FieldHeight; y++)
                    if (window.GetCell(x, y) != Window.Cell.Opened
                        && GetValidNeighbors(new Point(x, y)).All(neighbor => window.GetCell(neighbor.X, neighbor.Y) != Window.Cell.Opened))
                    // ^ this is optimizable for sure (dont get all neighbors, get one then check then get another one)
                    {
                        nonBorderCells.Add(new Point(x, y));
                    }
            return nonBorderCells;
        }

        private bool FlagAllObviousCells()
        {
            bool impact = false;
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
                                    MinesLeft--;
                                    impact = true;
                                }
                        }
                    }
            return impact;
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

        private bool FlagAllObviousCells(Dictionary<Point, double> solution)
        {
            bool impact = false;
            foreach (var pair in solution)
                if (pair.Value == 1)
                {
                    window.FlagCell(pair.Key);
                    MinesLeft--;
                    impact = true;
                }
            return impact;
        }

        private bool OpenAllObviousCells(Dictionary<Point, double> solution)
        {
            bool impact = false;
            foreach (var pair in solution)
                if (pair.Value == 0)
                {
                    window.OpenCell(pair.Key);
                    impact = true;
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
    }
}
