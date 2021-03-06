using ArknightSimulator.Enemies;
using ArknightSimulator.EventHandlers;
using ArknightSimulator.Operations;
using ArknightSimulator.Operators;
using ArknightSimulator.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using Point = ArknightSimulator.Operations.Point;

namespace ArknightSimulator.Manager
{
    public class MapManager : INotifyPropertyChanged    // 管理地图和敌人
    {
        private Operation operation;
        private List<EnemyMovement> enemiesNotAppear;
        private List<EnemyMovement> enemiesAppear;
        private Dictionary<string, float> markTime;



        private int currentHomeLife;          // 当前据点耐久
        private int enemyTotalCount;          // 敌人总数
        private int currentEnemyCount;        // 当前敌人数


        public Operation CurrentOperation => operation;
        public List<EnemyMovement> EnemiesNotAppear => enemiesNotAppear;
        public List<EnemyMovement> EnemiesAppear => enemiesAppear;
        public int CurrentHomeLife { get => currentHomeLife; set { currentHomeLife = value; OnPropertyChanged(); } }
        public int EnemyTotalCount { get => enemyTotalCount; set { enemyTotalCount = value; OnPropertyChanged(); } }
        public int CurrentEnemyCount { get => currentEnemyCount; set { currentEnemyCount = value; OnPropertyChanged(); } }


        public PointType[][] Map { get; set; }     // 地图


        public EnemyEventHandler OnEnemyAppearing; // 敌人出现事件
        public EnemyEventHandler OnEnemyMoving;    // 敌人出现事件
        public EnemyEventHandler OnEnemyRemove;    // 敌人移除事件


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public MapManager()
        {


        }
        // 载入地图
        public bool LoadOperation(string name)
        {
            try
            {
                string json = File.ReadAllText("./Json/Map/" + name + ".json");
                operation = JsonConvert.DeserializeObject<Operation>(json);
            }
            catch (FileNotFoundException e)
            {
                MessageBox.Show(string.Format("找不到地图 '{0}' 的定义", name));
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show("加载地图失败" + e.ToString());
                return false;
            }

            return true;
        }



        // 开始作战时初始化
        public void Init()
        {
            enemiesNotAppear = new List<EnemyMovement>();
            foreach (EnemyMovement t in operation.TimeLine)
            {
                //EnemyMovement em = new EnemyMovement(t);
                t.Enemy.Template = CurrentOperation.AvailableEnemies.Find(e => e.Id == t.Enemy.TemplateId);
                enemiesNotAppear.Add(new EnemyMovement(t));
            }
            enemiesAppear = new List<EnemyMovement>();
            foreach (EnemyMovement enemy in enemiesNotAppear)
            {
                EnemyTemplate et = enemy.Enemy.Template;
                enemy.Enemy.Status = new EnemyStatus(et.Status);
                enemy.PassPointCount = 0;
            }

            markTime = new Dictionary<string, float>();
            CurrentHomeLife = CurrentOperation.HomeLife;
            EnemyTotalCount = CurrentOperation.EnemyCount;
            CurrentEnemyCount = 0;

            Map = new PointType[CurrentOperation.MapHeight][];
            for (int i = 0; i < Map.Length; i++)
            {
                Map[i] = new PointType[CurrentOperation.MapWidth];
                for (int j = 0; j < Map[i].Length; j++)
                {
                    Map[i][j] = CurrentOperation.Map[i][j];
                }
            }
        }

        // 每帧更新
        public void Update(float totalTime, int refresh, List<Operator> OnMapOperators, double operatorColliderRadius, double enemyColliderRadius)
        {
            EnemyAppearing(totalTime);

            EnemyMoving(totalTime, refresh, OnMapOperators, operatorColliderRadius, enemyColliderRadius);

            EnemyAttack(refresh, OnMapOperators, operatorColliderRadius);

        }


        // 敌人出现
        private void EnemyAppearing(float totalTime)
        {
            List<EnemyMovement> newEnemies = EnemiesNotAppear.FindAll(e => e.EntryTime <= totalTime);
            if (newEnemies != null)
            {
                foreach (EnemyMovement newEnemy in newEnemies)
                {
                    EnemiesNotAppear.Remove(newEnemy);
                    EnemiesAppear.Add(newEnemy);
                    newEnemy.Enemy.Position.X = newEnemy.MovingPoints[0].X;
                    newEnemy.Enemy.Position.Y = newEnemy.MovingPoints[0].Y;
                    newEnemy.PassPointCount = 1;
                    newEnemy.Enemy.SearchOperatorType = new SearchOperatorType[newEnemy.Enemy.Template.SearchOperatorType.Length];
                    for (int i = 0; i < newEnemy.Enemy.Template.SearchOperatorType.Length; i++)
                    {
                        newEnemy.Enemy.SearchOperatorType[i] = newEnemy.Enemy.Template.SearchOperatorType[i];
                    }
                    newEnemy.Enemy.AttackType = newEnemy.Enemy.Template.AttackType;
                    newEnemy.Enemy.DamageType = newEnemy.Enemy.Template.DamageType;

                    OnEnemyAppearing(this, new EnemyEventArgs(newEnemy));
                }
            }
        }

