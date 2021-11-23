﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ArknightSimulator.Operators
{
    public class Status : IStatus
    {
        public int MaxLife { get; set; } // 最大生命
        public int CurrentLife { get; set; } // 当前生命
        public int SkillPoint { get; set; } // 技力
        public int Attack { get; set; } // 攻击力
        public int Defence { get; set; } // 防御 defence
        public int MagicDefence { get; set; } // 法术抗性
        public int Time { get; set; } // 再部署时间
        public int[] Cost { get; set; } // 部署费用
        public int DeployCount { get; set; } // 部署位
        public int Block { get; set; } // 阻挡数
        public float AttackTime { get; set; } // 攻击间隔 
        public int[][][] Range { get; set; } // 攻击范围

        public Status() { }
        public Status(IStatus status)
        {
            if (status == null)
                return;
            MaxLife = status.MaxLife;
            CurrentLife = status.CurrentLife;
            SkillPoint = status.SkillPoint;
            Attack = status.Attack;
            Defence = status.Defence;
            MagicDefence = status.MagicDefence;
            Time = status.Time;
            Cost = new int[status.Cost.Length];
            for (int i = 0; i < status.Cost.Length; i++)
            {
                Cost[i] = status.Cost[i];
            }
            DeployCount = status.DeployCount;
            Block = status.Block;
            AttackTime = status.AttackTime;
            Range = new int[status.Range.Length][][];
            for (int i = 0; i < status.Range.Length; i++)
            {
                Range[i] = new int[status.Range[i].Length][];
                for (int j = 0; j < status.Range[i].Length; j++)
                {
                    Range[i][j] = new int[status.Range[i][j].Length];
                    for (int k = 0; k < status.Range[i][j].Length; k++)
                    {
                        Range[i][j][k] = status.Range[i][j][k];
                    }
                }
            }
        }
    }
}
