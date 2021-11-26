using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Enemies
{
    public class EnemyStatusDecorator : IEnemyStatus
    {
        public EnemyStatusDecorator(IEnemyStatus status)
        {
            this.Inner = status;
        }
        public IEnemyStatus Inner { get; private set; }
        public virtual int Attack => Inner.Attack;

        public virtual int AttackSpeed => Inner.AttackSpeed;

        public virtual float AttackTime => Inner.AttackTime;

        public virtual int Count => Inner.Count;

        public virtual int CurrentLife => Inner.CurrentLife;

        public virtual int Defence => Inner.Defence;

        public virtual bool DizzyDefence => Inner.DizzyDefence;

        public virtual int MagicDefence => Inner.MagicDefence;

        public virtual int MaxLife => Inner.MaxLife;

        public virtual float MoveSpeed => Inner.MoveSpeed;

        public virtual float Range => Inner.Range;

        public virtual int RecoverSpeed => Inner.RecoverSpeed;

        public virtual bool SilenceDefence => Inner.SilenceDefence;

        public virtual bool SleepDefence => Inner.SleepDefence;

        public virtual int Weight => Inner.Weight;

        public virtual Action DieEvent => Inner.DieEvent;
    }
}
