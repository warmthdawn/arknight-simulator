using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operators
{
    public class GiftStatusDecorator: StatusDecorator
    {
        public GiftStatusDecorator(IStatus status) : base(status)
        {

        }
    }
}
