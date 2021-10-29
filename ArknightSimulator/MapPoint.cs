using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator
{
    enum PointType
    {

    }
    class MapPoint
    {
        PointType Type { get; set; }
        bool Enable { get; set; }
        bool OperatorStand { get; set; }
    }
}
