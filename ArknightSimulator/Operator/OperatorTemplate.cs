using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operator
{
    class OperatorTemplate
    {
        string Name { get; set; } // 代号
        string Picture { get; set; } // 图片
        int EliteLevel { get; set; } // 精英化等级
        int Level { get; set; } // 等级
        int Belief { get; set; } // 信赖
        int Potential { get; set; } // 潜能
        Status Status { get; set; } // 属性状态
        Gift[] Gift { get; set; } // 天赋
        Skill[] Skill { get; set; } // 技能
    }
}
