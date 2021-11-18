using System;
using System.Collections.Generic;
using System.Text;
using ArknightSimulator.Enemies;

namespace ArknightSimulator.Operations
{
    public class Operation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public int MapWidth { get; set; }
        public int MapHeight { get; set; }
        public PointType[][] Map { get; set; }
        public int DeploymentLimit { get; set; }
        public int InitialCost { get; set; }
        public int MaxCost { get; set; }
        public int HomeLife { get; set; }
        public int EnemyCount { get; set; }
        public List<EnemyMovement> TimeLine { get; set; }

        public List<EnemyTemplate> AvailableEnemies { get; set; }
    }
}
