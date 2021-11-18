using System;
using System.Collections.Generic;
using System.Text;
using ArknightSimulator.Operations;

namespace ArknightSimulator.Enemies
{
    public class Enemy
    {
        public int InstanceId { get; set; }         // 敌人实例的ID
        public string TemplateId { get; set; }      // 敌人模板的ID
        public IEnemyStatus Status { get; set; }    // 属性状态
        public Point Position { get; set; }         // 坐标
        public void Move(double moveX, double moveY)
        {
            Position.X += moveX;
            Position.Y += moveY;
        }
        public void Attack() { }
        public void Hurt() { }
        public void SkillOn() { }

    }
}
