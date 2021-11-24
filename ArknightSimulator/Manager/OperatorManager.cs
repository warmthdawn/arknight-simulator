using ArknightSimulator.Enemies;
using ArknightSimulator.EventHandlers;
using ArknightSimulator.Operations;
using ArknightSimulator.Operators;
using ArknightSimulator.Skills;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using Point = ArknightSimulator.Operations.Point;

namespace ArknightSimulator.Manager
{
    public class OperatorManager : INotifyPropertyChanged    // 管理干员、费用等
    {
        private int currentCost;          // 当前费用
        private int maxCost;              // 最大费用
        private int costUnit;             // 费用单元
        private int restDeploymentCount;  // 剩余可部署人数 
        private int totalDeploymentCount; // 总部署次数，包括撤回的干员数


        public int CurrentCost { get => currentCost; set { currentCost = value; OnPropertyChanged(); } }
        public int MaxCost { get => maxCost; set => maxCost = value; }
        public int CostUnit { get => costUnit; set { costUnit = value; OnPropertyChanged(); } }
        public int RestDeploymentCount { get => restDeploymentCount; set { restDeploymentCount = value; OnPropertyChanged(); } }
        public int TotalDeploymentCount { get => totalDeploymentCount; set => totalDeploymentCount = value; }



        public List<OperatorTemplate> AvailableOperators { get; private set; } // 总干员列表
        public List<OperatorTemplate> SelectedOperators { get; set; }   // 选择出战的干员

        public ObservableCollection<OperatorTemplate> NotOnMapOperators { get; set; }   // 游戏中未上场干员
        public List<Operator> OnMapOperators { get; set; }   // 游戏中已上场干员
        public List<Operator> SkillOnOperators { get; set; } // 正在使用技能的干员



        public OperatorEventHandler OnOperatorEnable;   // 费用足够后干员可用（或不足不可用）事件


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public OperatorManager()
        {
            AvailableOperators = new List<OperatorTemplate>();
            LoadOperators();
        }

        public void LoadOperators(string path = "./Json/Operator")
        {
            try
            {
                string[] operatorFiles = Directory.GetFiles(path);
                foreach (string opFile in operatorFiles)
                {
                    string jsonString = File.ReadAllText(opFile);
                    OperatorTemplate op = JsonConvert.DeserializeObject<OperatorTemplate>(jsonString);
                    AvailableOperators.Add(op);
                }
                //                 string json = File.ReadAllText(path);
                //                 AvailableOperators = JsonConvert.DeserializeObject<List<OperatorTemplate>>(json);
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show(string.Format("找不到干员文件夹：{0}", path));
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("干员加载失败：{0}", e.Message));
            }

        }


        public void Init(List<OperatorTemplate> selected, int initialCost, int maxCost, int deploymentLimit)
        {
            SelectedOperators = new List<OperatorTemplate>();
            NotOnMapOperators = new ObservableCollection<OperatorTemplate>();
            foreach (OperatorTemplate opt in selected)
            {
                opt.ResetStatus();
                SelectedOperators.Add(new OperatorTemplate(opt));
                NotOnMapOperators.Add(new OperatorTemplate(opt));
            }

            OnMapOperators = new List<Operator>();
            SkillOnOperators = new List<Operator>();
            CurrentCost = (initialCost >= 0) ? initialCost : 0;
            MaxCost = (maxCost >= 0) ? maxCost : 0;
            RestDeploymentCount = (deploymentLimit >= 0) ? deploymentLimit : 0;
            CostUnit = 0;
            TotalDeploymentCount = 0;


        }

        public void Update(int refresh, List<EnemyMovement> EnemiesAppear, Operation CurrentOperation, double operatorColliderRadius, double enemyColliderRadius)
        {
            CostIncreasing(refresh);

            OperatorBlock(EnemiesAppear, CurrentOperation, operatorColliderRadius, enemyColliderRadius);

            SkillUsing(refresh);

            OperatorAttack();
        }


        // 费用随时间增加
        public void CostIncreasing(int costRefresh)
        {
            if (CurrentCost < MaxCost)
            {
                int nextCostUnit = (CostUnit + 100 / costRefresh) % 100;
                if (nextCostUnit < CostUnit)
                {
                    CurrentCost++;
                }
                CostUnit = nextCostUnit;
            }



            foreach (var opt in NotOnMapOperators)   // 判断费用是否足够放置干员
            {
                if (CurrentCost >= opt.Status.Cost[opt.EliteLevel])
                    OnOperatorEnable(this, new OperatorEventArgs(opt, true));
                else
                    OnOperatorEnable(this, new OperatorEventArgs(opt, false));
            }
        }

