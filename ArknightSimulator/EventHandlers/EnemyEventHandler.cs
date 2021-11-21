using System;
using System.Collections.Generic;
using System.Text;
using ArknightSimulator.Operations;
using ArknightSimulator.Operators;

namespace ArknightSimulator.EventHandlers
{
    public delegate void EnemyEventHandler(object sender, EnemyEventArgs e);

    // 敌人事件参数类
    public class EnemyEventArgs : EventArgs
    {
        public EnemyMovement EnemyMovement { get; set; }
        public Directions Direction { get; set; }
        public bool IsTurningDirection { get; set; }
        public EnemyEventArgs(EnemyMovement enemy)
        {
            EnemyMovement = enemy;
        }
        public EnemyEventArgs(EnemyMovement enemy, Directions direction,bool isTurningDirection)
        {
            EnemyMovement = enemy;
            Direction = direction;
            IsTurningDirection = isTurningDirection;
        }
    }
}

