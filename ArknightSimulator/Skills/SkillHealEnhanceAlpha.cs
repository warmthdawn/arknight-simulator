using System;
using System.Collections.Generic;
using System.Text;
using ArknightSimulator.Operators;

namespace ArknightSimulator.Skills
{
    /* 治疗强化 alpha型
     * 自动回复 手动触发
     * 等级   描述       初始  消耗  持续
     * 1    攻击力+10%    0    40    20
     * 2    攻击力+15%    0    40    20
     * 3    攻击力+20%    0    40    20
     * 4    攻击力+30%    0    35    20
     * 5    攻击力+35%    0    35    20
     * 6    攻击力+40%    0    35    20
     * 7    攻击力+50%    0    30    20
     * 
     * e.g. 芙蓉一技能
     */
    class SkillHealEnhanceAlpha :Skill
    {
        public float HealEnhancePercent { get; set; }

        public SkillHealEnhanceAlpha(int level)
        {
            RecoveryType = SkillRecoveryType.AutoRecovery;
            StartType = SkillStartType.ManualStart;

            switch (level)
            {
                case 1:
                    Level = 1;
                    HealEnhancePercent = 0.1f;
                    Initial = 0;
                    Cost = 40;
                    Time = 20;
                    break;
                case 2:
                    Level = 2;
                    HealEnhancePercent = 0.15f;
                    Initial = 0;
                    Cost = 40;
                    Time = 20;
                    break;
                case 3:
                    Level = 3;
                    HealEnhancePercent = 0.2f;
                    Initial = 0;
                    Cost = 40;
                    Time = 20;
                    break;
                case 4:
                    Level = 4;
                    HealEnhancePercent = 0.3f;
                    Initial = 0;
                    Cost = 35;
                    Time = 20;
                    break;
                case 5:
                    Level = 5;
                    HealEnhancePercent = 0.35f;
                    Initial = 0;
                    Cost = 35;
                    Time = 20;
                    break;
                case 6:
                    Level = 6;
                    HealEnhancePercent = 0.4f;
                    Initial = 0;
                    Cost = 35;
                    Time = 20;
                    break;
                case 7:
                    Level = 7;
                    HealEnhancePercent = 0.5f;
                    Initial = 0;
                    Cost = 30;
                    Time = 20;
                    break;
                default: throw new Exception("技能等级错误！");
            }
        }
        public SkillHealEnhanceAlpha(Skill skill) : base(skill)
        {
            HealEnhancePercent = ((SkillHealEnhanceAlpha)skill).HealEnhancePercent;
        }

        public override IStatus Start(IStatus status)
        {
            CurrentTime = Time;
            TimeUnit = 100;
            IsUsing = true;
            return new Decorator(status, this);
        }

        public override IStatus End(IStatus status)
        {
            return status;
        }


        private class Decorator : StatusDecorator
        {
            private SkillHealEnhanceAlpha skill;
            public Decorator(IStatus status, SkillHealEnhanceAlpha skill) : base(status)
            {
                this.skill = skill;
            }

            public override int Attack => (int)(base.Attack + base.Attack * skill.HealEnhancePercent);
            public override int SkillPoint => 0;

        }
    }
}
