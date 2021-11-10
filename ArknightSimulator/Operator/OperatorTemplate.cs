using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operator
{
    public class OperatorTemplate
    {
        public string Id { get; set; }  // 编号
        public string Name { get; set; } = "临光"; // 代号
        public string Picture { get; set; } = "../Image/operator.png"; // 图片
        public string ModelPicture { get; set; }  // 模型
        public int EliteLevel { get; set; } = 1;// 精英化等级
        public int Level { get; set; } = 1;// 等级
        public int Belief { get; set; } = 1;// 信赖
        public int Potential { get; set; } = 1; // 潜能
        public Status Status { get; set; } // 属性状态
        public Gift[] Gift { get; set; } // 天赋
        public Skill[] Skill { get; set; } // 技能

    }
}
