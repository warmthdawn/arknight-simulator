using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Enemy
{
    public class EnemySkillStatusDecorator: EnemyStatusDecorator
    {
        public EnemySkillStatusDecorator(IEnemyStatus status) : base(status)
        {

        }
    }
}
