using System;
using System.Collections.Generic;
using System.Text;
using ArknightSimulator.Enemies;
using ArknightSimulator.Operations;
using ArknightSimulator.Utils;

namespace ArknightSimulator.Operators
{
    public class Operator // 已部署干员
    {
        private IStatus _status;


        public int InstanceId { get; set; }    // 干员实例的ID
        public OperatorTemplate Template { get; set; } // 干员实例
        //public string TemplateId { get; set; } // 干员模板的ID
        public IStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                if (_status is Status) // 更新绑定死亡事件
                    ((Status)_status).DieEvent += () => { DieEvent(this); };
            }
        }    // 属性状态
        public Gift[] Gift { get; set; } // 天赋
        public Skill Skill { get; set; } // 技能
        public Point Position { get; set; } = new Point();  // 坐标
        public int MapX { get; set; }   // 地图的格子横坐标
        public int MapY { get; set; }   // 地图的格子纵坐标
        public Directions Direction { get; set; }  // 干员方向
        public DeploymentType CurrentDeploymentType { get; set; }  // 部署后的部署位置
        public int BlockEnemyCount { get; set; } = 0;  // 正在阻挡敌人数
        public List<int> BlockEnemiesId { get; set; } = new List<int>();  // 正在阻挡敌人
        public List<int> AttackId { get; set; } = new List<int>();   // 索敌：攻击的敌人ID   TODO:是否要删
        public int AttackUnit { get; set; } = 0; // 攻击冷却计数
        public Action<Operator> AttackEvent { get; set; } // 干员攻击事件
        public Action<Operator> DieEvent { get; set; } // 干员死亡事件
        public SearchEnemyType[] SearchEnemyType { get; set; } = new SearchEnemyType[] { }; // 索敌类型
        public AttackType AttackType { get; set; } // 攻击类型（不攻击、单体、群体）
        public DamageType DamageType { get; set; } // 攻击伤害类型
        public bool IsChanged { get; set; } // 攻击

        public void RefreshAttack(int attackRefresh, List<Enemy> enemies = null)
        {
            // 若无攻击对象且 下次可攻击则干员空闲
            if ((enemies == null || enemies.Count == 0) && (int)(100 * Status.AttackTime) - AttackUnit <= 100 / attackRefresh)
            {
                AttackUnit = (int)(100 * Status.AttackTime) - 50;// 暂定半秒延迟
                return;
            }

            int next = (AttackUnit + 100 / attackRefresh) % (int)(100 * Status.AttackTime);
            if (enemies != null && next < AttackUnit)
            {
                if (AttackEvent != null)
                    AttackEvent(this); // 触发攻击事件
                foreach (var e in enemies)
                    Attack(e);
            }

            AttackUnit = next;
        }

        private void Attack(Enemy enemy)
        {
            
            enemy.Hurt(DamageType, Status.Attack);
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

            ((Status)Status).CurrentLife -= actualDamage;
            if (((Status)Status).CurrentLife > ((Status)Status).MaxLife)
                ((Status)Status).CurrentLife = ((Status)Status).MaxLife;

        }
        public void SkillStart()
        {
            Status = Skill.Start(Status);
        }
        public bool SkillUpdate(int refresh)
        {
            return Skill.Update(refresh);
        }
        public void SkillEnd()
        {
            Status = Skill.End(Template.Status);
        }

    }
}
