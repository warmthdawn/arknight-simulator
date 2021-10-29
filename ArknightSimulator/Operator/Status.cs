using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operator
{
    class Status
    {
        int MaxLife { get; set; } // 最大生命
        int CurrentLife { get; set; } // 当前生命
        int SkillPoint { get; set; } // 技力
        int Attack { get; set; } // 攻击力
        int Defence { get; set; } // 防御
        int MagicDefence { get; set; } // 法术抗性
        int Time { get; set; } // 再部署时间
        int Cost { get; set; } // 部署费用
        int Block { get; set; } // 阻挡数
        float AttackTime { get; set; } // 攻击间隔 
        int[][] Range { get; set; } // 攻击范围
    }
}
