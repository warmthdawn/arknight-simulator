using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Enemy
{
    public class EnemyGiftStatusDecorator: EnemyStatusDecorator
    {
        public EnemyGiftStatusDecorator(IEnemyStatus status) : base(status)
        {

        }
    }
}
