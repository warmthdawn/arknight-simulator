using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Enemy
{
    class EnemyStatus
    {
        int Attack { get; set; } // 攻击力
        int AttackSpeed { get; set; } // 攻击速度
        float AttackTime { get; set; } // 攻击间隔
        float Range { get; set; } // 攻击范围半径
        int Count { get; set; } // 扣除数量
        int Defence { get; set; } // 防御力
        int MaxLife { get; set; } // 最大生命
        int CurrentLife { get; set; } // 当前生命
        int MagicDefense { get; set; } // 法术抗性
        int RecoverSpeed { get; set; } // 生命恢复速度
        int Weight { get; set; } // 重量等级
        bool SilenceDefence { get; set; } // 沉默抗性 
        bool DizzyDefence { get; set; } // 眩晕抗性
        bool SleepDefence { get; set; } // 沉睡抗性
        float MoveSpeed { get; set; } // 移动速度
    }
}
