using System;
using System.Collections.Generic;
using System.Text;
using ArknightSimulator.Operations;

namespace ArknightSimulator.Operators
{
    public class Operator // 已部署干员
    {
        public int InstanceId { get; set; }    // 干员实例的ID
        public string TemplateId { get; set; } // 干员模板的ID
        public IStatus Status { get; set; }    // 属性状态
        public Point Position { get; set; } = new Point();// 坐标
        public int MapX { get; set; }   // 地图的格子横坐标
        public int MapY { get; set; }   // 地图的格子纵坐标
        public Directions Direction { get; set; }  // 干员方向
        public void Attack() { }
        public void Hurt() { }
        public void SkillOn() { }
        
    }
}
