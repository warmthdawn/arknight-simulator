using ArknightSimulator.Enemies;
using ArknightSimulator.EventHandlers;
using ArknightSimulator.Operations;
using ArknightSimulator.Operators;
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

        
        
        
        public void Init()
        {
            enemiesNotAppear = new List<EnemyMovement>();
            foreach (EnemyMovement t in operation.TimeLine)
            {
                enemiesNotAppear.Add(new EnemyMovement(t));
            }
            enemiesAppear = new List<EnemyMovement>();
            foreach (EnemyMovement enemy in enemiesNotAppear)
            {
                EnemyTemplate et = operation.AvailableEnemies.Find(e => e.Id == enemy.Enemy.TemplateId);
                enemy.Enemy.Status = new EnemyStatus(et.Status);
                enemy.PassPointCount = 0;
            }

            markTime = new Dictionary<string, float>();
            CurrentHomeLife = CurrentOperation.HomeLife;
            EnemyTotalCount = CurrentOperation.EnemyCount;
            CurrentEnemyCount = 0;
        }

        public void Update(float totalTime, int refresh)
        {
            EnemyAppearing(totalTime);

            EnemyMoving(totalTime, refresh);

            EnemyAttack();


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


                    OnEnemyAppearing(this, new EnemyEventArgs(newEnemy));
                }
            }
        }
        // 敌人移动
        private void EnemyMoving(float totalTime, int refresh)
        {
            List<EnemyMovement> goInsideEnemies = new List<EnemyMovement>(); // 进入据点的敌人
            foreach (var enemy in EnemiesAppear)
            {


                // ---------------------------------
                // 还要判断是否被阻挡
                // ---------------------------------


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
        private void EnemyAttack()
        {
            foreach (var enemy in EnemiesAppear)
            {

            }
        }


        public void GameOver()
        {
            foreach(var e in enemiesAppear)
            {
                OnEnemyRemove(this, new EnemyEventArgs(e));
            }

            enemiesNotAppear = null;
            enemiesAppear = null;
        }


    }
}
