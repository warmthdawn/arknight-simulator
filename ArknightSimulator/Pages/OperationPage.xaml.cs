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
using ArknightSimulator.Enemies;
using ArknightSimulator.EventHandlers;
using ArknightSimulator.Manager;
using ArknightSimulator.Operator;

namespace ArknightSimulator.Pages
{
    /// <summary>
    /// OperationPage.xaml 的交互逻辑
    /// </summary>
    public partial class OperationPage : Page    // 作战界面
    {
        private MainWindow mainWindow;
        private GameManager gameManager;
        private MapManager mapManager;
        private OperatorManager operatorManager;
        private OperatorTemplate currentDragOperator;
        private Image currentDragOperatorImg;

        public GameManager GameManager => gameManager;
        public MapManager MapManager => mapManager;
        public OperatorManager OperatorManager => operatorManager;

        public EventHandler OnDeleteOperationPage;  // 结束作战删除本页事件
        public EventHandler OnChangeToEditPage;   // 跳转到编辑页事件

        public OperationPage(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            gameManager = mainWindow.GameManager;
            mapManager = gameManager.MapManager;
            operatorManager = gameManager.OperatorManager;

            // 初始化控件
            selectedItems.DataContext = operatorManager.SelectedOperators;
            imgMap.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(mapManager.CurrentOperation.Picture)));

            lblCost.DataContext = operatorManager.CurrentCost;
            lblDeployment.DataContext = operatorManager.RestDeploymentCount;

            ImageBrush brush2 = new ImageBrush();
            brush2.ImageSource = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/continue.png")));
            btnStart.Background = brush2;


            // 先隐藏加速和暂停按钮
            btnPauseOrContinue.Visibility = Visibility.Hidden;
            btnSpeed.Visibility = Visibility.Hidden;


            operatorManager.OnCostIncrease += CostIncrease;
            mapManager.OnEnemyAppearing += EnemyAppearing;
        }


        private void CostIncrease(object sender,EventArgs e)
        {
            pgbCost.Value = operatorManager.CostUnit * 100.0 / gameManager.CostRefresh;
        }
        private void EnemyAppearing(object sender, EnemyEventArgs e)
        {
            Image enemyImg = new Image();
            EnemyTemplate et = mapManager.CurrentOperation.AvailableEnemies.Find(en => en.Id == e.enemy.Enemy.TemplateId);
            enemyImg.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(et.Picture)));
            enemyImg.Width = 200;
            enemyImg.Height = 200;
            enemyImg.Margin = new Thickness(700,350,700,350);
            grid.Children.Add(enemyImg);
        }

        // 已入队干员初始化后加载图片
        private void OpSelectedItem_Initialized(object sender, EventArgs e)
        {
            Image img = (Image)sender;
            img.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(((OperatorTemplate)img.DataContext).Picture)));
            double width = selectedItems.Width / operatorManager.SelectedOperators.Count;
            img.Width = (width < selectedItems.Height) ? width : selectedItems.Height;
        }


        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            lblPause.Visibility = Visibility.Hidden;
            btnStart.Visibility = Visibility.Hidden;

            ImageBrush brush2 = new ImageBrush();
            brush2.ImageSource = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/pause.png")));
            btnPauseOrContinue.Background = brush2;
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/speed1.png")));
            btnSpeed.Background = brush;

            gameManager.StartGame();

            btnPauseOrContinue.Visibility = Visibility.Visible;
            btnSpeed.Visibility = Visibility.Visible;
        }

        private void BtnPauseOrContinue_Click(object sender, RoutedEventArgs e)
        {
            if (gameManager.IsGoingOn)
            {
                lblPause.Visibility = Visibility.Visible;

                gameManager.Pause();

                ImageBrush brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/continue.png")));
                btnPauseOrContinue.Background = brush;
            }
            else
            {
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/pause.png")));
                btnPauseOrContinue.Background = brush;

                gameManager.Continue();

                lblPause.Visibility = Visibility.Hidden;
            }
        }

        private void BtnSpeed_Click(object sender, RoutedEventArgs e)
        {
            if (gameManager.Speed == 1)
            {
                gameManager.SpeedChange(2);

                ImageBrush brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/speed2.png")));
                btnSpeed.Background = brush;
            }
            else
            {
                gameManager.SpeedChange(1);

                ImageBrush brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/speed1.png")));
                btnSpeed.Background = brush;
            }
        }

        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("确定要退出作战吗？", "退出作战", MessageBoxButton.YesNo);
            if(result == MessageBoxResult.Yes)
            {
                gameManager.Quit();
                OnChangeToEditPage(this, null);
                OnDeleteOperationPage(this, null);
            }
        }

        private void BtnReChoose_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OpSelectedItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (currentDragOperator == null && e.LeftButton == MouseButtonState.Pressed)
            {
                currentDragOperator = (OperatorTemplate)((Image)sender).DataContext;
                currentDragOperatorImg = new Image();
                currentDragOperatorImg.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(currentDragOperator.ModelPicture)));
                currentDragOperatorImg.Width = 200;
                currentDragOperatorImg.Height = 200;
                currentDragOperatorImg.Margin = new Thickness(
                    e.GetPosition(grid).X - 0.5 * currentDragOperatorImg.Width,
                    e.GetPosition(grid).Y - currentDragOperatorImg.Width,
                    grid.ActualWidth - e.GetPosition(grid).X - 0.5 * currentDragOperatorImg.Width,
                    grid.ActualHeight - e.GetPosition(grid).Y);
                grid.Children.Add(currentDragOperatorImg);
            }
        }

        private void OpSelectedItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentDragOperator != null && e.LeftButton == MouseButtonState.Pressed)
            {
                currentDragOperatorImg.Margin = new Thickness(
                    e.GetPosition(grid).X - 0.5 * currentDragOperatorImg.Width,
                    e.GetPosition(grid).Y - currentDragOperatorImg.Width,
                    grid.ActualWidth - e.GetPosition(grid).X - 0.5 * currentDragOperatorImg.Width,
                    grid.ActualHeight - e.GetPosition(grid).Y);
            }
        }

        private void OpSelectedItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (currentDragOperator != null && e.LeftButton != MouseButtonState.Pressed)
            {
                // 操作

                currentDragOperator = null;
                currentDragOperatorImg = null;
            }
        }
    }
}
