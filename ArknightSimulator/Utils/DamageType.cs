using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Utils
{
    public enum DamageType : int
    {
        Physical = 0, // 物理攻击
        Spell = 1, // 法术攻击
        True = 2, // 真实伤害
        Heal = 3 // 恢复
    }
}
