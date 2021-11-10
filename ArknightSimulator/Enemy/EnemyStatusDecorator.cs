using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Enemy
{
    public class EnemyStatusDecorator : IEnemyStatus
    {
        public EnemyStatusDecorator(IEnemyStatus status)
        {
            this.Inner = status;
        }
        public IEnemyStatus Inner { get; private set; }
        public int Attack => Inner.Attack;

        public int AttackSpeed => Inner.AttackSpeed;

        public float AttackTime => Inner.AttackTime;

        public int Count => Inner.Count;

        public int CurrentLife => Inner.CurrentLife;

        public int Defence => Inner.Defence;

        public bool DizzyDefence => Inner.DizzyDefence;

        public int MagicDefence => Inner.MagicDefence;

        public int MaxLife => Inner.MaxLife;

        public float MoveSpeed => Inner.MoveSpeed;

        public float Range => Inner.Range;

        public int RecoverSpeed => Inner.RecoverSpeed;

        public bool SilenceDefence => Inner.SilenceDefence;

        public bool SleepDefence => Inner.SleepDefence;

        public int Weight => Inner.Weight;
    }
}
