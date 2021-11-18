using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Enemies
{
    public class EnemyGiftStatusDecorator: EnemyStatusDecorator
    {
        public EnemyGiftStatusDecorator(IEnemyStatus status) : base(status)
        {

        }
    }
}
