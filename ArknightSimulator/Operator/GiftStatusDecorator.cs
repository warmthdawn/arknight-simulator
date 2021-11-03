using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operator
{
    public class GiftStatusDecorator: StatusDecorator
    {
        public GiftStatusDecorator(IStatus status) : base(status)
        {

        }
    }
}
