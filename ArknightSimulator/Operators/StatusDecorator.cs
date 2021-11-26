using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operators
{
    public class StatusDecorator : IStatus
    {
        public StatusDecorator(IStatus status)
        {
            this.Inner = status;
        }
        public IStatus Inner { get; private set; }

        public virtual int MaxLife => Inner.MaxLife;
        public virtual int CurrentLife => Inner.CurrentLife;
        public virtual int SkillPoint { get => Inner.SkillPoint; set => Inner.SkillPoint = value; }    // 技力需要更新
        public virtual int SkillPointUnit { get => Inner.SkillPointUnit; set => Inner.SkillPointUnit = value; }   // 技力单元需要更新
        public virtual int Attack => Inner.Attack;
        public virtual int Defence => Inner.Defence;
        public virtual int MagicDefence => Inner.MagicDefence;
        public virtual int Time => Inner.Time;
        public virtual int CurrentTime => Inner.CurrentTime;
        public virtual int[] Cost => Inner.Cost;
        public virtual int DeployCount => Inner.DeployCount;
        public virtual int Block => Inner.Block;
        public virtual float AttackTime => Inner.AttackTime;
        public virtual int[][][] Range => Inner.Range;


    }
}
