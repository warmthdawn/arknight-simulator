using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operator
{
    public class LevelStatusDecorator : StatusDecorator
    {
        public LevelStatusDecorator(IStatus status) : base(status)
        {

        }
        public int Level { get; set; }


    }
}
