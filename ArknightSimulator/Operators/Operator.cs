using System;
using System.Collections.Generic;
using System.Text;
using ArknightSimulator.Enemies;
using ArknightSimulator.Operations;

namespace ArknightSimulator.Operators
{
    public class Operator // 已部署干员
    {
        public int InstanceId { get; set; }    // 干员实例的ID
        public string TemplateId { get; set; } // 干员模板的ID
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
        public int AttackId { get; set; } = -1;   // 索敌：攻击的敌人ID


        public void Attack(Enemy enemy)
        {

            
        }
        public void Hurt()
        {

        
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
