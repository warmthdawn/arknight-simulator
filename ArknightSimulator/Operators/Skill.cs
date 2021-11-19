using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operators
{
    public class Skill
    {
        public int Initial { get; set; }
        public int Cost { get; set; }
        public int Time { get; set; }
        public int Level { get; set; }


        public Skill() { }
        public Skill(Skill skill)
        {

        }

        public IStatus Start(IStatus status)
        {
            return new Decorator(status, this);
        }

        public IStatus End(IStatus status)
        {
            return default;
        }


        private class Decorator : SkillStatusDecorator
        {
            private Skill skill;
            public Decorator(IStatus status, Skill skill) : base(status)
            {
                this.skill = skill;
            }

            public override int Attack => base.Attack + skill.Level;

        }
    }
}
