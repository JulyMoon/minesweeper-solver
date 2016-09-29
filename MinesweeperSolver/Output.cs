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

        public static string WithSuffix(this int n)
        {
            var s = n.ToString();
            if (s.EndsWith("11")) return s + "th";
            if (s.EndsWith("12")) return s + "th";
            if (s.EndsWith("13")) return s + "th";
            if (s.EndsWith("1")) return s + "st";
            if (s.EndsWith("2")) return s + "nd";
            if (s.EndsWith("3")) return s + "rd";
            return s + "th";
        }
    }
}
