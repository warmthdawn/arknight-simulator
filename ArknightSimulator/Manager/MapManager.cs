using ArknightSimulator.Enemies;
using ArknightSimulator.EventHandlers;
using ArknightSimulator.Operations;
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
    public class MapManager
    {
        private Operation operation;
        private List<EnemyMovement> enemiesNotAppear;
        private List<EnemyMovement> enemiesAppear;


        public Operation CurrentOperation => operation;
        public List<EnemyMovement> EnemiesNotAppear => enemiesNotAppear;
        public List<EnemyMovement> EnemiesAppear => enemiesAppear;


        public EnemyEventHandler OnEnemyAppearing;

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
            foreach(EnemyMovement t in operation.TimeLine)
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
        }

        public void Update(float totalTime)
        {
            EnemyAppearing(totalTime);

            EnemyMoving();

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
        private void EnemyMoving()
        {
            foreach(var enemy in EnemiesAppear)
            {
                
            }
        }




        public void GameOver()
        {
            enemiesNotAppear = null;
            enemiesAppear = null;

        }


    }
}
