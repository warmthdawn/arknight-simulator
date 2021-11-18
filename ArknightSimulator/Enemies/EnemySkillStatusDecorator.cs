using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Enemies
{
    public class EnemySkillStatusDecorator: EnemyStatusDecorator
    {
        public EnemySkillStatusDecorator(IEnemyStatus status) : base(status)
        {

        }
    }
}
