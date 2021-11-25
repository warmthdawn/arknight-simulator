using ArknightSimulator.Enemies;
using ArknightSimulator.EventHandlers;
using ArknightSimulator.Operations;
using ArknightSimulator.Operators;
using ArknightSimulator.Skills;
using ArknightSimulator.Utils;
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

            OperatorAttack(refresh, EnemiesAppear);
        }


        // 费用随时间增加
        public void CostIncreasing(int refresh)
        {
            if (CurrentCost < MaxCost)
            {
                int nextCostUnit = (CostUnit + 100 / refresh) % 100;
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
                if (op.CurrentDeploymentType != DeploymentType.Land) // Land 上干员才能阻挡敌人
                    continue;
                int blockCount = 0;
                List<int> blockId = new List<int>();

                foreach (EnemyMovement enemy in enemiesAppear) // 遍历场上所有敌人，找到碰撞的敌人
                {
                    EnemyTemplate emt = enemy.Enemy.Template;
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
            // 技力增加
            foreach(Operator op in OnMapOperators)
            {
                if (SkillOnOperators.Contains(op))
                    continue;
                if (op.Skill == null)
                    continue;

                if (op.Status.SkillPoint < op.Skill.Cost)
                {
                    StatusDecorator status = new StatusDecorator(op.Status);
                    int nextSkillPointUnit = (status.SkillPointUnit + 100 / refresh) % 100;
                    if (nextSkillPointUnit < status.SkillPointUnit)
                    {
                        status.SkillPoint++;
                    }
                    op.Status.SkillPointUnit = nextSkillPointUnit;
                }
            }


            // 正在使用的技能更新
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
        public void OperatorAttack(int attackRefresh, List<EnemyMovement> movements)
        {
            foreach (Operator op in OnMapOperators) // 遍历场上干员
            {
                if (op.AttackType == AttackType.None) // 不攻击的干员
                    continue;

                List<Enemy> attackEnemies = new List<Enemy>();  // 总的攻击对象
                op.AttackId.Clear();
                int attackCount = (int)op.AttackType;  // 攻击数量


                if (attackCount > 0)  // 攻击数量大于0
                {
                    if (op.BlockEnemiesId.Count != 0) // 有阻挡敌人，则优先攻击
                    {
                        int i = 0;
                        for (; i < attackCount && i < op.BlockEnemiesId.Count; i++)
                        {
                            var enemy = movements.Find(e => e.Enemy.InstanceId == op.BlockEnemiesId[i]);
                            attackEnemies.Add(enemy.Enemy);
                            op.AttackId.Add(enemy.Enemy.InstanceId);
                        }
                        attackCount -= i;

                        if (attackCount <= 0)  // 攻击数量满了，则不再攻击范围内的敌人
                        {
                            op.RefreshAttack(attackRefresh, attackEnemies);
                            continue;
                        }

                    }
                }


                int[][] range = op.Status.Range[op.Template.EliteLevel]; // 待优化
                int selfi = 0;
                int selfj = 0;
                // 寻找自身点
                for (int i = 0; i < range.Length; i++)
                {
                    int j = 0;
                    for (; j < range[i].Length; j++)
                    {
                        if (range[i][j] == 0)
                        {
                            selfi = i;
                            selfj = j;
                            break;
                        }
                    }
                    if (j < range[i].Length)
                        break;
                }

                List<EnemyMovement> enemiesInRange = new List<EnemyMovement>();
                // 扫描攻击范围
                for (int i = 0; i < range.Length; i++)
                {
                    for (int j = 0; j < range[i].Length; j++)
                    {
                        if (range[i][j] == 1)
                        {
                            int di = i - selfi;  // 攻击范围的某一格与自身行距离
                            int dj = j - selfj;  // 列距离
                            int mapi = 0;   // 攻击范围在地图的行
                            int mapj = 0;   // 列
                            switch (op.Direction)
                            {
                                case Directions.Up:
                                    mapi = op.MapY - dj;
                                    mapj = op.MapX + di;
                                    break;
                                case Directions.Down:
                                    mapi = op.MapY + dj;
                                    mapj = op.MapX - di;
                                    break;
                                case Directions.Left:
                                    mapi = op.MapY - di;
                                    mapj = op.MapX - dj;
                                    break;
                                case Directions.Right:
                                    mapi = op.MapY + di;
                                    mapj = op.MapX + dj;
                                    break;
                            }

                            foreach (EnemyMovement em in movements)
                            {
                                var pos = em.Enemy.Position;
                                if (pos.Y <= mapi + 1 && pos.Y >= mapi && pos.X <= mapj + 1 && pos.X >= mapj) // x，j是列，y，i是行
                                {
                                    if (!enemiesInRange.Contains(em))
                                        enemiesInRange.Add(em);
                                }

                            }
                        }
                    }
                }


                // 攻击所有敌人的干员单独计算
                if (attackCount == -1)
                {
                    for (int i = 0; i < op.BlockEnemiesId.Count; i++)
                    {
                        var enemyall = movements.Find(e => e.Enemy.InstanceId == op.BlockEnemiesId[i]);
                        attackEnemies.Add(enemyall.Enemy);
                        op.AttackId.Add(enemyall.Enemy.InstanceId);
                    }
                    foreach (var e in enemiesInRange)
                    {
                        if (!attackEnemies.Contains(e.Enemy))
                        {
                            attackEnemies.Add(e.Enemy);
                            op.AttackId.Add(e.Enemy.InstanceId);
                        }
                    }
                    op.RefreshAttack(attackRefresh, attackEnemies);
                    continue;
                }


                // 剩余攻击位
                if (enemiesInRange.Count > 0)
                {
                    List<EnemyMovement> selectEnemies = new List<EnemyMovement>();

                    foreach (var type in op.SearchEnemyType)
                    {
                        // TODO: 索敌类型增加
                        switch (type)
                        {
                            case SearchEnemyType.Default:
                                // 选择离我方据点最近的敌人
                                if (selectEnemies.Count == 0)
                                {
                                    for (int i = 0; i < attackCount; i++)
                                    {
                                        EnemyMovement movement = enemiesInRange.Find(e => !selectEnemies.Contains(e));
                                        double distance = movement.DistanceToHome();
                                        foreach (var e in enemiesInRange)
                                        {
                                            if (selectEnemies.Contains(e))
                                                continue;
                                            if (e.DistanceToHome() < distance)
                                            {
                                                movement = e;
                                                distance = movement.DistanceToHome();
                                            }
                                        }
                                        selectEnemies.Add(movement);
                                    }
                                }
                                else
                                    selectEnemies.Sort((a, b) => (int)(a.DistanceToHome() - b.DistanceToHome()));
                                break;
                            case SearchEnemyType.Fly:
                                foreach (var e in enemiesInRange)
                                {
                                    if (e.Enemy.Template.Type == EnemyType.Fly)
                                        selectEnemies.Add(e);
                                }
                                break;
                            case SearchEnemyType.NoFly:
                                foreach (var e in enemiesInRange)
                                {
                                    if (e.Enemy.Template.Type == EnemyType.Fly)
                                        selectEnemies.Remove(e);
                                }
                                break;
                            default:
                                throw new Exception("新的干员索敌类型未定义");
                        }
                    }

                    for (int i = 0; i < attackCount && i < selectEnemies.Count; i++)
                    {
                        if (!attackEnemies.Contains(selectEnemies[i].Enemy))
                        {
                            attackEnemies.Add(selectEnemies[i].Enemy);
                            op.AttackId.Add(selectEnemies[i].Enemy.InstanceId);
                        }
                    }

                    op.RefreshAttack(attackRefresh, attackEnemies);
                    continue;
                }

                op.RefreshAttack(attackRefresh, null);

            }
        }



        // 部署干员
        public Operator Deploying(OperatorTemplate opt, Directions direction, int mapX, int mapY, DeploymentType deploymentType)
        {
            if (NotOnMapOperators.Remove(opt) == false)
                throw new Exception("不存在该干员！");
            Operator op = new Operator();
            op.InstanceId = TotalDeploymentCount;
            TotalDeploymentCount++;
            //op.TemplateId = opt.Id;
            op.Status = new Status(opt.Status);
            op.AttackUnit = (int)(100 * op.Status.AttackTime);
            op.Template = opt;

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
                    case "SkillAttackEnhanceAlpha": 
                        op.Skill = new SkillAttackEnhanceAlpha(opt.SkillLevel);
                        op.Status.SkillPoint = op.Skill.Initial;
                        break;
                    default: break;
                }
            }

            op.MapX = mapX;
            op.MapY = mapY;
            op.Position = new Point { X = mapX + 0.5, Y = mapY + 0.5 };
            op.Direction = direction;
            op.CurrentDeploymentType = deploymentType;
            op.SearchEnemyType = new SearchEnemyType[opt.SearchEnemyType.Length];
            for (int i = 0; i < opt.SearchEnemyType.Length; i++)
            {
                op.SearchEnemyType[i] = opt.SearchEnemyType[i];
            }
            op.AttackType = opt.AttackType;
            op.DamageType = opt.DamageType;

            OnMapOperators.Add(op);

            CurrentCost -= op.Status.Cost[opt.EliteLevel];
            RestDeploymentCount -= op.Status.DeployCount;
            return op;
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
