using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operator
{
    public class Status : IStatus
    {
        public int MaxLife { get; set; } // 最大生命
        public int CurrentLife { get; set; } // 当前生命
        public int SkillPoint { get; set; } // 技力
        public int Attack { get; set; } // 攻击力
        public int Defence { get; set; } // 防御 defence
        public int MagicDefence { get; set; } // 法术抗性
        public int Time { get; set; } // 再部署时间
        public int Cost { get; set; } // 部署费用
        public int Block { get; set; } // 阻挡数
        public float AttackTime { get; set; } // 攻击间隔 
        public int[][] Range { get; set; } // 攻击范围
    }
}
