using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operator
{
    public enum PositionType : int
    {
        Vanguard = 0,   // 先锋
        Guard = 1,      // 近卫
        Sniper = 2,     // 狙击
        Defender = 3,   // 重装
        Medic = 4,      // 医疗
        Supporter = 5,  // 辅助
        Caster = 6,     // 术师
        Specialist = 7, // 特种 
    }
}
