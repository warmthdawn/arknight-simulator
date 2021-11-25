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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArknightSimulator.Enemies;
using ArknightSimulator.EventHandlers;
using ArknightSimulator.Manager;
using ArknightSimulator.Operations;
using ArknightSimulator.Operators;
using WpfAnimatedGif;
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
        private DeploymentType currentDeploymentType;


        public GameManager GameManager => gameManager;
        public MapManager MapManager => mapManager;
        public OperatorManager OperatorManager => operatorManager;
        // Grid
        private List<List<Polygon>> blocks;


        public EventHandler OnDeleteOperationPage;  // 结束作战删除本页事件
        public EventHandler OnChangeToEditPage;   // 跳转到编辑页事件
        public Action<DeploymentType> OnPlaceOperator;   // 部署干员事件
        public Action<Operator, OperatorTemplate> OnSelectMapOperator;   // 选中场上干员事件
        public OperatorEventHandler OnOperatorRemove;  // 撤退干员事件

        public OperationPage(MainWindow mainWindow)
        {
            InitializeComponent();

            // 属性初始值
            this.mainWindow = mainWindow;
            gameManager = mainWindow.GameManager;
            mapManager = gameManager.MapManager;
            operatorManager = gameManager.OperatorManager;
            notOnMapImg = new List<Image>();
            blocks = new List<List<Polygon>>();

            // 初始化控件
            notOnMapItems.DataContext = operatorManager.NotOnMapOperators;
            imgMap.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(mapManager.CurrentOperation.Picture)));

            gridCanvas.DataContext = this;
            for (int x = 0; x < mapManager.CurrentOperation.MapWidth; x++)
            {
                List<Polygon> row = new List<Polygon>();
                for (int y = 0; y < mapManager.CurrentOperation.MapHeight; y++)
                {
                    Polygon block = new Polygon();
                    Point point = new Point();
                    point = mapManager.CurrentOperation.GetPosition(new Point(x, y + 1)); // 左下角
                    block.Points.Add(new System.Windows.Point(point.X, point.Y));
                    point = mapManager.CurrentOperation.GetPosition(new Point(x, y)); // 左上角
                    block.Points.Add(new System.Windows.Point(point.X, point.Y));
                    point = mapManager.CurrentOperation.GetPosition(new Point(x + 1, y)); // 右上角
                    block.Points.Add(new System.Windows.Point(point.X, point.Y));
                    point = mapManager.CurrentOperation.GetPosition(new Point(x + 1, y + 1)); // 右下角
                    block.Points.Add(new System.Windows.Point(point.X, point.Y));

                    block.Stroke = Brushes.Black;

                    block.Fill = new SolidColorBrush(Colors.Green);
                    block.Opacity = 0.3;
                    block.IsHitTestVisible = true;
                    block.MouseLeftButtonUp += OpSelectedItem_MouseLeftButtonUp;
                    block.MouseEnter += Block_MouseEnter;
                    block.MouseLeave += Block_MouseLeave;
                    block.Name = $"block{x}_{y}";
                    //grid.RegisterName($"block{x}_{y}", block);
                    gridCanvas.Children.Add(block);

                    row.Add(block);
                }
                blocks.Add(row);
            }

            ImageBrush brush2 = new ImageBrush();
            brush2.ImageSource = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/continue.png")));
            btnStart.Background = brush2;


            // 先隐藏加速和暂停按钮
            btnPauseOrContinue.Visibility = Visibility.Hidden;
            btnSpeed.Visibility = Visibility.Hidden;

            // 添加事件
            gameManager.OnLose += Lose;
            gameManager.OnWin += Win;
            operatorManager.OnOperatorEnable += OperatorEnable;
            mapManager.OnEnemyAppearing += EnemyAppearing;
            mapManager.OnEnemyMoving += EnemyMoving;
            mapManager.OnEnemyRemove += EnemyRemove;
            OnPlaceOperator += PlaceOperator;
            OnSelectMapOperator += SelectMapOperator;
            OnOperatorRemove += OperatorRemove;
        }

        // 删除事件
        public void DeleteEvent()
        {
            // 每次作战结束需要删除与Manager关联的事件
            gameManager.OnLose -= Lose;
            gameManager.OnWin -= Win;
            operatorManager.OnOperatorEnable -= OperatorEnable;
            mapManager.OnEnemyAppearing -= EnemyAppearing;
            mapManager.OnEnemyMoving -= EnemyMoving;
            mapManager.OnEnemyRemove -= EnemyRemove;
            //OnPlaceOperator -= PlaceOperator;
        }

        private void Block_MouseLeave(object sender, MouseEventArgs e)
        {
            Polygon block = (Polygon)sender;
            block.Opacity -= 0.2;
        }

        private void Block_MouseEnter(object sender, MouseEventArgs e)
        {
            Polygon block = (Polygon)sender;
            block.Opacity += 0.2;
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
            EnemyTemplate et = e.EnemyMovement.Enemy.Template;
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
            Panel.SetZIndex(enemyImg, -1);
            grid.Children.Add(enemyImg);

            AddEnemyProgressBar(e.EnemyMovement.Enemy);
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
            else if (e.IsTurningDirection && e.Direction == Directions.Right)
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

            UpdateEnemyProgressBar(e.EnemyMovement.Enemy);
        }

        private void Lose(object sender, EventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("作战失败", "作战失败", MessageBoxButton.OK);

            gameManager.GameOver();
            DeleteEvent();
            OnChangeToEditPage(this, null);
            OnDeleteOperationPage(this, null);

        }
        private void Win(object sender, EventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("作战完成", "作战完成", MessageBoxButton.OK);

            gameManager.GameOver();
            DeleteEvent();
            OnChangeToEditPage(this, null);
            OnDeleteOperationPage(this, null);

        }
        private void EnemyRemove(object sender, EnemyEventArgs e)
        {
            Image enemyImg = (Image)grid.FindName("enemy" + e.EnemyMovement.Enemy.InstanceId.ToString());
            grid.UnregisterName("enemy" + e.EnemyMovement.Enemy.InstanceId.ToString());
            grid.Children.Remove(enemyImg);
        }
        private void OperatorRemove(object sender, OperatorEventArgs e)
        {
            if (e.Operator != null)
                currentDragOperator = new OperatorTemplate(e.Operator.Template);

            Image currentImg = (Image)grid.FindName(currentDragOperator.Id.Replace(" ", "_"));
            grid.Children.Remove(currentImg);
            grid.UnregisterName(currentDragOperator.Id.Replace(" ", "_"));
            currentDragOperatorImg = null;
            currentDragOperator = null;

            canvasDirection.Visibility = Visibility.Hidden;
            gridCanvas.Visibility = Visibility.Hidden;
            gameManager.Continue();
            btnPauseOrContinue.IsEnabled = true;
        }

        // 根据干员部署类型更新可放置网格
        private void PlaceOperator(DeploymentType type) // TODO: 待完善
        {
            PointType place1 = PointType.None; // 可放置网格类型
            PointType place2 = PointType.None;
            switch (type)
            {
                case DeploymentType.All:
                    place1 = PointType.Land;
                    place2 = PointType.HighLand;
                    break;
                case DeploymentType.Land:
                    place1 = PointType.Land;
                    break;
                case DeploymentType.HighLand:
                    place1 = PointType.HighLand;
                    break;
            }

            for (int x = 0; x < mapManager.CurrentOperation.MapWidth; x++)
            {
                for (int y = 0; y < mapManager.CurrentOperation.MapHeight; y++)
                {
                    if (place1 != PointType.None)
                    {
                        if (mapManager.Map[y][x] == place1)
                            blocks[x][y].Visibility = Visibility.Visible;
                        else
                            blocks[x][y].Visibility = Visibility.Hidden;
                    }
                    if (place2 != PointType.None)
                    {
                        if (mapManager.Map[y][x] == place2)
                            blocks[x][y].Visibility = Visibility.Visible;
                        else
                            blocks[x][y].Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        // 根据干员攻击范围更新网格
        private void SelectMapOperator(Operator op, OperatorTemplate opt)
        {
            // 先将所有网格设为不可见
            for (int x = 0; x < mapManager.CurrentOperation.MapWidth; x++)
            {
                for (int y = 0; y < mapManager.CurrentOperation.MapHeight; y++)
                {
                    blocks[x][y].Visibility = Visibility.Hidden;
                }
            }

            int[][] range = op.Status.Range[opt.EliteLevel];
            int selfi = 0;
            int selfj = 0;
            // 寻找自身点
            for (int i = 0; i < range.Length; i++)
            {
                int j = 0;
                for (; j < range[i].Length; j++)
                {
                    if (range[i][j] == 0)
                    {
                        selfi = i;
                        selfj = j;
                        blocks[op.MapX][op.MapY].Visibility = Visibility.Visible;
                        break;
                    }
                }
                if (j < range[i].Length)
                    break;
            }

            // 确定攻击范围
            for (int i = 0; i < range.Length; i++)
            {
                for (int j = 0; j < range[i].Length; j++)
                {
                    if (range[i][j] == 1)
                    {
                        int di = i - selfi;  // 攻击范围的某一格与自身行距离
                        int dj = j - selfj;  // 列距离
                        int mapi = 0;   // 攻击范围在地图的行
                        int mapj = 0;   // 列
                        switch (op.Direction)
                        {
                            case Directions.Up:
                                mapi = op.MapY - dj;
                                mapj = op.MapX + di;
                                break;
                            case Directions.Down:
                                mapi = op.MapY + dj;
                                mapj = op.MapX - di;
                                break;
                            case Directions.Left:
                                mapi = op.MapY - di;
                                mapj = op.MapX - dj;
                                break;
                            case Directions.Right:
                                mapi = op.MapY + di;
                                mapj = op.MapX + dj;
                                break;
                        }

                        blocks[mapj][mapi].Visibility = Visibility.Visible; // block的第一维是列
                    }
                }
            }
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
                DeleteEvent();
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
        private void OpSelectedItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (currentDragOperatorImg == null && currentDragOperator == null)
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

                //ImageSource cropped = new CroppedBitmap(new BitmapImage(new Uri(System.IO.Path.GetFullPath(currentDragOperator.AttackPicture))), new Int32Rect(160, 200, 320, 320));
                //currentDragOperatorImg.Clip = new RectangleGeometry(new Rect(160, 200, 320, 320));
                //ImageBehavior.SetRepeatBehavior(currentDragOperatorImg, new RepeatBehavior(1));
                ImageBehavior.SetAnimatedSource(currentDragOperatorImg, new BitmapImage(new Uri(System.IO.Path.GetFullPath(currentDragOperator.AttackPicture))));
                //ImageBehavior.GetAnimationController(currentDragOperatorImg).Pause();
                ImageBehavior.SetAutoStart(currentDragOperatorImg, false);


                //ImageBehavior.SetAnimationDuration(currentDragOperatorImg, TimeSpan.FromSeconds(3));
                //currentDragOperatorImg.Source = cropped;

                //currentDragOperatorImg.ClipToBounds = true;
                //ImageBehavior.SetRepeatBehavior(currentDragOperatorImg, RepeatBehavior.Forever);
                //currentDragOperatorImg.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(currentDragOperator.ModelPicture)));
                currentDragOperatorImg.Width = 400;
                currentDragOperatorImg.Height = 400;
                currentDragOperatorImg.Margin = new Thickness(
                    e.GetPosition(grid).X - 0.8 * currentDragOperatorImg.Width,
                    e.GetPosition(grid).Y - 0.2 * currentDragOperatorImg.Width,
                    grid.ActualWidth - e.GetPosition(grid).X - 0.5 * currentDragOperatorImg.Width,
                    grid.ActualHeight - e.GetPosition(grid).Y - 0.2 * currentDragOperatorImg.Width);

                grid.RegisterName(currentDragOperator.Id.Replace(" ", "_"), currentDragOperatorImg);
                currentDragOperatorImg.Name = currentDragOperator.Id.Replace(" ", "_");
                Panel.SetZIndex(currentDragOperatorImg, -1);
                grid.Children.Add(currentDragOperatorImg);


                // 网格可视化
                OnPlaceOperator(currentDragOperator.DeploymentType);
                gridCanvas.Visibility = Visibility.Visible;
                currentDragOperatorImg.IsHitTestVisible = false;
            }
        }

        // 拖动干员
        private void OpSelectedItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentDragOperatorImg != null && e.LeftButton == MouseButtonState.Pressed)
            {
                currentDragOperatorImg.Margin = new Thickness(
                    e.GetPosition(grid).X - 0.5 * currentDragOperatorImg.Width,
                    e.GetPosition(grid).Y - 0.8 * currentDragOperatorImg.Width,
                    grid.ActualWidth - e.GetPosition(grid).X - 0.5 * currentDragOperatorImg.Width,
                    grid.ActualHeight - e.GetPosition(grid).Y - 0.2 * currentDragOperatorImg.Width);
            }
        }

        // 放下干员
        private void OpSelectedItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (currentDragOperatorImg != null)
            {
                // 若没有放在格子上
                if (sender.GetType() != typeof(Polygon))
                {
                    OnOperatorRemove(this, new OperatorEventArgs(currentDragOperator));
                    return;
                }


                // 坐标转换，将鼠标位置转换为地图格子坐标
                Polygon block = (Polygon)sender;
                string coordinate = block.Name.Substring(5);
                currentDragOperatorImg.IsHitTestVisible = true;

                //var mapPoint = mapManager.CurrentOperation.GetPosition(new Point(e.GetPosition(grid).X, e.GetPosition(grid).Y));
                currentMapX = int.Parse(coordinate.Split("_")[0]);
                currentMapY = int.Parse(coordinate.Split("_")[1]);
                switch (mapManager.Map[currentMapY][currentMapX])
                {
                    case PointType.Land: currentDeploymentType = DeploymentType.Land; break;
                    case PointType.HighLand: currentDeploymentType = DeploymentType.HighLand; break;
                    default: throw new Exception("地图方格类型为特殊，未定义！");
                }
                var pos = mapManager.CurrentOperation.GetPosition(new Point(currentMapX + 0.5, currentMapY + 0.5));


                currentDragOperatorImg.Margin = new Thickness(
                    pos.X - 0.5 * currentDragOperatorImg.Width,
                    pos.Y - 0.65 * currentDragOperatorImg.Height,
                    grid.ActualWidth - pos.X - 0.5 * currentDragOperatorImg.Width,
                    grid.ActualHeight - pos.Y - 0.35 * currentDragOperatorImg.Height);

                //                 canvasDirection.Margin = new Thickness(
                //                     currentDragOperatorImg.Margin.Left - 100,
                //                     currentDragOperatorImg.Margin.Top - 100,
                //                     currentDragOperatorImg.Margin.Right - 100,
                //                     currentDragOperatorImg.Margin.Bottom - 100);


                canvasDirection.Margin = new Thickness(
                    pos.X - 200,
                    pos.Y - 200,
                    grid.ActualWidth - pos.X - 200,
                    grid.ActualHeight - pos.Y - 200);
                canvasDirection.Visibility = Visibility.Visible;

                currentDragOperatorImg.MouseLeftButtonDown += CurrentOperatorImg_MouseLeftButtonDown;

                currentDragOperatorImg = null;
                gridCanvas.Visibility = Visibility.Hidden;
            }
        }

        // 调整方向
        private void BtnTurnUp_Click(object sender, RoutedEventArgs e)
        {
            Operator op = operatorManager.Deploying(currentDragOperator, Directions.Up, currentMapX, currentMapY, currentDeploymentType);
            op.AttackEvent += OperatorAttack;
            AddOperatorProgressBar(op);

            Image img = notOnMapImg.Find(i => ((OperatorTemplate)i.DataContext).Id == currentDragOperator.Id);
            notOnMapImg.Remove(img);
            currentDragOperator = null;
            canvasDirection.Visibility = Visibility.Hidden;
            gameManager.Continue();   // 部署后继续游戏
            btnPauseOrContinue.IsEnabled = true;
        }
        private void BtnTurnDown_Click(object sender, RoutedEventArgs e)
        {
            Operator op = operatorManager.Deploying(currentDragOperator, Directions.Up, currentMapX, currentMapY, currentDeploymentType);
            op.AttackEvent += OperatorAttack;
            AddOperatorProgressBar(op);

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


            Operator op = operatorManager.Deploying(currentDragOperator, Directions.Up, currentMapX, currentMapY, currentDeploymentType);
            op.AttackEvent += OperatorAttack;
            AddOperatorProgressBar(op);

            Image img = notOnMapImg.Find(i => ((OperatorTemplate)i.DataContext).Id == currentDragOperator.Id);
            notOnMapImg.Remove(img);
            currentDragOperator = null;
            canvasDirection.Visibility = Visibility.Hidden;
            gameManager.Continue();
            btnPauseOrContinue.IsEnabled = true;
        }
        private void BtnTurnRight_Click(object sender, RoutedEventArgs e)
        {
            Operator op = operatorManager.Deploying(currentDragOperator, Directions.Up, currentMapX, currentMapY, currentDeploymentType);
            op.AttackEvent += OperatorAttack;
            AddOperatorProgressBar(op);

            Image img = notOnMapImg.Find(i => ((OperatorTemplate)i.DataContext).Id == currentDragOperator.Id);
            notOnMapImg.Remove(img);
            currentDragOperator = null;
            canvasDirection.Visibility = Visibility.Hidden;
            gameManager.Continue();
            btnPauseOrContinue.IsEnabled = true;
        }
        private void BtnTurnCancel_Click(object sender, RoutedEventArgs e)
        {
            OnOperatorRemove(this, new OperatorEventArgs(currentDragOperator));
        }

        // 添加干员血条和技能条
        private void AddOperatorProgressBar(Operator op)
        {
            ProgressBar lifeBar = new ProgressBar();
            ProgressBar skillBar = new ProgressBar();
            Binding binding = new Binding();
            binding.Source = op.Status;
            binding.Path = new PropertyPath("CurrentLife");
            lifeBar.SetBinding(ProgressBar.ValueProperty, binding);
            lifeBar.Maximum = op.Status.MaxLife;
            binding = new Binding();
            binding.Source = op.Status;
            binding.Path = new PropertyPath("SkillPoint");
            skillBar.SetBinding(ProgressBar.ValueProperty, binding);
            if (op.Skill == null)
                skillBar.Maximum = 100;
            else
                skillBar.Maximum = op.Skill.Cost;

            lifeBar.Background = new SolidColorBrush(Colors.Black);
            lifeBar.Foreground = new SolidColorBrush(Colors.SkyBlue);
            skillBar.Background = new SolidColorBrush(Colors.Black);
            skillBar.Foreground = new SolidColorBrush(Colors.YellowGreen);
            lifeBar.BorderThickness = new Thickness(0);
            skillBar.BorderThickness = new Thickness(0);
            var pos = mapManager.CurrentOperation.GetPosition(new Point(op.MapX + 0.5, op.MapY + 0.5));
            //Image img = (Image)grid.FindName(currentDragOperator.Id.Replace(" ", "_"));
            //             lifeBar.Margin = new Thickness(
            //                 img.Margin.Left + 50,
            //                 img.Margin.Top + 205,
            //                 img.Margin.Right + 50,
            //                 img.Margin.Bottom - 12
            //                 );
            //             skillBar.Margin = new Thickness(
            //                 img.Margin.Left + 50,
            //                 img.Margin.Top + 212,
            //                 img.Margin.Right + 50,
            //                 img.Margin.Bottom - 19
            //                 );

            lifeBar.Margin = new Thickness(
                pos.X - 50,
                pos.Y + 50,
                grid.ActualWidth - pos.X - 50,
                grid.ActualHeight - pos.Y - 55
                );
            skillBar.Margin = new Thickness(
                pos.X - 50,
                pos.Y + 55,
                grid.ActualWidth - pos.X - 50,
                grid.ActualHeight - pos.Y - 60
                );

            grid.RegisterName("lifeBar" + currentDragOperator.Id.Replace(" ", "_"), lifeBar);
            grid.RegisterName("skillBar" + currentDragOperator.Id.Replace(" ", "_"), skillBar);
            grid.Children.Add(lifeBar);
            grid.Children.Add(skillBar);
        }



        // 添加敌人血条和技能条
        private void AddEnemyProgressBar(Enemy enemy)
        {
            ProgressBar lifeBar = new ProgressBar();
            Binding binding = new Binding();
            binding.Source = enemy.Status;
            binding.Path = new PropertyPath("CurrentLife");
            lifeBar.SetBinding(ProgressBar.ValueProperty, binding);
            lifeBar.Maximum = enemy.Status.MaxLife;
            lifeBar.Background = new SolidColorBrush(Colors.Black);
            lifeBar.Foreground = new SolidColorBrush(Colors.DarkOrange);
            lifeBar.BorderThickness = new Thickness(0);
            Image img = (Image)grid.FindName("enemy" + enemy.InstanceId);
            lifeBar.Margin = new Thickness(
                img.Margin.Left + 50,
                img.Margin.Top + 205,
                img.Margin.Right + 50,
                img.Margin.Bottom - 12
                );
            grid.RegisterName("enemylifeBar" + enemy.InstanceId, lifeBar);
            grid.Children.Add(lifeBar);
        }

        // 血条随敌人移动
        private void UpdateEnemyProgressBar(Enemy enemy)
        {
            Image img = (Image)grid.FindName("enemy" + enemy.InstanceId);
            ProgressBar bar = (ProgressBar)grid.FindName("enemylifeBar" + enemy.InstanceId);
            if (enemy.Status.CurrentLife >= enemy.Status.MaxLife)
                bar.Visibility = Visibility.Hidden;

            bar.Margin = new Thickness(
            img.Margin.Left + 50,
            img.Margin.Top + 205,
            img.Margin.Right + 50,
            img.Margin.Bottom - 12
            );
        }

        // 选中场上干员，可选择撤退或使用技能
        private void CurrentOperatorImg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (gameManager.IsGoingOn == false)
                return;
            if (canvasSkillOrWithdraw.Visibility == Visibility)
                return;

            if (currentDragOperatorImg == null && currentDragOperator == null)
            {
                btnPauseOrContinue.IsEnabled = false;
                gameManager.Pause();   // 部署时先暂停

                string id = ((Image)sender).Name.Replace("_", " ");
                OperatorTemplate opt = operatorManager.SelectedOperators.Find(o => o.Id == id);
                Operator op = operatorManager.OnMapOperators.Find(o => o.Template.Id == opt.Id);
                if (op.Skill == null)
                {
                    btnSkill.Visibility = Visibility.Hidden;
                }
                else
                {
                    btnSkill.Visibility = Visibility.Visible;
                    string skillname = opt.SkillNames[opt.SkillChooseId - 1];
                    ImageBrush brush = new ImageBrush();
                    brush.ImageSource = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/Skills/" + skillname + ".png")));
                    btnSkill.Background = brush;
                }

                // 网格可视化
                OnSelectMapOperator(op, opt);
                gridCanvas.Visibility = Visibility.Visible;

                imgMap.MouseDown += ImgMap_MouseDown;

                var pos = mapManager.CurrentOperation.GetPosition(new Point(op.MapX + 0.5, op.MapY + 0.5));
                canvasSkillOrWithdraw.Margin = new Thickness(
                    pos.X - 200,
                    pos.Y - 200,
                    grid.ActualWidth - pos.X - 200,
                    grid.ActualHeight - pos.Y - 200);
                canvasSkillOrWithdraw.Visibility = Visibility.Visible;

            }


        }

        // 取消选中场上干员
        private void ImgMap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            imgMap.MouseDown -= ImgMap_MouseDown;
            canvasSkillOrWithdraw.Visibility = Visibility.Hidden;
            gridCanvas.Visibility = Visibility.Hidden;

            gameManager.Continue();
            btnPauseOrContinue.IsEnabled = true;

        }
        // 撤退干员
        private void BtnWithdraw_Click(object sender, RoutedEventArgs e)
        {

        }
        // 使用技能
        private void BtnSkill_Click(object sender, RoutedEventArgs e)
        {

        }

        // 干员攻击
        private void OperatorAttack(Operator op)
        {
            Image opImg = (Image)grid.FindName(op.Template.Id.Replace(" ", "_"));
            if (opImg == null)
                return;
            var controller = ImageBehavior.GetAnimationController(opImg);
            ImageBehavior.SetAnimationDuration(opImg, TimeSpan.FromSeconds(op.Status.AttackTime / gameManager.Speed));
            ImageBehavior.SetRepeatBehavior(opImg, new RepeatBehavior(1));
            controller.GotoFrame(0);
            controller.Play();

            //ImageBehavior.SetRepeatBehavior(opImg, new RepeatBehavior(1));

        }

    }
}