        // 判断是否被阻挡 TODO 暂不考虑空降挤开和特殊敌人
        private bool EnemyBlocked(EnemyMovement enemy, List<Operator> onMapOperators, double operatorColliderRadius, double enemyColliderRadius)
        {
            EnemyTemplate emt = enemy.Enemy.Template;
            if (emt.Type != EnemyType.Ground)
                return false;

            foreach (Operator op in onMapOperators)
            {
                if (op.CurrentDeploymentType != DeploymentType.Land)
                    continue;

                double distanceX = op.Position.X - enemy.Enemy.Position.X;
                double distanceY = op.Position.Y - enemy.Enemy.Position.Y;
                double distanceBlock = operatorColliderRadius + enemyColliderRadius;
                if (distanceX * distanceX + distanceY * distanceY <= distanceBlock * distanceBlock)
                {
                    if (enemy.Enemy.IsBlocked)
                        return true;
                    if (op.BlockEnemiesId.Contains(enemy.Enemy.InstanceId))
                    {
                        enemy.Enemy.BlockId = op.InstanceId;
                        enemy.Enemy.BlockOp = op;
                        enemy.Enemy.IsBlocked = true;
                        return true;
                    }
                }
            }
            return false;
        }


        // 敌人移动
        private void EnemyMoving(float totalTime, int refresh, List<Operator> onMapOperators, double operatorColliderRadius, double enemyColliderRadius)
        {
            List<EnemyMovement> goInsideEnemies = new List<EnemyMovement>(); // 进入据点的敌人
            foreach (var enemy in EnemiesAppear)
            {
                // 判断是否被阻挡
                if (EnemyBlocked(enemy, onMapOperators, operatorColliderRadius, enemyColliderRadius) == true)
                    continue;

                enemy.Enemy.IsBlocked = false;
                enemy.Enemy.BlockId = -1;
                enemy.Enemy.BlockOp = null;


                double x = enemy.MovingPoints[enemy.PassPointCount].X - enemy.Enemy.Position.X;
                double y = enemy.MovingPoints[enemy.PassPointCount].Y - enemy.Enemy.Position.Y;
                bool isTurningDirection = false;
                if (enemy.PassPointCount == 1
                    && Math.Abs(enemy.MovingPoints[0].X - enemy.Enemy.Position.X) < double.Epsilon
                    && Math.Abs(enemy.MovingPoints[0].Y - enemy.Enemy.Position.Y) < double.Epsilon)
                    isTurningDirection = true;


                // 抵达检查点
                if (x * x + y * y < 4 * Math.Pow(enemy.Enemy.Status.MoveSpeed * 0.5 / refresh, 2))
                {
                    // 判断是否正在滞留
                    if (markTime.ContainsKey("enemy" + enemy.Enemy.InstanceId + "Stay"))
                    {
                        if (totalTime - markTime["enemy" + enemy.Enemy.InstanceId + "Stay"] < 1)
                            continue;
                        else
                            markTime.Remove("enemy" + enemy.Enemy.InstanceId + "Stay");
                    }



                    enemy.PassPointCount++;


                    // 是否进入我方据点
                    if (enemy.PassPointCount >= enemy.MovingPoints.Count)
                    {
                        CurrentHomeLife -= enemy.Enemy.Status.Count;
                        CurrentEnemyCount++;
                        goInsideEnemies.Add(enemy);
                        OnEnemyRemove(this, new EnemyEventArgs(enemy));
                        continue;
                    }


                    // 判断是否需要滞留
                    if (Math.Abs(enemy.MovingPoints[enemy.PassPointCount].X
                        - enemy.MovingPoints[enemy.PassPointCount - 1].X) < double.Epsilon
                        && Math.Abs(enemy.MovingPoints[enemy.PassPointCount].Y
                        - enemy.MovingPoints[enemy.PassPointCount - 1].Y) < double.Epsilon)
                    {
                        markTime.Add("enemy" + enemy.Enemy.InstanceId + "Stay", totalTime);
                        continue;
                    }



                    x = enemy.MovingPoints[enemy.PassPointCount].X - enemy.Enemy.Position.X;
                    y = enemy.MovingPoints[enemy.PassPointCount].Y - enemy.Enemy.Position.Y;
                    isTurningDirection = true;
                }

                // 开始移动
                double distance = Math.Sqrt(x * x + y * y);
                double moveX = enemy.Enemy.Status.MoveSpeed * 0.5 * x / distance / refresh;
                double moveY = enemy.Enemy.Status.MoveSpeed * 0.5 * y / distance / refresh;

                enemy.Enemy.Move(moveX, moveY);

                double passPointX = enemy.MovingPoints[enemy.PassPointCount].X - enemy.MovingPoints[enemy.PassPointCount - 1].X;
                double passPointY = enemy.MovingPoints[enemy.PassPointCount].Y - enemy.MovingPoints[enemy.PassPointCount - 1].Y;
                if (passPointX < 0)
                    OnEnemyMoving(this, new EnemyEventArgs(enemy, Directions.Left, isTurningDirection));
                else if (passPointX > 0)
                    OnEnemyMoving(this, new EnemyEventArgs(enemy, Directions.Right, isTurningDirection));
                else
                {
                    if (passPointY < 0)
                        OnEnemyMoving(this, new EnemyEventArgs(enemy, Directions.Up, isTurningDirection));
                    else
                        OnEnemyMoving(this, new EnemyEventArgs(enemy, Directions.Down, isTurningDirection));
                }

            }

            foreach (var i in goInsideEnemies)  // 进入据点的敌人要从列表中删除
            {
                EnemiesAppear.Remove(i);
            }
        }


