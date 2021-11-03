using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operator
{
    class Operator // 已部署干员
    {
        OperatorTemplate Template { get; set; } // 模板
        IStatus Status { get; set; } // 属性状态

        void Attack() { }
        void Hurt() { }
        void SkillOn() { }
        
    }
}
