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
        public int InstanceId { get; set; }    // 干员实例的ID
        public OperatorTemplate Template { get; set; } // 干员实例
        //public string TemplateId { get; set; } // 干员模板的ID
        public IStatus Status { get; set; }    // 属性状态
        public Gift[] Gift { get; set; } // 天赋
        public Skill Skill { get; set; } // 技能
        public Point Position { get; set; } = new Point();  // 坐标
        public int MapX { get; set; }   // 地图的格子横坐标
        public int MapY { get; set; }   // 地图的格子纵坐标
        public Directions Direction { get; set; }  // 干员方向
        public DeploymentType CurrentDeploymentType { get; set; }  // 部署后的部署位置
        public int BlockEnemyCount { get; set; } = 0;  // 正在阻挡敌人数
        public List<int> BlockEnemiesId { get; set; } = new List<int>();  // 正在阻挡敌人
        public int AttackId { get; set; } = -1;   // 索敌：攻击的敌人ID   TODO:是否要删
        public int AttackUnit { get; set; } = 0; // 攻击冷却计数
        public Action<Operator> AttackEvent { get; set; } // 干员攻击事件
        public DamageType AttackType { get; set; } // 攻击伤害类型

        public void RefreshAttack(int attackRefresh, Enemy enemy = null)
        {
            // 若无攻击对象且 下次可攻击则干员空闲
            if (enemy == null && (int)(100 * Status.AttackTime) - AttackUnit <= 100 / attackRefresh)
            {
                AttackUnit = (int)(100 * Status.AttackTime);
                return;
            }



            int next = (AttackUnit + 100 / attackRefresh) % (int)(100 * Status.AttackTime);
            if (enemy != null && next < AttackUnit)
                Attack(enemy);
            AttackUnit = next;
        }

        private void Attack(Enemy enemy)
        {
            if (AttackEvent != null)
                AttackEvent(this); // 触发攻击事件

            enemy.Hurt(AttackType, Status.Attack);
        }
        public void Hurt(DamageType type, int damage)
        {
            switch (type)
            {
                case DamageType.Physical:
                        break;
                case DamageType.Spell:
                    break;
                case DamageType.True:
                    break;
                case DamageType.Heal:
                    break;
                default:
                    throw new ArgumentException("非法伤害类型");
            }

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
            Status = Skill.End(Status);
        }

    }
}
