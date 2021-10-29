using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Enemy
{
    class Enemy
    {
        EnemyTemplate Template { get; set; } // 模板
        EnemyStatus Status { get; set; } // 属性状态

        void Attack() { }
        void Hurt() { }
        void SkillOn() { }

    }
}
