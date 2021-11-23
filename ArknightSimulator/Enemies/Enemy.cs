using System;
using System.Collections.Generic;
using System.Text;
using ArknightSimulator.Operations;
using ArknightSimulator.Operators;

namespace ArknightSimulator.Enemies
{
    public class Enemy
    {
        public int InstanceId { get; set; }         // 敌人实例的ID
        public string TemplateId { get; set; }      // 敌人模板的ID
        public IEnemyStatus Status { get; set; }    // 属性状态
        public Point Position { get; set; } = new Point();  // 坐标
        public int AttackId { get; set; } = -1;   // 索敌：攻击的干员ID

        public Enemy() { }
        public Enemy(Enemy enemy)
        {
            if (enemy == null)
                return;
            InstanceId = enemy.InstanceId;
            TemplateId = enemy.TemplateId;
            Status = new EnemyStatus(enemy.Status);
            Position.X = enemy.Position.X;
            Position.Y = enemy.Position.Y;
            AttackId = enemy.AttackId;
        }


        public void Move(double moveX, double moveY)
        {
            Position.X += moveX;
            Position.Y += moveY;
        }
        public void Attack(Operator op)
        {

        }
        public void Hurt()
        {

        }
        public void SkillOn() { }

    }
}
