using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Enemy
{
    public class EnemyTemplate
    {
        public string Id { get; set; }  // 编号
        public string Name { get; set; } // 名称
        public string Picture { get; set; } // 图片
        public int Level { get; set; } // 等级
        public EnemyType Type { get; set; } // 敌人类型
        public IEnemyStatus Status { get; set; } // 属性状态
        public EnemyGift Gift { get; set; }
        public EnemySkill Skill { get; set; }
        
    }
}
