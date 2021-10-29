using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operator
{
    class Skill
    {
        int Initial { get; set; }
        int Cost { get; set; }
        int Time { get; set; }
        int Level { get; set; }

        Status Start(Status status)
        {
            return default;
        }

        Status End(Status status)
        {
            return default;
        }
    }
}
