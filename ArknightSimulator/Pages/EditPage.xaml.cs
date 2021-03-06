using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArknightSimulator.UserControls;
using ArknightSimulator.Operators;
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;
using ArknightSimulator.Manager;
using ArknightSimulator.Enemies;

namespace ArknightSimulator.Pages
{
    /// <summary>
    /// EditPage.xaml 的交互逻辑
    /// </summary>
    public partial class EditPage : Page   // 干员和关卡编辑界面
    {
        private MainWindow mainWindow;
        private GameManager gameManager;
        private MapManager mapManager;
        private OperatorManager operatorManager;

        private ObservableCollection<OperatorTemplate> operators;
        private ObservableCollection<OperatorTemplate> selected;


        public EventHandler OnChangeToOperationPage;   //  跳转到作战界面事件

        public EditPage(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            gameManager = mainWindow.GameManager;
            mapManager = gameManager.MapManager;
            operatorManager = gameManager.OperatorManager;

            // mook
            ObservableCollection<OperatorTemplate> operators = new ObservableCollection<OperatorTemplate>(operatorManager.AvailableOperators);
            selected = new ObservableCollection<OperatorTemplate>();
            this.operators = operators;
            operatorItems.DataContext = this.operators;
            selectedItems.DataContext = selected;
            selectedItems2.DataContext = selected;
            //RefreshOperatorSettingTab(operators);


        }

        private void RefreshOperatorSettingTab(ObservableCollection<OperatorTemplate> operators = null)
        {
            if (operators != null)
                this.operators = operators;
            operatorItems.ItemsSource = this.operators;
        }

        // 选中关卡
        private void btnOperationSelected_Click(object sender, RoutedEventArgs e)
        {
            //var mapManager = mainWindow.GameManager.MapManager;
            bool success = mapManager.LoadOperation(this.txtOperation.Text.Trim());
            if(success)
            {
                try
                {
                    var path = System.IO.Path.GetFullPath(mapManager.CurrentOperation.Picture);
                    var bmp = new BitmapImage(new Uri(path));
                    imgOperation.Source = bmp;

                    enemyItems.DataContext = mapManager.CurrentOperation.AvailableEnemies;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("无法获取地图图片" + ex.ToString());
                }
            }
        }


        // 开始作战
        private void btnStartOperation_Click(object sender, RoutedEventArgs e)
        {
            if (mapManager.CurrentOperation == null)
                return;

            // 创建作战
            gameManager.Init();
            mapManager.Init();
            operatorManager.Init(new List<OperatorTemplate>(selected), mapManager.CurrentOperation.InitialCost, mapManager.CurrentOperation.MaxCost, mapManager.CurrentOperation.DeploymentLimit);
            OnChangeToOperationPage(this, null);
        }

        // 左键弹出干员信息
        private void OpSettingItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 转换为自定义类型
            OpSettingItem item = (OpSettingItem)sender;
            detailBoard.DataContext = item.DataContext;
            
            selectionMask.Width = item.ActualWidth + 10;
            selectionMask.Height = item.ActualHeight + 10;
            Point point = item.TranslatePoint(new Point(), globalCanvas); // 计算相对位置
            if (Canvas.GetLeft(selectionMask) == point.X - 10 - 5 && selectionMask.Visibility == Visibility.Visible) // 同一位置再次触发，取消选中
            {
                Canvas.SetLeft(selectionMask, 0); // 重置
                Canvas.SetTop(selectionMask, 0); // 重置
                selectionMask.Visibility = Visibility.Hidden;
                if (detailBoard.ActualWidth > 0)
                    BeginStoryboard((Storyboard)Resources["OffSide"]);
                return;
            }
            selectionMask.Visibility = Visibility.Visible;
            BeginStoryboard((Storyboard)Resources["ShowSide"]);
            if (Canvas.GetLeft(selectionMask) == 0) // 首次触发，不激活动画
            {
                Canvas.SetLeft(selectionMask, point.X - 10 - 5); // X - Margin - additional width
                Canvas.SetTop(selectionMask, point.Y  -10 - 5);
                
                BeginStoryboard((Storyboard)Resources["ShowSide"]);
                return;
            }
            DoubleAnimation maskMoveX = new DoubleAnimation(Canvas.GetLeft(selectionMask), point.X - 10 - 5, new Duration(TimeSpan.FromSeconds(0.1)));
            DoubleAnimation maskMoveY = new DoubleAnimation(Canvas.GetTop(selectionMask), point.Y - 10 - 5, new Duration(TimeSpan.FromSeconds(0.1)));
            selectionMask.BeginAnimation(Canvas.LeftProperty, maskMoveX);
            selectionMask.BeginAnimation(Canvas.TopProperty, maskMoveY);
            //Storyboard Mave

        }

