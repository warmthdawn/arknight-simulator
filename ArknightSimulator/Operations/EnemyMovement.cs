using System;
using System.Collections.Generic;
using System.Text;
using ArknightSimulator.Enemies;

namespace ArknightSimulator.Operations
{
    public class EnemyMovement
    {
        public Enemy Enemy { get; set; }
        public float EntryTime { get; set; }
        public List<Point> MovingPoints { get; set; }
        public int PassPointCount { get; set; }   // 已经过的路线检查点数


        public EnemyMovement() { }

        public EnemyMovement(EnemyMovement enemyMovement)
        {
            if (enemyMovement == null)
                return;
            Enemy = new Enemy(enemyMovement.Enemy);
            EntryTime = enemyMovement.EntryTime;
            MovingPoints = new List<Point>();
            foreach(var m in enemyMovement.MovingPoints)
            {
                MovingPoints.Add(new Point { X = m.X, Y = m.Y });
            }
            PassPointCount = enemyMovement.PassPointCount;
        }

    }

   
}
