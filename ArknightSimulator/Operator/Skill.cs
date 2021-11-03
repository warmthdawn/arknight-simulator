using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operator
{
    public class Skill
    {
        public int Initial { get; set; }
        public int Cost { get; set; }
        public int Time { get; set; }
        public int Level { get; set; }

        public IStatus Start(IStatus status)
        {
            return default;
        }

        public IStatus End(IStatus status)
        {
            return default;
        }
    }
}
