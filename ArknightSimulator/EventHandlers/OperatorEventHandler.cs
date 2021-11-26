using System;
using System.Collections.Generic;
using System.Text;
using ArknightSimulator.Operators;

namespace ArknightSimulator.EventHandlers
{
    public delegate void OperatorEventHandler(object sender, OperatorEventArgs e);

    // 干员事件参数类
    public class OperatorEventArgs : EventArgs
    {
        public Operator Operator { get; set; }
        public OperatorTemplate OperatorTemplate { get; set; }
        public bool CostEnough { get; set; }
        public bool TimeEnough { get; set; }
        public int Index { get; set; }

        public OperatorEventArgs(Operator op)
        {
            Operator = op;
        }
        public OperatorEventArgs(OperatorTemplate opt)
        {
            OperatorTemplate = opt;
        }
        public OperatorEventArgs(OperatorTemplate opt, bool costEnough, bool timeEnough, int index)
        {
            OperatorTemplate = opt;
            CostEnough = costEnough;
            TimeEnough = timeEnough;
            Index = index;
        }

    }
}
