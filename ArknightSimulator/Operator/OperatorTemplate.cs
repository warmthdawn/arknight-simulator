using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operator
{
    public class OperatorTemplate
    {
        public string Name { get; set; } // 代号
        public string Picture { get; set; } // 图片
        public int EliteLevel { get; set; } // 精英化等级
        public int Level { get; set; } // 等级
        public int Belief { get; set; } // 信赖
        public int Potential { get; set; } // 潜能
        public Status Status { get; set; } // 属性状态
        public Gift[] Gift { get; set; } // 天赋
        public Skill[] Skill { get; set; } // 技能
    }
}
