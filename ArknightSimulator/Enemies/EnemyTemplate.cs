using ArknightSimulator.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Enemies
{
    public class EnemyTemplate
    {
        public string Id { get; set; }  // 编号
        public string Name { get; set; } // 名称
        public string Picture { get; set; } // 图片
        public string MovePicture { get; set; } // 移动图片
        public string AttackPicture { get; set; } // 攻击图片
        public int Level { get; set; } // 等级
        public EnemyType Type { get; set; } // 敌人类型
        public EnemyStatus Status { get; set; } // 属性状态
        public EnemyGift Gift { get; set; }
        public EnemySkill Skill { get; set; }
        public SearchOperatorType[] SearchOperatorType { get; set; } // 索敌类型
        public AttackType AttackType { get; set; } // 攻击类型（不攻击、单体、群体）
        public DamageType DamageType { get; set; } // 伤害类型
        
    }
}
