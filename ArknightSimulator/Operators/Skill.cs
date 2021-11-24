using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operators
{
    // 技力回复类型
    public enum SkillRecoveryType
    {
        AutoRecovery,      // 自动回复
        AttackRecovery,    // 攻击回复
        HurtRecovery       // 受击回复
    }
    // 触发类型
    public enum SkillStartType
    {
        AutoStart,    // 自动触发
        ManualStart,  // 手动触发
    }

    public class Skill
    {
        public int Level { get; set; }       // 技能等级
        public int Initial { get; set; }     // 初始技力
        public int Cost { get; set; }        // 启动消耗
        public int Time { get; set; }        // 持续时间
        public SkillRecoveryType RecoveryType { get; set; }   // 技力回复类型
        public SkillStartType StartType { get; set; }   // 触发类型

        public Skill() { }
        public Skill(Skill skill)
        {
            Level = skill.Level;
            Initial = skill.Initial;
            Cost = skill.Cost;
            Time = skill.Time;
            RecoveryType = skill.RecoveryType;
            StartType = skill.StartType;
        }

        public virtual IStatus Start(IStatus status)
        {
            return new Decorator(status, this);
        }

        public virtual IStatus End(IStatus status)
        {
            return default;
        }


        private class Decorator : SkillStatusDecorator
        {
            private Skill skill;
            public Decorator(IStatus status, Skill skill) : base(status)
            {
                this.skill = skill;
            }

            public override int Attack => base.Attack + skill.Level;

        }
    }
}
