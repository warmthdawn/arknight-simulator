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
        public int Rare { get; set; } // 稀有度
        public int EliteLevel { get; set; } = 1;// 精英化等级
        public int Level { get; set; } = 1;// 等级
        public int Belief { get; set; } = 1;// 信赖
        public int Potential { get; set; } = 1; // 潜能
        public Status Status { get; set; } // 属性状态
        public Gift[] Gift { get; set; } // 天赋
        public Skill[] Skill { get; set; } // 技能

        public int[] LevelUpLife { get; set; }   // 包括各个阶段血量  精英0 1级 	精英0 满级 	精英1 满级 	精英2 满级
        public int[] LevelUpAttack { get; set; } // 包括各个阶段攻击
        public int[] LevelUpDefense { get; set; } // 包括各个阶段防御
        public int[] LevelUpMagicDefence { get; set; } // 包括各个阶段法抗
        public int[] BeliefUpStatus { get; set; }  // 信赖提升时各属性最大增值 生命上限，攻击，防御，法抗
        public PotentialModifier[] PotentialValues { get; set; }   // 每级潜能提升属性修改
        public class PotentialModifier
        {
            public string Type { get; set; }
            public int UpValue { get; set; }
        }


        public static Dictionary<int, int[]> LevelLimit   // 等级限制
            = new Dictionary<int, int[]>
            {
                {1, new int[1]{30} },
                {2, new int[1]{30} },
                {3, new int[2]{40,55} },
                {4, new int[3]{45,60,70} },
                {5, new int[3]{50,70,80} },
                {6, new int[3]{50,80,90} }
            };

    }
}
