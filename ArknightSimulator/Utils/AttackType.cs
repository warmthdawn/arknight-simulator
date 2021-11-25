using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Utils
{
    public enum AttackType : int   // 攻击类型，与伤害类型不同
    {
        RangeAll = -1, // 群体攻击（无限制）
        None = 0,     // 不攻击
        Single = 1,   // 单体攻击
        Range2 = 2,   // 群体攻击（同时两个）
        Range3 = 3,   // 群体攻击（同时三个）
    }
}
