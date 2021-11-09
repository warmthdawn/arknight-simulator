using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using ArknightSimulator.Enemy;

namespace ArknightSimulator.Operation
{
    public class EnemyMovement
    {
        public EnemyTemplate Enemy { get; set; }
        public int EntryTime { get; set; }
        public List<Point> MovingPoints { get; set; }


    }

   
}
