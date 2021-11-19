using System;
using System.Collections.Generic;
using System.Text;
using ArknightSimulator.Operations;

namespace ArknightSimulator.EventHandlers
{
    public delegate void EnemyEventHandler(object sender, EnemyEventArgs e);

    // 敌人事件参数类
    public class EnemyEventArgs : EventArgs
    {
        public EnemyMovement EnemyMovement { get; set; }
        public EnemyEventArgs(EnemyMovement enemy)
        {
            EnemyMovement = enemy;
        }
    }
}

