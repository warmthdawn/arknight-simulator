﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ArknightSimulator.Operator
{
    public class OperatorTemplate: INotifyPropertyChanged
    {
        private int eliteLevel;
        private int level;
        private int belief;
        private int potential;

        public string Id { get; set; }  // 编号
        public string Name { get; set; } // 代号
        public PositionType Position { get; set; } // 职业
        public string Picture { get; set; }// 图片
        public string ModelPicture { get; set; }  // 模型
        public int Rare { get; set; } // 稀有度
        public int EliteLevel { get => eliteLevel; set { eliteLevel = value; OnPropertyChanged(); } }  // 精英化等级
        public int Level { get => level; set { level = value; OnPropertyChanged(); } }     // 等级
        public int Belief { get => belief; set { belief = value; OnPropertyChanged(); } }   // 信赖
        public int Potential { get => potential; set { potential = value; OnPropertyChanged(); } } // 潜能
        public Status InitStatus { get; set; } // 初始属性状态
        public Status Status { get; set; } // 属性状态
        public Gift[] Gift { get; set; } // 天赋
        public Skill[] Skill { get; set; } // 技能

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

        // 重新设置属性（属性设置界面）
        public void ResetStatus()
        {
            Status status = new Status();
            status.MaxLife = StatusCalculator(LevelUpLife) + (int)Math.Round(BeliefUpStatus[0] / 2 * Belief * 1.0 / 50, MidpointRounding.AwayFromZero);
            status.Attack = StatusCalculator(LevelUpAttack) + (int)Math.Round(BeliefUpStatus[1] / 2 * Belief * 1.0 / 50, MidpointRounding.AwayFromZero);
            status.Defence = StatusCalculator(LevelUpDefence) + (int)Math.Round(BeliefUpStatus[2] / 2 * Belief * 1.0 / 50, MidpointRounding.AwayFromZero);
            status.MagicDefence = StatusCalculator(LevelUpMagicDefence) + (int)Math.Round(BeliefUpStatus[3] / 2 * Belief * 1.0 / 50, MidpointRounding.AwayFromZero);
            status.SkillPoint = InitStatus.SkillPoint;
            status.Time = InitStatus.Time;
            status.Cost = new int[InitStatus.Cost.Length];
            InitStatus.Cost.CopyTo(status.Cost, 0);
            status.Block = InitStatus.Block;
            status.AttackTime = InitStatus.AttackTime;
            status.Range = new int[InitStatus.Range.Length][][];
            for(int i=0;i< InitStatus.Range.Length;i++)
            {
                status.Range[i] = new int[InitStatus.Range[i].Length][];
                for(int j = 0;j< InitStatus.Range[i].Length;j++)
                {
                    status.Range[i][j] = new int[InitStatus.Range[i][j].Length];
                }
            }
            InitStatus.Range.CopyTo(status.Range, 0);


            for (int i = 0; i < Potential - 1; i++)
            {
                switch (PotentialValues[i].Type)
                {
                    case "MaxLife": status.MaxLife += PotentialValues[i].UpValue; break;
                    case "Attack": status.Attack += PotentialValues[i].UpValue; break;
                    case "Defence": status.Defence += PotentialValues[i].UpValue; break;
                    case "MagicDefence": status.MagicDefence += PotentialValues[i].UpValue; break;
                    case "Time": status.Time += PotentialValues[i].UpValue; break;
                    case "Cost":
                        for(int j = 0;j < status.Cost.Length;j++)
                        {
                            status.Cost[j] += PotentialValues[i].UpValue;
                        }
                        break;
                    case "AttackSpeed":
                        float speed = (InitStatus.AttackTime / status.AttackTime * 100) + PotentialValues[i].UpValue;
                        status.AttackTime = InitStatus.AttackTime * 100 / speed;
                        break;
                    default: break;
                }
            }

            status.CurrentLife = status.MaxLife;

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
