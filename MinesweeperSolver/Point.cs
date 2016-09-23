using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver
{
    public class Point
    {
        public readonly int X, Y;
        public static readonly Point Origin = new Point(0, 0);
        public static readonly Point Up = new Point(0, -1);
        public static readonly Point Down = new Point(0, 1);
        public static readonly Point Left = new Point(-1, 0);
        public static readonly Point Right = new Point(1, 0);
        public static readonly Point TopLeft = new Point(-1, -1);
        public static readonly Point TopRight = new Point(1, -1);
        public static readonly Point BottomLeft = new Point(-1, 1);
        public static readonly Point BottomRight = new Point(1, 1);

        public Point(int x, int y) { X = x; Y = y; }

        public Point() : this(0, 0) { }

        public static Point operator +(Point a, Point b) => new Point(a.X + b.X, a.Y + b.Y);

        public static Point operator -(Point a) => new Point(-a.X, -a.Y);

        public static Point operator -(Point a, Point b) => a + -b;

        public override bool Equals(object o) => Equals(o as Point);

        public bool Equals(Point p) => (X == p?.X) && (Y == p?.Y);

        public override int GetHashCode() => X ^ Y;

        public static implicit operator string(Point a) => $"Point({a.X}, {a.Y})";

        public override string ToString() => this;
    }
}
