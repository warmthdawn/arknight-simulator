using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operator
{
    public class StatusDecorator : IStatus
    {
        public StatusDecorator(IStatus status)
        {
            this.Inner = status;
        }
        public IStatus Inner { get; private set; }
        public int MaxLife => Inner.MaxLife;
        public int CurrentLife => Inner.CurrentLife;
        public int SkillPoint => Inner.SkillPoint;
        public int Attack => Inner.Attack;
        public int Defence => Inner.Defence;
        public int MagicDefence => Inner.MagicDefence;
        public int Time => Inner.Time;
        public int Cost => Inner.Cost;
        public int Block => Inner.Block;
        public float AttackTime => Inner.AttackTime;
        public int[][] Range => Inner.Range;
    }
}
