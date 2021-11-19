using ArknightSimulator.Enemies;
using ArknightSimulator.Operators;
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
    public class OperatorManager : INotifyPropertyChanged
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



        //public EventHandler OnCostIncrease;


        public OperatorManager()
        {
            AvailableOperators = new List<OperatorTemplate>();
            LoadOperators();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
            CurrentCost = (initialCost >= 0) ? initialCost : 0;
            MaxCost = (maxCost >= 0) ? maxCost : 0;
            RestDeploymentCount = (deploymentLimit >= 0) ? deploymentLimit : 0;
            CostUnit = 0;
            totalDeploymentCount = 0;
        }
        public void Update(float totalTime, int costRefresh)
        {
            if (CurrentCost < MaxCost)
            {
                int nextCostUnit = (CostUnit + 100 / costRefresh) % 100;
                if (nextCostUnit < CostUnit)
                    CurrentCost++;
                CostUnit = nextCostUnit;
            }

        }

        // 部署干员
        public void Deploying(OperatorTemplate opt, Directions direction, int mapX, int mapY)
        {
            if (NotOnMapOperators.Remove(opt) == false)
                throw new Exception("不存在该干员！");
            Operator op = new Operator();
            op.InstanceId = TotalDeploymentCount;
            TotalDeploymentCount++;
            op.TemplateId = opt.Id;
            op.Status = new Status(opt.Status);
            op.MapX = mapX;
            op.MapY = mapY;
            op.Position = new Point { X = mapX + 0.5, Y = mapY + 0.5 };
            op.Direction = direction;


            CurrentCost -= op.Status.Cost[opt.EliteLevel];
            RestDeploymentCount--;
        }





        public void GameOver()
        {
            SelectedOperators = null;
            NotOnMapOperators = null;
            OnMapOperators = null;
        }
    }
}