        // 敌人攻击
        private void EnemyAttack(int refresh, List<Operator> OnMapOperators, double operatorColliderRadius)
        {
            foreach (var e in EnemiesAppear)
            {
                Enemy enemy = e.Enemy;
                if (enemy.AttackType == AttackType.None) // 不攻击的敌人
                    continue;

                List<Operator> attackop = new List<Operator>();  // 总的攻击对象
                enemy.AttackId.Clear();
                int attackCount = (int)enemy.AttackType;  // 攻击数量


                if (attackCount > 0)  // 攻击数量大于0
                {
                    if (enemy.IsBlocked == true) // 被阻挡，则优先攻击
                    {
                        var op = OnMapOperators.Find(o => o.InstanceId == enemy.BlockId);
                        attackop.Add(op);
                        enemy.AttackId.Add(op.InstanceId);

                        attackCount--;

                        if (attackCount <= 0)  // 攻击数量满了，则不再攻击范围内的干员
                        {
                            enemy.RefreshAttack(refresh, attackop);
                            continue;
                        }

                    }
                }

                if (enemy.Status.Range == -1)   // 攻击范围为-1的敌人，即近战攻击（阻挡攻击）
                {
                    enemy.RefreshAttack(refresh, null);
                    continue;
                }


                // 敌人在攻击半径内检测到干员的碰撞体（半径为operatorColliderRadius）则攻击
                List<Operator> opInRange = OnMapOperators.FindAll(o => (o.Position - enemy.Position) <= enemy.Status.Range + operatorColliderRadius);


                // 攻击半径内所有干员的敌人单独计算
                if (attackCount == -1)
                {
                    var op = OnMapOperators.Find(o => o.InstanceId == enemy.BlockId);
                    attackop.Add(op);
                    enemy.AttackId.Add(op.InstanceId);
                    foreach (var o in opInRange)
                    {
                        if (!attackop.Contains(o))
                        {
                            attackop.Add(o);
                            enemy.AttackId.Add(o.InstanceId);
                        }
                    }
                    enemy.RefreshAttack(refresh, attackop);
                    continue;
                }



                // 剩余攻击位
                if (opInRange.Count > 0)
                {
                    List<Operator> selectop = new List<Operator>();
                    foreach (var type in enemy.SearchOperatorType)
                    {
                        // TODO: 索敌类型增加
                        switch (type)
                        {
                            case SearchOperatorType.Default:
                                // 最后部署的干员
                                if (selectop.Count == 0)
                                {
                                    for (int i = 0; i < attackCount; i++)
                                    {
                                        int maxId = 0;
                                        foreach (var o in opInRange)
                                        {
                                            if (selectop.Contains(o))
                                                continue;
                                            if (o.InstanceId > maxId)
                                            {
                                                maxId = o.InstanceId;
                                            }
                                        }
                                        selectop.Add(opInRange.Find(o => o.InstanceId == maxId));
                                    }
                                }
                                else
                                    selectop.Sort((a, b) => b.InstanceId - a.InstanceId);
                                break;
                            default: throw new Exception("新的敌人索敌类型未定义");
                        }
                    }


                    for (int i = 0; i < attackCount && i < selectop.Count; i++)
                    {
                        if (!attackop.Contains(selectop[i]))
                        {
                            attackop.Add(selectop[i]);
                            enemy.AttackId.Add(selectop[i].InstanceId);
                        }
                    }


                    enemy.RefreshAttack(refresh, attackop);
                    continue;
                }

                enemy.RefreshAttack(refresh, null);
            }


        }


        public void GameOver()
        {
            foreach (var e in enemiesAppear)
            {
                OnEnemyRemove(this, new EnemyEventArgs(e));
            }

            enemiesNotAppear = null;
            enemiesAppear = null;
            Map = null;
        }


    }
}
