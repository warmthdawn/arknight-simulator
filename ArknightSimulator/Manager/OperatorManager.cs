using ArknightSimulator.Enemies;
using ArknightSimulator.Operator;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace ArknightSimulator.Manager
{
    public class OperatorManager : INotifyPropertyChanged
    {
        private int currentCost;          // 当前费用
        private int maxCost;              // 最大费用
        private int costUnit;             // 费用单元
        private int restDeploymentCount;  // 剩余可部署人数 

        public int CurrentCost { get => currentCost; set { currentCost = value; OnPropertyChanged(); } }
        public int MaxCost { get; set; }
        public int CostUnit { get => costUnit; set { costUnit = value; OnPropertyChanged(); } }
        public int RestDeploymentCount { get => restDeploymentCount; set { restDeploymentCount = value; OnPropertyChanged(); } }

        public List<OperatorTemplate> AvailableOperators { get; private set; }
        public ObservableCollection<OperatorTemplate> SelectedOperators { get; set; }


        public EventHandler OnCostIncrease;
        //public EventHandler OnDeploymentDecrease;


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

        public void Init(ObservableCollection<OperatorTemplate> selected, int initialCost, int maxCost, int deploymentLimit)
        {
            SelectedOperators = selected;
            CurrentCost = (initialCost >= 0) ? initialCost : 0;
            MaxCost = (maxCost >= 0) ? maxCost : 0;
            RestDeploymentCount = (deploymentLimit >= 0) ? deploymentLimit : 0;
            CostUnit = 0;
        }

        public void Update(float totalTime, int costRefresh)
        {
            if (CurrentCost < MaxCost)
            {
                int nextCostUnit = (CostUnit + 1) % costRefresh;
                if (nextCostUnit < CostUnit)
                    CurrentCost++;
                CostUnit = nextCostUnit;
                OnCostIncrease(this, null);
            }

        }


        public void GameOver()
        {
            SelectedOperators = null;
        }
    }
}
