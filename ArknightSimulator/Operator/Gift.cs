using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operator
{
    public class Gift
    {
        public int EliteLevel { get; set; }
        public int Level { get; set; }

        public IStatus Start(IStatus status)
        {
            return new Decorator(status, this);
        }

        private class Decorator : GiftStatusDecorator
        {
            private Gift gift;
            public Decorator(IStatus status, Gift gift) : base(status)
            {
                this.gift = gift;
            }

            public override int Attack => base.Attack + gift.Level;

        }
    }
}
