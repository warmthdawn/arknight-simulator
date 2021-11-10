using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Enemy
{
    public class Enemy
    {
        public EnemyTemplate Template { get; set; } // 模板
        public IEnemyStatus Status { get; set; } // 属性状态
        public void Attack() { }
        public void Hurt() { }
        public void SkillOn() { }

    }
}
