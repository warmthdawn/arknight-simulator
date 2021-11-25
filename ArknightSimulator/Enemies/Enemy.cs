using System;
using System.Collections.Generic;
using System.Text;
using ArknightSimulator.Operations;
using ArknightSimulator.Operators;
using ArknightSimulator.Utils;

namespace ArknightSimulator.Enemies
{
    public class Enemy
    {
        public int InstanceId { get; set; }         // 敌人实例的ID
        public EnemyTemplate Template { get; set; } // 敌人模板
        public string TemplateId { get; set; }      // 敌人模板的ID，用于反序列化时索引Template
        public IEnemyStatus Status { get; set; }    // 属性状态
        public Point Position { get; set; } = new Point();  // 坐标
        public bool IsBlocked { get; set; } = false; // 是否被阻挡
        public int BlockId { get; set; } = -1;    // 阻挡的干员ID
        public int AttackId { get; set; } = -1;   // 索敌：攻击的干员ID

        public Enemy() { }
        public Enemy(Enemy enemy)
        {
            if (enemy == null)
                return;
            InstanceId = enemy.InstanceId;
            Template = enemy.Template;  // 引用，无需new
            //TemplateId = enemy.TemplateId;
            Status = new EnemyStatus(enemy.Status);
            Position.X = enemy.Position.X;
            Position.Y = enemy.Position.Y;
            IsBlocked = enemy.IsBlocked;
            BlockId = enemy.BlockId;
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
        public void Hurt(DamageType type, int damage)
        {

        }
        public void SkillOn() { }

    }
}
