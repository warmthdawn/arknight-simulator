using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Enemy
{
    public class EnemyStatus : IEnemyStatus
    {
        public int Attack { get; set; } // 攻击力
        public int AttackSpeed { get; set; } // 攻击速度
        public float AttackTime { get; set; } // 攻击间隔
        public float Range { get; set; } // 攻击范围半径
        public int Count { get; set; } // 扣除数量
        public int Defense { get; set; } // 防御力
        public int MaxLife { get; set; } // 最大生命
        public int CurrentLife { get; set; } // 当前生命
        public int MagicDefense { get; set; } // 法术抗性
        public int RecoverSpeed { get; set; } // 生命恢复速度
        public int Weight { get; set; } // 重量等级
        public bool SilenceDefence { get; set; } // 沉默抗性 
        public bool DizzyDefense { get; set; } // 眩晕抗性
        public bool SleepDefence { get; set; } // 沉睡抗性
        public float MoveSpeed { get; set; } // 移动速度
    }
}
