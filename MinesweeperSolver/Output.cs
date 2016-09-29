using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver
{
    public static class Output
    {
        public static string ToString(List<Point> island)
        {
            string s = "[";
            foreach (var point in island.Take(island.Count - 1))
                s += $"{point}, ";
            if (island.Count > 0)
                s += $"{island.Last()}]";
            return s;
        }

        public static string ToString(Dictionary<Point, double> solution)
        {
            string s = "{\n";
            foreach (var pair in solution)
                s += $"\t{pair.Key}: {pair.Value:P1}\n";
            return s + "}";
        }
    }
}
