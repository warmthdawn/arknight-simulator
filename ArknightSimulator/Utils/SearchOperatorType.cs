using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Utils
{
    public enum SearchOperatorType : int   // 索敌类型（可多选）
    {
        Default = 0,     // 默认，即攻击最后部署的干员（而阻挡自己的干员更优先）
    }
}
