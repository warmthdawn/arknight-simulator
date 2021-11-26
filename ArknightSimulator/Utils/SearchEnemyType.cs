using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Utils
{
    public enum SearchEnemyType : int   // 索敌类型（可多选）
    {
        Default = 0,     // 默认，即攻击离我方据点路线最近的敌人
        DefenceMax = 1,  // 防御力最高
        DefenceMin = 2,  // 防御力最低
        LifeMax = 3,     // 当前生命最高
        LifeMin = 4,     // 当前生命最低
        WeightMax = 5,   // 重量最大
        WeightMin = 6,   // 重量最小
        Blocked = 7,     // 被阻挡（优先、一般用于远程位）
        NotBlocked = 8,  // 未被阻挡（优先）
        Fly = 9,         // 飞行（优先）
        NoFly = 10,      // 不能攻击飞行（放在条件最后）
        OperatorLifePercentMin = 14   // 右方  当前生命比例最低
    }
}
