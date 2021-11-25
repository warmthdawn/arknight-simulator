using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operations
{
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x = 0, double y = 0)
        {
            X = x;
            Y = y;
        }
        public static double operator -(Point p1, Point p2)
        {
            double x = p1.X - p2.X;
            double y = p1.Y - p2.Y;
            return Math.Sqrt(x * x + y * y);
        }
    }
}
