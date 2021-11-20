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
    }
}
