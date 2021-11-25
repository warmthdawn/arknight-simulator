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
        public int AttackUnit { get; set; } = 0; // 攻击冷却计数
        public Action<Enemy> AttackEvent { get; set; } // 敌人攻击事件
        public SearchOperatorType[] SearchOperatorType { get; set; } = new SearchOperatorType[] { }; // 索敌类型
        public AttackType AttackType { get; set; } // 攻击类型（不攻击、单体、群体）
        public DamageType DamageType { get; set; } // 伤害类型

        public Enemy() { }
        public Enemy(Enemy enemy)
        {
            if (enemy == null)
                return;
            InstanceId = enemy.InstanceId;
            Template = enemy.Template;  // 引用，无需new
            TemplateId = enemy.TemplateId;
            Status = new EnemyStatus(enemy.Status);
            Position.X = enemy.Position.X;
            Position.Y = enemy.Position.Y;
            IsBlocked = enemy.IsBlocked;
            BlockId = enemy.BlockId;
            AttackId = enemy.AttackId;
            AttackUnit = enemy.AttackUnit;
            SearchOperatorType = new SearchOperatorType[enemy.SearchOperatorType.Length];
            for (int i = 0; i < enemy.SearchOperatorType.Length; i++)
            {
                SearchOperatorType[i] = enemy.SearchOperatorType[i];
            }
            AttackType = enemy.AttackType;
            DamageType = enemy.DamageType;
        }

        public void RefreshAttack(int attackRefresh, List<Operator> op = null)
        {
            // 若无攻击对象且 下次可攻击则干员空闲
            if ((op == null || op.Count == 0) && (int)(100 * Status.AttackTime) - AttackUnit <= 100 / attackRefresh)
            {
                AttackUnit = (int)(100 * Status.AttackTime);
                return;
            }



            int next = (AttackUnit + 100 / attackRefresh) % (int)(100 * Status.AttackTime);
            if (op != null && next < AttackUnit)
            {
                foreach (var o in op)
                    Attack(o);
            }

            AttackUnit = next;
        }

        public void Move(double moveX, double moveY)
        {
            Position.X += moveX;
            Position.Y += moveY;
        }
        public void Attack(Operator op)
        {
            if (AttackEvent != null)
                AttackEvent(this); // 触发攻击事件

            op.Hurt(DamageType, Status.Attack);
        }
        public void Hurt(DamageType type, int damage)
        {
            int actualDamage = 0;
            switch (type)
            {
                case DamageType.Physical:
                    actualDamage = damage - Status.Defence; // Defence 计算交给装饰器 (目标防御力-数值减防御)×(1-百分比减防御)
                    break;
                case DamageType.Spell:
                    actualDamage = (int)(damage * 0.01 * (100 - Status.MagicDefence)); // 同上 法抗 = (目标法术抗性-数值减法抗)×(1-百分比减法抗)
                    break;
                case DamageType.True:
                    actualDamage = damage;
                    break;
                case DamageType.Heal:
                    actualDamage = -damage;
                    break;
                default:
                    throw new ArgumentException("非法伤害类型");
            }

            EnemyStatus newStatus = new EnemyStatus(Status);
            newStatus.CurrentLife -= actualDamage;
            Status = newStatus;
        }
        public void SkillOn() { }

    }
}
