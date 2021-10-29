using System;
using System.Collections.Generic;
using System.Text;
using ArknightSimulator.Enemy;

namespace ArknightSimulator
{
    class Operation
    {
        string Name { get; set; }
        string Picture { get; set; }
        object[][] Map { get; set; }
        int DeploymengLimit { get; set; }
        int InitialCost { get; set; }
        int HomeLife { get; set; }
        int EnemyCount { get; set; }
        List<Enemy.Enemy> Enemies { get; set; }
    }
}
