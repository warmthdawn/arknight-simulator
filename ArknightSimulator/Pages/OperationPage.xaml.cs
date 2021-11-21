using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ArknightSimulator.Operators;
using Point = ArknightSimulator.Operations.Point;


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
        private List<Image> notOnMapImg;
        private int currentMapX;
        private int currentMapY;


        public GameManager GameManager => gameManager;
        public MapManager MapManager => mapManager;
        public OperatorManager OperatorManager => operatorManager;



        public EventHandler OnDeleteOperationPage;  // 结束作战删除本页事件
        public EventHandler OnChangeToEditPage;   // 跳转到编辑页事件

        public OperationPage(MainWindow mainWindow)
        {
            InitializeComponent();

            // 属性初始值
            this.mainWindow = mainWindow;
            gameManager = mainWindow.GameManager;
            mapManager = gameManager.MapManager;
            operatorManager = gameManager.OperatorManager;
            notOnMapImg = new List<Image>();



            // 初始化控件
            notOnMapItems.DataContext = operatorManager.NotOnMapOperators;
            imgMap.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(mapManager.CurrentOperation.Picture)));
            lblCost.DataContext = operatorManager.CurrentCost;
            lblDeployment.DataContext = operatorManager.RestDeploymentCount;
            pgbCost.DataContext = operatorManager.CostUnit;

            ImageBrush brush2 = new ImageBrush();
            brush2.ImageSource = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/continue.png")));
            btnStart.Background = brush2;


            // 先隐藏加速和暂停按钮
            btnPauseOrContinue.Visibility = Visibility.Hidden;
            btnSpeed.Visibility = Visibility.Hidden;

            // 添加事件
            operatorManager.OnOperatorEnable += OperatorEnable;
            mapManager.OnEnemyAppearing += EnemyAppearing;
            mapManager.OnEnemyMoving += EnemyMoving;
            mapManager.OnLose += Lose;
            mapManager.OnEnemyRemove += EnemyRemove;
        }

        

        // 判断费用是否足够放置
        private void OperatorEnable(object sender, OperatorEventArgs e)
        {
            Image img = notOnMapImg.Find(i => ((OperatorTemplate)i.DataContext).Id == e.OperatorTemplate.Id);
            if (e.CostEnough)
            {
                img.Opacity = 1;
            }
            else
            {
                img.Opacity = 0.3;
            }
        }


        // 敌人出现
        private void EnemyAppearing(object sender, EnemyEventArgs e)
        {
            Image enemyImg = new Image();
            EnemyTemplate et = mapManager.CurrentOperation.AvailableEnemies.Find(en => en.Id == e.EnemyMovement.Enemy.TemplateId);
            enemyImg.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(et.Picture)));
            enemyImg.Width = 200;
            enemyImg.Height = 200;
            var pos = mapManager.CurrentOperation.GetPosition(e.EnemyMovement.Enemy.Position);

            enemyImg.Margin = new Thickness(
                pos.X - 0.5 * enemyImg.Width,
                pos.Y - enemyImg.Height,
                grid.ActualWidth - pos.X - 0.5 * enemyImg.Width,
                grid.ActualHeight - pos.Y
                );


            grid.RegisterName("enemy" + e.EnemyMovement.Enemy.InstanceId.ToString(), enemyImg);
            grid.Children.Add(enemyImg);
        }


        // 敌人移动
        private void EnemyMoving(object sender, EnemyEventArgs e)
        {
            Image enemyImg = (Image)grid.FindName("enemy" + e.EnemyMovement.Enemy.InstanceId.ToString());
            var pos = mapManager.CurrentOperation.GetPosition(e.EnemyMovement.Enemy.Position);
            
            // 转向
            if (e.IsTurningDirection && e.Direction == Directions.Left)
            {
                ScaleTransform scaleTransform = new ScaleTransform();
                scaleTransform.ScaleX = -1;
                scaleTransform.CenterX = 0.5 * enemyImg.Width;
                enemyImg.RenderTransform = scaleTransform;
            }
            else if(e.IsTurningDirection && e.Direction == Directions.Right)
            {
                ScaleTransform scaleTransform = new ScaleTransform();
                scaleTransform.ScaleX = 1;
                scaleTransform.CenterX = 0.5 * enemyImg.Width;
                enemyImg.RenderTransform = scaleTransform;
            }


            enemyImg.Margin = new Thickness(
                pos.X - 0.5 * enemyImg.Width,
                pos.Y - enemyImg.Height,
                grid.ActualWidth - pos.X - 0.5 * enemyImg.Width,
                grid.ActualHeight - pos.Y
                );
        }

        private void Lose(object sender, EventArgs e)
        {
            gameManager.Pause();
            MessageBoxResult result = MessageBox.Show("作战失败", "作战失败", MessageBoxButton.OK);
            if (result == MessageBoxResult.OK)
            {
                gameManager.GameOver();
                OnChangeToEditPage(this, null);
                OnDeleteOperationPage(this, null);
            }
        }

        private void EnemyRemove(object sender, EnemyEventArgs e)
        {
            Image enemyImg = (Image)grid.FindName("enemy" + e.EnemyMovement.Enemy.InstanceId.ToString());
            grid.Children.Remove(enemyImg);
        }

        // 已入队干员初始化后加载图片
        private void OpSelectedItem_Initialized(object sender, EventArgs e)
        {
            Image img = (Image)sender;
            img.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(((OperatorTemplate)img.DataContext).Picture)));
            double width = notOnMapItems.Width / operatorManager.NotOnMapOperators.Count;
            img.Width = (width < notOnMapItems.Height) ? width : notOnMapItems.Height;

            notOnMapImg.Add(img);
        }

        // 游戏开始
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

        // 暂停
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

        // 调整游戏速度
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

        // 退出
        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            gameManager.Pause();
            MessageBoxResult result = MessageBox.Show("确定要退出作战吗？", "退出作战", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                gameManager.GameOver();
                OnChangeToEditPage(this, null);
                OnDeleteOperationPage(this, null);
            }
            else
                gameManager.Continue();
        }

        // 重选干员
        private void BtnReChoose_Click(object sender, RoutedEventArgs e)
        {

        }

        // 选中干员准备拖动
        private void OpSelectedItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (currentDragOperatorImg == null && currentDragOperator == null && e.LeftButton == MouseButtonState.Pressed)
            {
                currentDragOperator = (OperatorTemplate)((Image)sender).DataContext;

                if (gameManager.IsGoingOn == false || currentDragOperator.Status.Cost[currentDragOperator.EliteLevel] > operatorManager.CurrentCost)
                {
                    currentDragOperator = null;
                    return;
                }

                btnPauseOrContinue.IsEnabled = false;
                gameManager.Pause();   // 部署时先暂停

                currentDragOperatorImg = new Image();
                currentDragOperatorImg.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(currentDragOperator.ModelPicture)));
                currentDragOperatorImg.Width = 200;
                currentDragOperatorImg.Height = 200;
                currentDragOperatorImg.Margin = new Thickness(
                    e.GetPosition(grid).X - 0.5 * currentDragOperatorImg.Width,
                    e.GetPosition(grid).Y - currentDragOperatorImg.Width,
                    grid.ActualWidth - e.GetPosition(grid).X - 0.5 * currentDragOperatorImg.Width,
                    grid.ActualHeight - e.GetPosition(grid).Y);

                grid.RegisterName(currentDragOperator.Id.Replace(" ", "_"), currentDragOperatorImg);
                grid.Children.Add(currentDragOperatorImg);
            }
        }

        // 拖动干员
        private void OpSelectedItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentDragOperatorImg != null && e.LeftButton == MouseButtonState.Pressed)
            {
                currentDragOperatorImg.Margin = new Thickness(
                    e.GetPosition(grid).X - 0.5 * currentDragOperatorImg.Width,
                    e.GetPosition(grid).Y - currentDragOperatorImg.Width,
                    grid.ActualWidth - e.GetPosition(grid).X - 0.5 * currentDragOperatorImg.Width,
                    grid.ActualHeight - e.GetPosition(grid).Y);
            }
        }

        // 放下干员
        private void OpSelectedItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (currentDragOperatorImg != null && e.LeftButton != MouseButtonState.Pressed)
            {
                // 坐标转换，将鼠标位置转换为地图格子坐标
                // mapManager.CurrentOperation.XXXXXXXX   // 坐标转换
                // currentMapX = ...     // 地图格子坐标
                // currentMapY = ...
                // currentGridX = ...;  // 实际窗口中的坐标
                // currentGridY = ...;

                var mapPoint = mapManager.CurrentOperation.GetPosition(new Point(e.GetPosition(grid).X, e.GetPosition(grid).Y));
                currentMapX = (int)mapPoint.X;
                currentMapY = (int)mapPoint.Y;
                var pos = mapManager.CurrentOperation.GetPosition(new Point(currentMapX + 0.5, currentMapY + 0.5));


                currentDragOperatorImg.Margin = new Thickness(
                    pos.X - 0.5 * currentDragOperatorImg.Width,
                    pos.Y - currentDragOperatorImg.Width,
                    grid.ActualWidth - pos.X - 0.5 * currentDragOperatorImg.Width,
                    grid.ActualHeight - pos.Y);

                canvasDirection.Margin = new Thickness(
                    currentDragOperatorImg.Margin.Left - 100,
                    currentDragOperatorImg.Margin.Top - 100,
                    currentDragOperatorImg.Margin.Right - 100,
                    currentDragOperatorImg.Margin.Bottom - 100);
                canvasDirection.Visibility = Visibility.Visible;

                currentDragOperatorImg = null;
            }
        }

        // 调整方向
        private void BtnTurnUp_Click(object sender, RoutedEventArgs e)
        {
            operatorManager.Deploying(currentDragOperator, Directions.Up, currentMapX, currentMapY);

            Image img = notOnMapImg.Find(i => ((OperatorTemplate)i.DataContext).Id == currentDragOperator.Id);
            notOnMapImg.Remove(img);
            currentDragOperator = null;
            canvasDirection.Visibility = Visibility.Hidden;
            gameManager.Continue();   // 部署后继续游戏
            btnPauseOrContinue.IsEnabled = true;
        }
        private void BtnTurnDown_Click(object sender, RoutedEventArgs e)
        {
            operatorManager.Deploying(currentDragOperator, Directions.Down, currentMapX, currentMapY);

            Image img = notOnMapImg.Find(i => ((OperatorTemplate)i.DataContext).Id == currentDragOperator.Id);
            notOnMapImg.Remove(img);
            currentDragOperator = null;
            canvasDirection.Visibility = Visibility.Hidden;
            gameManager.Continue();
            btnPauseOrContinue.IsEnabled = true;
        }
        private void BtnTurnLeft_Click(object sender, RoutedEventArgs e)
        {
            Image currentImg = (Image)grid.FindName(currentDragOperator.Id.Replace(" ", "_"));

            // 干员模型图片转向左边（原来默认的模型转向为右边）
            ScaleTransform scaleTransform = new ScaleTransform();
            scaleTransform.ScaleX = -1;
            scaleTransform.CenterX = 0.5 * currentImg.Width;
            currentImg.RenderTransform = scaleTransform;


            operatorManager.Deploying(currentDragOperator, Directions.Left, currentMapX, currentMapY);

            Image img = notOnMapImg.Find(i => ((OperatorTemplate)i.DataContext).Id == currentDragOperator.Id);
            notOnMapImg.Remove(img);
            currentDragOperator = null;
            canvasDirection.Visibility = Visibility.Hidden;
            gameManager.Continue();
            btnPauseOrContinue.IsEnabled = true;
        }
        private void BtnTurnRight_Click(object sender, RoutedEventArgs e)
        {
            operatorManager.Deploying(currentDragOperator, Directions.Right, currentMapX, currentMapY);

            Image img = notOnMapImg.Find(i => ((OperatorTemplate)i.DataContext).Id == currentDragOperator.Id);
            notOnMapImg.Remove(img);
            currentDragOperator = null;
            canvasDirection.Visibility = Visibility.Hidden;
            gameManager.Continue();
            btnPauseOrContinue.IsEnabled = true;
        }
        private void BtnTurnCancel_Click(object sender, RoutedEventArgs e)
        {
            currentDragOperator = null;
            canvasDirection.Visibility = Visibility.Hidden;
            gameManager.Continue();
            btnPauseOrContinue.IsEnabled = true;
        }
    }
}
