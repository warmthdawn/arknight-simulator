using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ArknightSimulator.Operators
{
    public class OperatorTemplate: INotifyPropertyChanged
    {
        private int eliteLevel;
        private int level;
        private int belief;
        private int potential;
        private int skillChooseId;
        private int skillLevel;

        public string Id { get; set; }  // 编号
        public string Name { get; set; } // 代号
        public PositionType Position { get; set; } // 职业
        public DeploymentType DeploymentType { get; set; } // 部署位置
        public string Picture { get; set; }// 图片
        public string ModelPicture { get; set; }  // 模型
        public string AttackPicture { get; set; } // 攻击动图
        public int Rare { get; set; } // 稀有度
        public int EliteLevel { get => eliteLevel; set { eliteLevel = value; OnPropertyChanged(); } }  // 精英化等级
        public int Level { get => level; set { level = value; OnPropertyChanged(); } }     // 等级
        public int Belief { get => belief; set { belief = value; OnPropertyChanged(); } }   // 信赖
        public int Potential { get => potential; set { potential = value; OnPropertyChanged(); } } // 潜能
        public Status InitStatus { get; set; } // 初始属性状态
        public Status Status { get; set; } // 属性状态
        public string[] GiftNames { get; set; } // 天赋名
        public string[] SkillNames { get; set; } // 技能名
        public int SkillChooseId { get=> skillChooseId; set { skillChooseId = value; OnPropertyChanged(); } }  // 选择的技能序号（从1开始，0表示无技能）
        public int SkillLevel { get => skillLevel; set { skillLevel = value; OnPropertyChanged(); } }    // 技能等级（从1开始，8、9、10表示专一、二、三）
        public int SkillMaxLevel { get; set; } // 技能最大等级


        public int[] LevelUpLife { get; set; }   // 包括各个阶段血量  精英0 1级 	精英0 满级 	精英1 满级 	精英2 满级
        public int[] LevelUpAttack { get; set; } // 包括各个阶段攻击
        public int[] LevelUpDefence { get; set; } // 包括各个阶段防御
        public int[] LevelUpMagicDefence { get; set; } // 包括各个阶段法抗
        public int[] BeliefUpStatus { get; set; }  // 信赖提升时各属性最大增值 生命上限，攻击，防御，法抗
        public PotentialModifier[] PotentialValues { get; set; }   // 每级潜能提升属性修改
        public class PotentialModifier
        {
            public string Type { get; set; }
            public int UpValue { get; set; }
        }



        public static int[][] LevelLimit = new int[][] // 等级限制，第一维是稀有度减一
        {
            new int[1]{30} ,
            new int[1]{30} ,
            new int[2]{40,55} ,
            new int[3]{45,60,70},
            new int[3]{50,70,80},
            new int[3]{50,80,90}
        };


        // 数据更新
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }






        public OperatorTemplate() { }
        public OperatorTemplate(OperatorTemplate opt)
        {
            if (opt == null)
                return;
            Id = opt.Id;
            Name = opt.Name;
            Position = opt.Position;
            DeploymentType = opt.DeploymentType;
            Picture = opt.Picture;
            ModelPicture = opt.ModelPicture;
            AttackPicture = opt.AttackPicture;
            Rare = opt.Rare;
            EliteLevel = opt.EliteLevel;
            Level = opt.Level;
            Belief = opt.Belief;
            Potential = opt.Potential;
            InitStatus = new Status(opt.InitStatus);
            Status = new Status(opt.Status);
            GiftNames = new string[opt.GiftNames.Length];
            for (int i = 0; i < opt.GiftNames.Length; i++)
            {
                GiftNames[i] = opt.GiftNames[i];
            }
            SkillNames = new string[opt.SkillNames.Length];
            for (int i = 0; i < opt.SkillNames.Length; i++)
            {
                SkillNames[i] = opt.SkillNames[i];
            }
            SkillChooseId = opt.SkillChooseId;
            SkillLevel = opt.SkillLevel;
            SkillMaxLevel = opt.SkillMaxLevel;
            LevelUpLife = new int[opt.LevelUpLife.Length];
            for (int i = 0; i < opt.LevelUpLife.Length; i++)
            {
                LevelUpLife[i] = opt.LevelUpLife[i];
            }
            LevelUpAttack = new int[opt.LevelUpAttack.Length];
            for (int i = 0; i < opt.LevelUpAttack.Length; i++)
            {
                LevelUpAttack[i] = opt.LevelUpAttack[i];
            }
            LevelUpDefence = new int[opt.LevelUpDefence.Length];
            for (int i = 0; i < opt.LevelUpDefence.Length; i++)
            {
                LevelUpDefence[i] = opt.LevelUpDefence[i];
            }
            LevelUpMagicDefence = new int[opt.LevelUpMagicDefence.Length];
            for (int i = 0; i < opt.LevelUpMagicDefence.Length; i++)
            {
                LevelUpMagicDefence[i] = opt.LevelUpMagicDefence[i];
            }
            BeliefUpStatus = new int[opt.BeliefUpStatus.Length];
            for (int i = 0; i < opt.BeliefUpStatus.Length; i++)
            {
                BeliefUpStatus[i] = opt.BeliefUpStatus[i];
            }
            PotentialValues = new PotentialModifier[opt.PotentialValues.Length];
            for (int i = 0; i < opt.PotentialValues.Length; i++)
            {
                PotentialValues[i] = new PotentialModifier { Type = opt.PotentialValues[i].Type, UpValue = opt.PotentialValues[i].UpValue };
            }
        }






        // 重新设置属性（属性设置界面）
        public void ResetStatus()
        {
            Status = new Status(InitStatus);

            Status.MaxLife = StatusCalculator(LevelUpLife) + (int)Math.Round(BeliefUpStatus[0] / 2 * Belief * 1.0 / 50, MidpointRounding.AwayFromZero);
            Status.Attack = StatusCalculator(LevelUpAttack) + (int)Math.Round(BeliefUpStatus[1] / 2 * Belief * 1.0 / 50, MidpointRounding.AwayFromZero);
            Status.Defence = StatusCalculator(LevelUpDefence) + (int)Math.Round(BeliefUpStatus[2] / 2 * Belief * 1.0 / 50, MidpointRounding.AwayFromZero);
            Status.MagicDefence = StatusCalculator(LevelUpMagicDefence) + (int)Math.Round(BeliefUpStatus[3] / 2 * Belief * 1.0 / 50, MidpointRounding.AwayFromZero);


            for (int i = 0; i < Potential - 1; i++)
            {
                switch (PotentialValues[i].Type)
                {
                    case "MaxLife": Status.MaxLife += PotentialValues[i].UpValue; break;
                    case "Attack": Status.Attack += PotentialValues[i].UpValue; break;
                    case "Defence": Status.Defence += PotentialValues[i].UpValue; break;
                    case "MagicDefence": Status.MagicDefence += PotentialValues[i].UpValue; break;
                    case "Time": Status.Time += PotentialValues[i].UpValue; break;
                    case "Cost":
                        for(int j = 0; j < Status.Cost.Length;j++)
                        {
                            Status.Cost[j] += PotentialValues[i].UpValue;
                        }
                        break;
                    case "AttackSpeed":
                        float speed = (InitStatus.AttackTime / Status.AttackTime * 100) + PotentialValues[i].UpValue;
                        Status.AttackTime = InitStatus.AttackTime * 100 / speed;
                        break;
                    default: break;
                }
            }

            Status.CurrentLife = Status.MaxLife;

        }

        private int StatusCalculator(int[] levelUpV)
        {
            double value = levelUpV[EliteLevel] 
                + (Level - 1) * (levelUpV[EliteLevel + 1] - levelUpV[EliteLevel]) * 1.0
                / (LevelLimit[Rare - 1][EliteLevel] - 1);

            return (int)Math.Round(value, MidpointRounding.AwayFromZero);

        }

    }
}
