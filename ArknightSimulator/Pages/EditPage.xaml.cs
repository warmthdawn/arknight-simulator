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
using ArknightSimulator.Operator;
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;
using ArknightSimulator.Manager;

namespace ArknightSimulator.Pages
{
    /// <summary>
    /// EditPage.xaml 的交互逻辑
    /// </summary>
    public partial class EditPage : Page
    {
        private MainWindow mainWindow;
        public ObservableCollection<OperatorTemplate> Operators { get; set; }
        public ObservableCollection<OperatorTemplate> Operators2 { get; set; }
        private ObservableCollection<OperatorTemplate> selected;
        public EditPage(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            //this.mainWindow.GameManager.OperatorManager.AvailableOperators

            // mook
            ObservableCollection<OperatorTemplate> operators = new ObservableCollection<OperatorTemplate>()
            {
                new OperatorTemplate() {Name="耀骑士临光", Picture="../Image/operator.png",Level=90},
                new OperatorTemplate() {Name="op2", Picture="../Image/operator.png"},
                new OperatorTemplate() {Name="op3", Picture="../Image/operator.png"},
                new OperatorTemplate() {Name="op4", Picture="../Image/operator.png"},
                new OperatorTemplate() {Name="op5", Picture="../Image/operator.png"},
                new OperatorTemplate() {Name="op6", Picture="../Image/operator.png"},
                new OperatorTemplate() {Name="op7", Picture="../Image/operator.png"},
                new OperatorTemplate() {Name="op8", Picture="../Image/operator.png"}
            };
            ObservableCollection<OperatorTemplate> operators2 = new ObservableCollection<OperatorTemplate>()
            {
                new OperatorTemplate() {Name="op1", Picture="../Image/operator.png"},
                new OperatorTemplate() {Name="op2", Picture="../Image/operator.png"},
                new OperatorTemplate() {Name="op3", Picture="../Image/operator.png"}
            };
            selected = new ObservableCollection<OperatorTemplate>();
            Operators = operators;
            Operators2 = operators2;
            operatorItems.DataContext = Operators;
            //operatorItems2.DataContext = Operators2;
            selectedItems.DataContext = selected;
            //RefreshOperatorSettingTab(operators);
        }

        private void RefreshOperatorSettingTab(ObservableCollection<OperatorTemplate> operators = null)
        {
            if (operators != null)
                this.Operators = operators;
            operatorItems.ItemsSource = this.Operators;
        }

        private void btnOperationSelected_Click(object sender, RoutedEventArgs e)
        {
            var mapManager = mainWindow.GameManager.MapManager;
            bool success = mapManager.LoadOperation(this.txtOperation.Text.Trim());
            if(success)
            {
                try
                {
                    var path = System.IO.Path.GetFullPath(mapManager.CurrentOperation.Picture);
                    var bmp = new BitmapImage(new Uri(path));
                    imgOperation.Source = bmp;
                    

                }
                catch (Exception ex)
                {
                    MessageBox.Show("无法获取地图图片" + ex.ToString());
                }
            }


        }


        private void btnStartOperation_Click(object sender, RoutedEventArgs e)
        {
            // 创建
        }

        private void OpSettingItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 转换为自定义类型
            OpSettingItem item = (OpSettingItem)sender;
            detialBoard.DataContext = item.DataContext;
            
            selectionMask.Width = item.ActualWidth + 10;
            selectionMask.Height = item.ActualHeight + 10;
            Point point = item.TranslatePoint(new Point(), globalCanvas); // 计算相对位置
            if (Canvas.GetLeft(selectionMask) == point.X - 10 - 5 && selectionMask.Visibility == Visibility.Visible) // 同一位置再次触发，取消选中
            {
                Canvas.SetLeft(selectionMask, 0); // 重置
                Canvas.SetTop(selectionMask, 0); // 重置
                selectionMask.Visibility = Visibility.Hidden;
                if (detialBoard.ActualWidth > 0)
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

        private void OpSettingItem_MouseRigthButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpSettingItem item = (OpSettingItem)sender;
            OperatorTemplate opt = (OperatorTemplate)item.DataContext;
            selected.Add(opt);
        }

        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            BeginStoryboard((Storyboard)Resources["OffSide"]);
        }

        private void OnRemoveOperator(object sender, RoutedEventArgs e)
        {
            OperatorTemplate op = (OperatorTemplate)((Image)sender).DataContext;
            selected.Remove(op);
        }
    }
}
