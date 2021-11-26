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
        private IEnemyStatus _status;
        public int InstanceId { get; set; }         // 敌人实例的ID
        public EnemyTemplate Template { get; set; } // 敌人模板
        public string TemplateId { get; set; }      // 敌人模板的ID，用于反序列化时索引Template
        public IEnemyStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                if (_status is EnemyStatus) // 更新绑定死亡事件
                    ((EnemyStatus)_status).DieEvent += () => { DieEvent(this); };
            }
        }    // 属性状态
        public Point Position { get; set; } = new Point();  // 坐标
        public bool IsBlocked { get; set; } = false; // 是否被阻挡
        public int BlockId { get; set; } = -1;    // 阻挡的干员ID
        public Operator BlockOp { get; set; } // 阻挡干员引用
        public List<int> AttackId { get; set; } = new List<int>();  // 索敌：攻击的干员ID
        public int AttackUnit { get; set; } = 0; // 攻击冷却计数
        public Action<Enemy> AttackEvent { get; set; } // 敌人攻击事件
        public Action<Enemy> DieEvent { get; set; } // 敌人死亡事件
        public SearchOperatorType[] SearchOperatorType { get; set; } = new SearchOperatorType[] { }; // 索敌类型
        public AttackType AttackType { get; set; } // 攻击类型（不攻击、单体、群体）
        public DamageType DamageType { get; set; } // 伤害类型
        public bool IsChanged { get; set; } // 图片是否需要重新加载

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
            BlockOp = enemy.BlockOp;
            AttackId = new List<int>();
            foreach (var i in enemy.AttackId)
                AttackId.Add(i);
            AttackUnit = enemy.AttackUnit;
            SearchOperatorType = new SearchOperatorType[enemy.SearchOperatorType.Length];
            for (int i = 0; i < enemy.SearchOperatorType.Length; i++)
            {
                SearchOperatorType[i] = enemy.SearchOperatorType[i];
            }
            AttackType = enemy.AttackType;
            DamageType = enemy.DamageType;
            IsChanged = enemy.IsChanged;
        }

        public void RefreshAttack(int attackRefresh, List<Operator> ops = null, List<Enemy> enemies = null)
        {
            // 若无攻击对象且 下次可攻击则干员空闲
            if ((ops == null || ops.Count == 0) && (enemies == null || enemies.Count == 0) && (int)(100 * Status.AttackTime) - AttackUnit <= 100 / attackRefresh)
            {
                AttackUnit = (int)(100 * Status.AttackTime) - 50;// 暂定半秒延迟
                return;
            }

            int next = (AttackUnit + 100 / attackRefresh) % (int)(100 * Status.AttackTime);
            if (ops != null && next < AttackUnit)
            {
                foreach (var o in ops)
                    Attack(o);
            }
            if (enemies != null && next < AttackUnit)
            {
                foreach (var e in enemies)
                    Attack(e);
            }

            AttackUnit = next;
        }

        public void Move(double moveX, double moveY)
        {
            Position.X += moveX;
            Position.Y += moveY;
        }
        public void Attack(object opOrenemy)
        {
            if (AttackEvent != null)
                AttackEvent(this); // 触发攻击事件

            if (opOrenemy is Operator)
                ((Operator)opOrenemy).Hurt(DamageType, Status.Attack);
            else if (opOrenemy is Enemy)
                ((Enemy)opOrenemy).Hurt(DamageType, Status.Attack);
        }
        public void Hurt(DamageType type, int damage)
        {
            int actualDamage = 0;
            switch (type)
            {
                case DamageType.Physical:
                    if (Status.Defence >= 0.95 * damage)
                        actualDamage = (int)0.05 * damage;  // 保底伤害
                    else
                        actualDamage = damage - Status.Defence; // Defence 计算交给装饰器 (目标防御力-数值减防御)×(1-百分比减防御)
                    break;
                case DamageType.Spell:
                    if (Status.MagicDefence >= 95)
                        actualDamage = (int)0.05 * damage;   // 保底伤害
                    else
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

            //EnemyStatus newStatus = new EnemyStatus(Status);
            //newStatus.CurrentLife -= actualDamage;
            ((EnemyStatus)Status).CurrentLife -= actualDamage;
            if (((EnemyStatus)Status).CurrentLife > ((EnemyStatus)Status).MaxLife)
                ((EnemyStatus)Status).CurrentLife = ((EnemyStatus)Status).MaxLife;
        }
        public void SkillOn() { }

    }
}
