using System;
using System.Collections.Generic;
using System.Text;
using ArknightSimulator.Operators;

namespace ArknightSimulator.Skills
{
    /* 攻击力强化 alpha型
     * 自动回复 手动触发
     * 等级   描述       初始  消耗  持续
     * 1    攻击力+10%    0    50    20
     * 2    攻击力+15%    0    50    20
     * 3    攻击力+20%    0    50    20
     * 4    攻击力+30%    0    45    20
     * 5    攻击力+35%    0    45    20
     * 6    攻击力+40%    0    45    20
     * 7    攻击力+50%    0    40    20
     * 
     * e.g. 玫兰莎、泡普卡、安德切尔一技能
     */
    class SkillAttackEnhanceAlpha : Skill
    {
        public float AttackEnhancePercent { get; set; }

        public SkillAttackEnhanceAlpha(int level)
        {
            RecoveryType = SkillRecoveryType.AutoRecovery;
            StartType = SkillStartType.ManualStart;

            switch(level)
            {
                case 1: 
                    Level = 1;
                    AttackEnhancePercent = 0.1f;
                    Initial = 0;
                    Cost = 50;
                    Time = 20;
                    break;
                case 2:
                    Level = 2;
                    AttackEnhancePercent = 0.15f;
                    Initial = 0;
                    Cost = 50;
                    Time = 20;
                    break;
                case 3:
                    Level = 3;
                    AttackEnhancePercent = 0.2f;
                    Initial = 0;
                    Cost = 50;
                    Time = 20;
                    break;
                case 4:
                    Level = 4;
                    AttackEnhancePercent = 0.3f;
                    Initial = 0;
                    Cost = 45;
                    Time = 20;
                    break;
                case 5:
                    Level = 5;
                    AttackEnhancePercent = 0.35f;
                    Initial = 0;
                    Cost = 45;
                    Time = 20;
                    break;
                case 6:
                    Level = 6;
                    AttackEnhancePercent = 0.4f;
                    Initial = 0;
                    Cost = 45;
                    Time = 20;
                    break;
                case 7:
                    Level = 7;
                    AttackEnhancePercent = 0.5f;
                    Initial = 0;
                    Cost = 40;
                    Time = 20;
                    break;
                default: throw new Exception("技能等级错误！");
            }
        }
        public SkillAttackEnhanceAlpha(Skill skill):base(skill)
        {
            AttackEnhancePercent = ((SkillAttackEnhanceAlpha)skill).AttackEnhancePercent;
        }

        public override IStatus Start(IStatus status)
        {
            return new Decorator(status, this);
        }

        public override IStatus End(IStatus status)
        {
            return status;
        }


        private class Decorator : StatusDecorator
        {
            private SkillAttackEnhanceAlpha skill;
            public Decorator(IStatus status, SkillAttackEnhanceAlpha skill) : base(status)
            {
                this.skill = skill;
            }

            public override int Attack => (int)(base.Attack + base.Attack*skill.AttackEnhancePercent);

        }

    }
}
