using System;
using System.Collections.Generic;
using System.Text;
using ArknightSimulator.Enemy;

namespace ArknightSimulator.Operation
{
    public class Operation
    {
        public string Name { get; set; }
        public string Picture { get; set; }
        public PointType[][] Map { get; set; }
        public int DeploymengLimit { get; set; }
        public int InitialCost { get; set; }
        public int HomeLife { get; set; }
        public int EnemyCount { get; set; }
        public List<Enemy.Enemy> Enemies { get; set; }
    }
}