        // 右键入队
        private void OpSettingItem_MouseRigthButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpSettingItem item = (OpSettingItem)sender;
            OperatorTemplate opt = (OperatorTemplate)item.DataContext;
            if (!selected.Contains(opt))
                selected.Add(opt);
        }

        // 隐藏属性板
        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            BeginStoryboard((Storyboard)Resources["OffSide"]);
        }

        // 已入队干员初始化后加载图片
        private void OpSelectedItem_Initialized(object sender, EventArgs e)
        {
            Image img = (Image)sender;
            img.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(((OperatorTemplate)img.DataContext).Picture)));
        }
        
        // 地图确定后敌人加载图片
        private void EnemyItems_Initialized(object sender, EventArgs e)
        {
            Image img = (Image)sender;
            img.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(((EnemyTemplate)img.DataContext).Picture)));
        }

        // 左键弹出已入队干员信息
        private void OpSelectedItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 转换为自定义类型
            Image item = (Image)sender;
            detailBoard.DataContext = item.DataContext;

            selectionMask.Width = 100;
            selectionMask.Height = 170;
            Point point = item.TranslatePoint(new Point(), globalCanvas); // 计算相对位置
            if (Canvas.GetLeft(selectionMask) == point.X - 10 - 5 && selectionMask.Visibility == Visibility.Visible) // 同一位置再次触发，取消选中
            {
                Canvas.SetLeft(selectionMask, 0); // 重置
                Canvas.SetTop(selectionMask, 0); // 重置
                selectionMask.Visibility = Visibility.Hidden;
                if (detailBoard.ActualWidth > 0)
                    BeginStoryboard((Storyboard)Resources["OffSide"]);
                return;
            }
            selectionMask.Visibility = Visibility.Visible;
            BeginStoryboard((Storyboard)Resources["ShowSide"]);
            if (Canvas.GetLeft(selectionMask) == 0) // 首次触发，不激活动画
            {
                Canvas.SetLeft(selectionMask, point.X - 10 - 5); // X - Margin - additional width
                Canvas.SetTop(selectionMask, point.Y - 10 - 5);

                BeginStoryboard((Storyboard)Resources["ShowSide"]);
                return;
            }
            DoubleAnimation maskMoveX = new DoubleAnimation(Canvas.GetLeft(selectionMask), point.X - 10 - 5, new Duration(TimeSpan.FromSeconds(0.1)));
            DoubleAnimation maskMoveY = new DoubleAnimation(Canvas.GetTop(selectionMask), point.Y - 10 - 5, new Duration(TimeSpan.FromSeconds(0.1)));
            selectionMask.BeginAnimation(Canvas.LeftProperty, maskMoveX);
            selectionMask.BeginAnimation(Canvas.TopProperty, maskMoveY);
            //Storyboard Mave
        }

        // 右键取消编队
        private void OpSelectedItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            OperatorTemplate opt = (OperatorTemplate)((Image)sender).DataContext;
            selected.Remove(opt);
        }


        // 精英等级减小
        private void BtnEliteLevelDown_Click(object sender, RoutedEventArgs e)
        {
            OperatorTemplate opt = (OperatorTemplate)detailBoard.DataContext;
            if (opt.EliteLevel - 1 >= 0)
            {
                opt.Level = 1;
                if (opt.SkillNames.Length == 0)
                    opt.SkillChooseId = 0;
                else
                    opt.SkillChooseId = 1;
                opt.EliteLevel--;
                opt.ResetStatus();
            }
        }

        // 精英等级增大
        private void BtnEliteLevelUp_Click(object sender, RoutedEventArgs e)
        {
            OperatorTemplate opt = (OperatorTemplate)detailBoard.DataContext;
            if (opt.EliteLevel + 1 <= OperatorTemplate.LevelLimit[opt.Rare-1].Length-1)
            {
                opt.Level = 1;
                if (opt.SkillNames.Length == 0)
                    opt.SkillChooseId = 0;
                else
                    opt.SkillChooseId = 1;
                opt.EliteLevel++;
                opt.ResetStatus();
            }
        }

        // 等级减小
        private void BtnLevelDown_Click(object sender, RoutedEventArgs e)
        {
            OperatorTemplate opt = (OperatorTemplate)detailBoard.DataContext;
            if (opt.Level - 1 >= 1)
            {
                opt.Level--;
                opt.ResetStatus();
            }
        }

        // 等级增大
        private void BtnLevelUp_Click(object sender, RoutedEventArgs e)
        {
            OperatorTemplate opt = (OperatorTemplate)detailBoard.DataContext;
            if (opt.Level + 1 <= OperatorTemplate.LevelLimit[opt.Rare - 1][opt.EliteLevel])
            {
                opt.Level++;
                opt.ResetStatus();
            }
        }

        // 信赖减小
        private void BtnBeliefDown_Click(object sender, RoutedEventArgs e)
        {
            OperatorTemplate opt = (OperatorTemplate)detailBoard.DataContext;
            if (opt.Belief - 1 >= 0)
            {
                opt.Belief--;
                opt.ResetStatus();
            }
        }

        // 信赖增大
        private void BtnBeliefUp_Click(object sender, RoutedEventArgs e)
        {
            OperatorTemplate opt = (OperatorTemplate)detailBoard.DataContext;
            if (opt.Belief + 1 <= 100)
            {
                opt.Belief++;
                opt.ResetStatus();
            }
        }

        // 潜能减小
        private void BtnPotentialDown_Click(object sender, RoutedEventArgs e)
        {
            OperatorTemplate opt = (OperatorTemplate)detailBoard.DataContext;
            if (opt.Potential - 1 >= 1)
            {
                opt.Potential--;
                opt.ResetStatus();
            }
        }

        // 潜能增大
        private void BtnPotentialUp_Click(object sender, RoutedEventArgs e)
        {
            OperatorTemplate opt = (OperatorTemplate)detailBoard.DataContext;
            if (opt.Potential + 1 <= 6)
            {
                opt.Potential++;
                opt.ResetStatus();
            }
        }

        // 技能选择减小
        private void BtnSkillChooseIdDown_Click(object sender, RoutedEventArgs e)
        {
            OperatorTemplate opt = (OperatorTemplate)detailBoard.DataContext;
            if (opt.SkillNames.Length == 0)
                return;
            if (opt.SkillChooseId - 1 > 0)
            {
                opt.SkillChooseId--;
            }
        }

        // 技能选择增大
        private void BtnSkillChooseIdUp_Click(object sender, RoutedEventArgs e)
        {
            OperatorTemplate opt = (OperatorTemplate)detailBoard.DataContext;
            if (opt.SkillChooseId + 1 <= opt.SkillNames.Length && opt.SkillChooseId + 1 <= opt.EliteLevel + 1)
            {
                opt.SkillChooseId++;
            }
        }

        // 技能等级减小
        private void BtnSkillLevelDown_Click(object sender, RoutedEventArgs e)
        {
            OperatorTemplate opt = (OperatorTemplate)detailBoard.DataContext;
            if (opt.SkillNames.Length == 0)
                return;
            if (opt.SkillLevel - 1 > 0)
            {
                opt.SkillLevel--;
            }
        }

        // 技能选择增大
        private void BtnSkillLevelUp_Click(object sender, RoutedEventArgs e)
        {
            OperatorTemplate opt = (OperatorTemplate)detailBoard.DataContext;
            if (opt.SkillLevel + 1 <= opt.SkillMaxLevel)
            {
                opt.SkillLevel++;
            }
        }

    }
}