        // 干员阻挡 TODO 暂不考虑空降挤开和特殊敌人
        public void OperatorBlock(List<EnemyMovement> enemiesAppear, Operation currentOperation, double operatorColliderRadius, double enemyColliderRadius)
        {
            foreach (Operator op in OnMapOperators)
            {
                if (op.CurrentDeploymentType != DeploymentType.Land)
                    continue;
                int blockCount = 0;
                List<int> blockId = new List<int>();

                foreach (EnemyMovement enemy in enemiesAppear)
                {
                    EnemyTemplate emt = currentOperation.AvailableEnemies.Find(e => e.Id == enemy.Enemy.TemplateId);
                    if (emt.Type != EnemyType.Ground)
                        continue;

                    double distanceX = op.Position.X - enemy.Enemy.Position.X;
                    double distanceY = op.Position.Y - enemy.Enemy.Position.Y;
                    double distanceBlock = operatorColliderRadius + enemyColliderRadius;
                    if (distanceX * distanceX + distanceY * distanceY <= distanceBlock * distanceBlock)
                    {
                        blockCount++;
                        blockId.Add(enemy.Enemy.InstanceId);
                    }
                }
                op.BlockEnemyCount = blockCount;
                List<int> removeId = new List<int>();
                op.BlockEnemiesId.ForEach(e =>
                    {
                        if (!blockId.Contains(e))
                            removeId.Add(e);
                    });
                op.BlockEnemiesId.RemoveAll(e => removeId.Contains(e));
                blockId.ForEach(e =>
                    {
                        if (op.BlockEnemiesId.Count >= op.Status.Block)
                        {
                            op.BlockEnemyCount = op.Status.Block;
                            return;
                        }
                        if (!op.BlockEnemiesId.Contains(e))
                            op.BlockEnemiesId.Add(e);

                    });
            }
        }

        // 技能使用
        public void SkillUsing(int refresh)
        {
            List<Operator> skillOffOperators = new List<Operator>();
            foreach (Operator op in SkillOnOperators)
            {
                if (!op.SkillUpdate(refresh))
                {
                    op.SkillEnd();
                    skillOffOperators.Add(op);
                }
            }
            foreach(Operator op in skillOffOperators)
            {
                SkillOnOperators.Remove(op);
            }
        }

        // 干员攻击
        public void OperatorAttack()
        {

        }



        // 部署干员
        public void Deploying(OperatorTemplate opt, Directions direction, int mapX, int mapY, DeploymentType deploymentType)
        {
            if (NotOnMapOperators.Remove(opt) == false)
                throw new Exception("不存在该干员！");
            Operator op = new Operator();
            op.InstanceId = TotalDeploymentCount;
            TotalDeploymentCount++;
            op.TemplateId = opt.Id;
            op.Status = new Status(opt.Status);


            op.Gift = new Gift[opt.GiftNames.Length];
            if (opt.GiftNames.Length != 0)
            {
                foreach (string gift in opt.GiftNames)
                {
                    switch (gift)
                    {
                        default: break;
                    }
                }
            }


            if (opt.SkillNames.Length != 0)
            {
                string skill = opt.SkillNames[opt.SkillChooseId - 1];
                switch (skill)
                {
                    case "SkillAttackEnhanceAlpha": op.Skill = new SkillAttackEnhanceAlpha(opt.SkillLevel); break;
                    default: break;
                }
            }

            op.MapX = mapX;
            op.MapY = mapY;
            op.Position = new Point { X = mapX + 0.5, Y = mapY + 0.5 };
            op.Direction = direction;
            op.CurrentDeploymentType = deploymentType;

            OnMapOperators.Add(op);

            CurrentCost -= op.Status.Cost[opt.EliteLevel];
            RestDeploymentCount -= op.Status.DeployCount;
        }

        // 技能开始 TODO
        public void SkillStart(Operator op)
        {
            op.SkillStart();
        }



        public void GameOver()
        {
            SelectedOperators = null;
            NotOnMapOperators = null;
            OnMapOperators = null;
            SkillOnOperators = null;
        }
    }
}
