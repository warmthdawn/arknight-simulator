using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Utils
{
    public enum DamageType : int   // 伤害类型，与攻击类型（单体攻击、群体攻击）不同
    {
        Physical = 0, // 物理攻击
        Spell = 1, // 法术攻击
        True = 2, // 真实伤害
        Heal = 3 // 恢复
    }
}
