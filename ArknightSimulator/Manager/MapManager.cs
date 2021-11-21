using ArknightSimulator.Enemies;
using ArknightSimulator.EventHandlers;
using ArknightSimulator.Operations;
using ArknightSimulator.Operators;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using Point = ArknightSimulator.Operations.Point;

namespace ArknightSimulator.Manager
{
    public class MapManager    // 管理地图和敌人
    { 
        private Operation operation;
        private List<EnemyMovement> enemiesNotAppear;
        private List<EnemyMovement> enemiesAppear;
        private Dictionary<string, float> markTime;

        private int currentHomeLife;          // 当前据点耐久


        public Operation CurrentOperation => operation;
        public List<EnemyMovement> EnemiesNotAppear => enemiesNotAppear;
        public List<EnemyMovement> EnemiesAppear => enemiesAppear;
        private int CurrentHomeLife => currentHomeLife;



        public EnemyEventHandler OnEnemyAppearing;   // 敌人出现事件
        public EnemyEventHandler OnEnemyMoving;   // 敌人出现事件
        public EventHandler OnLose;   // 作战失败
        public EnemyEventHandler OnEnemyRemove;   // 敌人移除事件


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
            currentHomeLife = CurrentOperation.HomeLife;
        }

        public void Update(float totalTime, int costRefresh)
        {
            EnemyAppearing(totalTime);

            EnemyMoving(totalTime, costRefresh);

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
        private void EnemyMoving(float totalTime, int costRefresh)
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
                if (x * x + y * y < 4 * Math.Pow(enemy.Enemy.Status.MoveSpeed * 0.5 / costRefresh, 2))
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


                    // 判断是否需要滞留
                    if (Math.Abs(enemy.MovingPoints[enemy.PassPointCount].X
                        - enemy.MovingPoints[enemy.PassPointCount - 1].X) < double.Epsilon
                        && Math.Abs(enemy.MovingPoints[enemy.PassPointCount].Y
                        - enemy.MovingPoints[enemy.PassPointCount - 1].Y) < double.Epsilon)
                    {
                        markTime.Add("enemy" + enemy.Enemy.InstanceId + "Stay", totalTime);
                        continue;
                    }


                    if (enemy.PassPointCount >= enemy.MovingPoints.Count)  // 进入我方据点
                    {
                        currentHomeLife -= enemy.Enemy.Status.Count;
                        if (currentHomeLife <= 0)
                        {
                            Lose();
                            return;
                        }
                        goInsideEnemies.Add(enemy);
                        OnEnemyRemove(this, new EnemyEventArgs(enemy));
                        continue;
                    }
                    x = enemy.MovingPoints[enemy.PassPointCount].X - enemy.Enemy.Position.X;
                    y = enemy.MovingPoints[enemy.PassPointCount].Y - enemy.Enemy.Position.Y;

                    isTurningDirection = true;
                }

                double distance = Math.Sqrt(x * x + y * y);
                double moveX = enemy.Enemy.Status.MoveSpeed * 0.5 * x / distance / costRefresh;
                double moveY = enemy.Enemy.Status.MoveSpeed * 0.5 * y / distance / costRefresh;

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





        public void Lose()
        {
            OnLose(this, null);
        }
        public void GameOver()
        {
            enemiesNotAppear = null;
            enemiesAppear = null;

        }


    }
}
