using ArknightSimulator.Enemy;
using ArknightSimulator.Operator;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Manager
{
    public class DataManager
    {
        public List<OperatorTemplate> AvailableOperators { get; private set; }
        public List<EnemyTemplate> AvailableEnemies { get; private set; }
    }
}
