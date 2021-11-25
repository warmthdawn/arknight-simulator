using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Enemies
{
    public class EnemyStatus : IEnemyStatus
    {
        public int _currentLife;
        public int Attack { get; set; } // 攻击力
        public int AttackSpeed { get; set; } // 攻击速度
        public float AttackTime { get; set; } // 攻击间隔
        public float Range { get; set; } // 攻击范围半径
        public int Count { get; set; } // 扣除数量
        public int Defence { get; set; } // 防御力
        public int MaxLife { get; set; } // 最大生命
        public int CurrentLife
        {
            get => _currentLife;
            set {
                if (value <= 0 && DieEvent != null)
                {
                    DieEvent();
                    _currentLife = 0;
                    return;
                }
                _currentLife = value;

            }
        } // 当前生命
        public int MagicDefence { get; set; } // 法术抗性
        public int RecoverSpeed { get; set; } // 生命恢复速度
        public int Weight { get; set; } // 重量等级
        public bool SilenceDefence { get; set; } // 沉默抗性 
        public bool DizzyDefence { get; set; } // 眩晕抗性
        public bool SleepDefence { get; set; } // 沉睡抗性
        public float MoveSpeed { get; set; } // 移动速度
        public Action DieEvent { get; set; }

        public EnemyStatus() { } // Json反序列化所需的无参构造器
        public EnemyStatus(IEnemyStatus enemyStatus)  // 深拷贝所需的构造器
        {
            if (enemyStatus == null)
                return;
            Attack = enemyStatus.Attack;
            AttackSpeed = enemyStatus.AttackSpeed;
            AttackTime = enemyStatus.AttackTime;
            Range = enemyStatus.Range;
            Count = enemyStatus.Count;
            Defence = enemyStatus.Defence;
            MaxLife = enemyStatus.MaxLife;
            CurrentLife = enemyStatus.CurrentLife;
            MagicDefence = enemyStatus.MagicDefence;
            RecoverSpeed = enemyStatus.RecoverSpeed;
            Weight = enemyStatus.Weight;
            SilenceDefence = enemyStatus.SilenceDefence;
            DizzyDefence = enemyStatus.DizzyDefence;
            SleepDefence = enemyStatus.SleepDefence;
            MoveSpeed = enemyStatus.MoveSpeed;
        }
    }
}
