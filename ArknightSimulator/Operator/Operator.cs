using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operator
{
    public class Operator // 已部署干员
    {
        public OperatorTemplate Template { get; set; } // 模板
        public IStatus Status { get; set; } // 属性状态
        public void Attack() { }
        public void Hurt() { }
        public void SkillOn() { }
        
    }
}
