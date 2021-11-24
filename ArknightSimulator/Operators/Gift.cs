using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operators
{
    public class Gift
    {
        public int EliteLevel { get; set; }
        public int Level { get; set; }

        public Gift() { }
        public Gift(Gift gift)
        {

        }


        public IStatus Start(IStatus status)
        {
            return new Decorator(status, this);
        }

        private class Decorator : StatusDecorator
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
