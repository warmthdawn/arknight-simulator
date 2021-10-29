using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Enemy
{
    class EnemyTemplate
    {
        string Name { get; set; } // 名称
        string Picture { get; set; } // 图片
        int Level { get; set; } // 等级
        EnemyStatus Status { get; set; } // 属性状态
        EnemyGift Gift { get; set; }
        EnemySkill Skill { get; set; }
        
    }
}
