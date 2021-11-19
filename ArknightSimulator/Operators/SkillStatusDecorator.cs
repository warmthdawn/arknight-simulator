using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operators
{
    public class SkillStatusDecorator: StatusDecorator
    {
        public SkillStatusDecorator(IStatus status) : base(status)
        {

        }
    }
}
