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
        public int CurrentTime { get; set; } // 当前技能已使用时间
        public int TimeUnit { get; set; }   // 已使用时间单元时间
        public SkillRecoveryType RecoveryType { get; set; }   // 技力回复类型
        public SkillStartType StartType { get; set; }   // 触发类型

        public Skill() { }
        public Skill(Skill skill)
        {
            Level = skill.Level;
            Initial = skill.Initial;
            Cost = skill.Cost;
            Time = skill.Time;
            CurrentTime = skill.CurrentTime;
            TimeUnit = skill.TimeUnit;
            RecoveryType = skill.RecoveryType;
            StartType = skill.StartType;
        }

        public virtual IStatus Start(IStatus status)
        {
            return default;
        }
        public bool Update(int refresh)
        {
            if (CurrentTime >= Time)
                return false;

            int nextTimeUnit = (TimeUnit + 100 / refresh) % 100;
            if (nextTimeUnit < TimeUnit)
            {
                CurrentTime++;
            }
            TimeUnit = nextTimeUnit;
            return true;
        }

        public virtual IStatus End(IStatus status)
        {
            return default;
        }

    }
}
