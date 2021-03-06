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
using ArknightSimulator.Utils;
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
        private Image currentOperatorImg;
        private Operator currentOp;
        private List<Image> notOnMapImg;
        private List<Label> notOnMapLbl;
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
            notOnMapLbl = new List<Label>();
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
            gameManager.OnUpdateOperatorProgressBar += UpdateOperatorProgressBar;
            gameManager.OnUpdateEnemyProgressBar += UpdateEnemyProgressBar;
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
            gameManager.OnUpdateOperatorProgressBar -= UpdateOperatorProgressBar;
            gameManager.OnUpdateEnemyProgressBar -= UpdateEnemyProgressBar;
            operatorManager.OnOperatorEnable -= OperatorEnable;
            mapManager.OnEnemyAppearing -= EnemyAppearing;
            mapManager.OnEnemyMoving -= EnemyMoving;
            mapManager.OnEnemyRemove -= EnemyRemove;

        }

        private void HandleRange(int mapX, int mapY, int[][] range, Action<Polygon> act, Directions dir = Directions.Right, bool reset = false, bool? self = null)
        {
            // 先将所有网格设为不可见
            if (reset)
            {
                for (int x = 0; x < mapManager.CurrentOperation.MapWidth; x++)
                {
                    for (int y = 0; y < mapManager.CurrentOperation.MapHeight; y++)
                    {
                        blocks[x][y].Visibility = Visibility.Hidden;
                    }
                }
            }
            if (act == null)
                return;
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
                        if (self != null)
                            blocks[mapX][mapY].Visibility = self.Value ? Visibility.Visible : Visibility.Hidden;
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
                        switch (dir) // TODO
                        {
                            case Directions.Up:
                                mapi = mapY - dj;
                                mapj = mapX + di;
                                break;
                            case Directions.Down:
                                mapi = mapY + dj;
                                mapj = mapX - di;
                                break;
                            case Directions.Left:
                                mapi = mapY - di;
                                mapj = mapX - dj;
                                break;
                            case Directions.Right:
                                mapi = mapY + di;
                                mapj = mapX + dj;
                                break;
                        }
                        if (mapi >= 0 && mapi <= mapManager.CurrentOperation.MapHeight - 1
                            && mapj >= 0 && mapj <= mapManager.CurrentOperation.MapWidth - 1)
                            act(blocks[mapj][mapi]);
                        //blocks[mapj][mapi].Visibility = Visibility.Visible; // block的第一维是列
                    }
                }
            }
        }

        private void Block_MouseLeave(object sender, MouseEventArgs e)
        {
            if (currentDragOperatorImg == null)
                return;
            Polygon block = (Polygon)sender;
            block.Opacity -= 0.2;
            string coordinate = block.Name.Substring(5);
            //currentDragOperatorImg.IsHitTestVisible = true;

            //var mapPoint = mapManager.CurrentOperation.GetPosition(new Point(e.GetPosition(grid).X, e.GetPosition(grid).Y));
            int mapX = int.Parse(coordinate.Split("_")[0]);
            int mapY = int.Parse(coordinate.Split("_")[1]);
            OperatorTemplate opt = currentDragOperator;
            int[][] range = opt.Status.Range[opt.EliteLevel];
            HandleRange(mapX, mapY, range, p =>
            {
                var color = (SolidColorBrush)p.Fill;
                p.Fill = new SolidColorBrush(Colors.Green);
                if (color.Color.Equals(Colors.Orange)) // 原先不可见点，继续不可见
                    p.Visibility = Visibility.Hidden;
            });
        }

        private void Block_MouseEnter(object sender, MouseEventArgs e)
        {
            if (currentDragOperatorImg == null)
                return;
            Polygon block = (Polygon)sender;
            block.Opacity += 0.2;
            string coordinate = block.Name.Substring(5);
            //currentDragOperatorImg.IsHitTestVisible = true;

            //var mapPoint = mapManager.CurrentOperation.GetPosition(new Point(e.GetPosition(grid).X, e.GetPosition(grid).Y));
            int mapX = int.Parse(coordinate.Split("_")[0]);
            int mapY = int.Parse(coordinate.Split("_")[1]);
            OperatorTemplate opt = currentDragOperator;
            int[][] range = opt.Status.Range[opt.EliteLevel];
            HandleRange(mapX, mapY, range, p =>
            {
                if (p.Visibility == Visibility.Visible)
                    p.Fill = new SolidColorBrush(Colors.Yellow);
                else
                {
                    p.Visibility = Visibility.Visible;
                    p.Fill = new SolidColorBrush(Colors.Orange);
                }
            });
        }



        // 判断费用是否足够放置
        private void OperatorEnable(object sender, OperatorEventArgs e)
        {
            Image img = notOnMapImg.Find(i => ((OperatorTemplate)i.DataContext).Id == e.OperatorTemplate.Id);
            Label lbl = notOnMapLbl.Find(i => ((OperatorTemplate)i.DataContext).Id == e.OperatorTemplate.Id);
            if (e.Index == 1 && img != null)
            {
                if (e.CostEnough)
                {
                    img.Opacity = 1;
                }
                else
                {
                    img.Opacity = 0.3;
                }
            }
            else if (e.Index == 2 && lbl != null)
            {
                if (e.TimeEnough)
                {
                    lbl.Visibility = Visibility.Hidden;
                }
                else
                {
                    lbl.Visibility = Visibility.Visible;
                }
            }
        }


        // 敌人出现
        private void EnemyAppearing(object sender, EnemyEventArgs e)
        {
            Image enemyImg = new Image();
            EnemyTemplate et = e.EnemyMovement.Enemy.Template;
            // ./Image/Enemy/B1_Attack.gif
            //enemyImg.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(et.Picture)));
            ImageBehavior.SetAnimatedSource(enemyImg, new BitmapImage(new Uri(System.IO.Path.GetFullPath(et.MovePicture))));
            //ImageBehavior.Set
            enemyImg.Width = 400;
            enemyImg.Height = 400;

            e.EnemyMovement.Enemy.DieEvent += EnemyDie;
            e.EnemyMovement.Enemy.AttackEvent += EnemyAttack;

            var pos = mapManager.CurrentOperation.GetPosition(e.EnemyMovement.Enemy.Position);
            enemyImg.DataContext = e.EnemyMovement.Enemy;
            ImageBehavior.AddAnimationCompletedHandler(enemyImg, AnimationLoaded);
            //ImageBehavior.SetRepeatBehavior(enemyImg, new RepeatBehavior(1));
            enemyImg.Margin = new Thickness(
                    pos.X - 0.5 * enemyImg.Width,
                    pos.Y - 0.7 * enemyImg.Height,
                    grid.ActualWidth - pos.X - 0.5 * enemyImg.Width,
                    grid.ActualHeight - pos.Y - 0.3 * enemyImg.Height);

            enemyImg.IsHitTestVisible = false;

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
                    pos.Y - 0.7 * enemyImg.Height,
                    grid.ActualWidth - pos.X - 0.5 * enemyImg.Width,
                    grid.ActualHeight - pos.Y - 0.3 * enemyImg.Height);

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
            Enemy enemy;
            if (e.EnemyMovement != null)
                enemy = e.EnemyMovement.Enemy;
            else
                enemy = e.Enemy;

            Image enemyImg = (Image)grid.FindName("enemy" + enemy.InstanceId.ToString());
            grid.UnregisterName("enemy" + enemy.InstanceId.ToString());
            grid.Children.Remove(enemyImg);

            ProgressBar bar = (ProgressBar)grid.FindName("enemylifeBar" + enemy.InstanceId);
            grid.UnregisterName("enemylifeBar" + enemy.InstanceId);
            grid.Children.Remove(bar);

        }
        private void OperatorRemove(object sender, OperatorEventArgs e)
        {
            if (e.Operator != null)
                currentDragOperator = new OperatorTemplate(e.Operator.Template);

            Image currentImg = (Image)grid.FindName(currentDragOperator.Id.Replace(" ", "_"));
            grid.Children.Remove(currentImg);
            grid.UnregisterName(currentImg.Name);
            Canvas currentHitArea = (Canvas)grid.FindName("hit_" + currentDragOperator.Id.Replace(" ", "_"));
            if (currentHitArea != null)
            {
                currentHitArea.MouseLeftButtonDown -= CurrentOperator_MouseLeftButtonDown;
                grid.Children.Remove(currentHitArea);
                grid.UnregisterName(currentHitArea.Name);
            }




            ProgressBar lifebar = (ProgressBar)grid.FindName("lifeBar" + currentDragOperator.Id.Replace(" ", "_"));
            ProgressBar skillbar = (ProgressBar)grid.FindName("skillBar" + currentDragOperator.Id.Replace(" ", "_"));
            if (lifebar != null)
            {
                grid.UnregisterName("lifeBar" + currentDragOperator.Id.Replace(" ", "_"));
                grid.Children.Remove(lifebar);
            }
            if (skillbar != null)
            {
                grid.UnregisterName("skillBar" + currentDragOperator.Id.Replace(" ", "_"));
                grid.Children.Remove(skillbar);
            }




            currentDragOperatorImg = null;
            currentDragOperator = null;

            canvasDirection.Visibility = Visibility.Hidden;
            canvasSkillOrWithdraw.Visibility = Visibility.Hidden;
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

        // 显示干员攻击范围
        private void SelectMapOperator(Operator op, OperatorTemplate opt)
        {
            int[][] range = op.Status.Range[opt.EliteLevel];
            currentOp = op;
            HandleRange(op.MapX, op.MapY, range, p =>
            {
                p.Visibility = Visibility.Visible;
                p.Fill = new SolidColorBrush(Colors.Orange);
            }, op.Direction, true, true);
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
        private void LblReDeployTime_Initialized(object sender, EventArgs e)
        {
            Label lblReDeployTime = (Label)sender;
            double width = notOnMapItems.Width / operatorManager.NotOnMapOperators.Count;
            lblReDeployTime.Width = (width < notOnMapItems.Height) ? width : notOnMapItems.Height;

            notOnMapLbl.Add(lblReDeployTime);
        }
        private void LblCost_Initialized(object sender, EventArgs e)
        {
            Label lblCost = (Label)sender;
            lblCost.Content = "C " + ((OperatorTemplate)lblCost.DataContext).Status.Cost[((OperatorTemplate)lblCost.DataContext).EliteLevel];
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

        // 重选干员 TODO
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

                ImageBehavior.SetAnimatedSource(currentDragOperatorImg, new BitmapImage(new Uri(System.IO.Path.GetFullPath(currentDragOperator.AttackPicture))));
                ImageBehavior.SetAutoStart(currentDragOperatorImg, false);
                ImageBehavior.AddAnimationLoadedHandler(currentDragOperatorImg, AnimationLoaded);
                //controller.Play();

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

        // 回调函数，解决动画加载导致动画丢失问题
        private void AnimationLoaded(object sender, EventArgs e)
        {
            Image img = (Image)sender;
            if (img.DataContext is Operator)
            {
                Operator op = (Operator)img.DataContext;
                if (op != null && op.IsChanged)
                {
                    OperatorAttack(op);
                    op.IsChanged = false;
                }
            }
            if (img.DataContext is Enemy)
            {
                Enemy enemy = (Enemy)img.DataContext;
                if (enemy != null && enemy.IsChanged)
                {
                    EnemyAttack(enemy);
                    enemy.IsChanged = false;
                }
            }
            return;
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
                //currentDragOperatorImg.IsHitTestVisible = true;

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
                ImageBehavior.SetRepeatBehavior(currentDragOperatorImg, new RepeatBehavior(1)); // 设置每次攻击动画执行一次
                //ImageAnimationController controller = ImageBehavior.GetAnimationController(currentDragOperatorImg);
                //controller.Play(); // 加载动画，避免第一次攻击失效
                //ImageBehavior.SetAnimatedSource(currentDragOperatorImg, new BitmapImage(new Uri(System.IO.Path.GetFullPath(currentDragOperator.AttackPicture))));
                currentDragOperatorImg.Margin = new Thickness(
                    pos.X - 0.5 * currentDragOperatorImg.Width,
                    pos.Y - 0.7 * currentDragOperatorImg.Height,
                    grid.ActualWidth - pos.X - 0.5 * currentDragOperatorImg.Width,
                    grid.ActualHeight - pos.Y - 0.3 * currentDragOperatorImg.Height);

                //                 canvasDirection.Margin = new Thickness(
                //                     currentDragOperatorImg.Margin.Left - 100,
                //                     currentDragOperatorImg.Margin.Top - 100,
                //                     currentDragOperatorImg.Margin.Right - 100,
                //                     currentDragOperatorImg.Margin.Bottom - 100);

                // 添加选中区域
                Canvas hitArea = new Canvas();
                hitArea.Background = new SolidColorBrush(Colors.Transparent);
                //hitArea.Opacity = 0;
                //hitArea.DataContext = currentDragOperatorImg.DataContext;
                hitArea.Margin = new Thickness(
                    pos.X - 50,
                    pos.Y - 50,
                    grid.ActualWidth - pos.X - 50,
                    grid.ActualHeight - pos.Y - 50);

                grid.RegisterName("hit_" + currentDragOperator.Id.Replace(" ", "_"), hitArea);
                hitArea.Name = "hit_" + currentDragOperator.Id.Replace(" ", "_");
                Panel.SetZIndex(hitArea, -1);
                grid.Children.Add(hitArea);


                canvasDirection.Margin = new Thickness(
                    pos.X - 200,
                    pos.Y - 200,
                    grid.ActualWidth - pos.X - 200,
                    grid.ActualHeight - pos.Y - 200);
                canvasDirection.Visibility = Visibility.Visible;

                hitArea.MouseLeftButtonDown += CurrentOperator_MouseLeftButtonDown;
                Block_MouseLeave(sender, e); // 触发离开事件，恢复网格
                // currentDragOperatorImg.MouseLeftButtonDown += CurrentOperatorImg_MouseLeftButtonDown;
                currentOperatorImg = currentDragOperatorImg;
                currentDragOperatorImg = null;
                gridCanvas.Visibility = Visibility.Hidden;

            }
        }

        // 调整方向
        private void BtnTurnUp_Click(object sender, RoutedEventArgs e)
        {
            Operator op = operatorManager.Deploying(currentDragOperator, Directions.Up, currentMapX, currentMapY, currentDeploymentType);
            op.AttackEvent += OperatorAttack;
            op.DieEvent += OperatorDie;
            AddOperatorProgressBar(op);
            currentOperatorImg.DataContext = op;
            ImageBehavior.AddAnimationCompletedHandler(currentOperatorImg, AnimationLoaded);
            currentOperatorImg = null;

            Image img = notOnMapImg.Find(i => ((OperatorTemplate)i.DataContext).Id == currentDragOperator.Id);
            Label lbl = notOnMapLbl.Find(i => ((OperatorTemplate)i.DataContext).Id == currentDragOperator.Id);
            notOnMapImg.Remove(img);
            notOnMapLbl.Remove(lbl);
            currentDragOperator = null;
            canvasDirection.Visibility = Visibility.Hidden;
            gameManager.Continue();   // 部署后继续游戏
            btnPauseOrContinue.IsEnabled = true;
        }
        private void BtnTurnDown_Click(object sender, RoutedEventArgs e)
        {
            Operator op = operatorManager.Deploying(currentDragOperator, Directions.Down, currentMapX, currentMapY, currentDeploymentType);
            op.AttackEvent += OperatorAttack;
            op.DieEvent += OperatorDie;
            AddOperatorProgressBar(op);
            currentOperatorImg.DataContext = op;
            ImageBehavior.AddAnimationCompletedHandler(currentOperatorImg, AnimationLoaded);
            currentOperatorImg = null;

            Image img = notOnMapImg.Find(i => ((OperatorTemplate)i.DataContext).Id == currentDragOperator.Id);
            Label lbl = notOnMapLbl.Find(i => ((OperatorTemplate)i.DataContext).Id == currentDragOperator.Id);
            notOnMapImg.Remove(img);
            notOnMapLbl.Remove(lbl);
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


            Operator op = operatorManager.Deploying(currentDragOperator, Directions.Left, currentMapX, currentMapY, currentDeploymentType);
            op.AttackEvent += OperatorAttack;
            op.DieEvent += OperatorDie;
            AddOperatorProgressBar(op);
            currentOperatorImg.DataContext = op;
            ImageBehavior.AddAnimationCompletedHandler(currentOperatorImg, AnimationLoaded);
            currentOperatorImg = null;

            Image img = notOnMapImg.Find(i => ((OperatorTemplate)i.DataContext).Id == currentDragOperator.Id);
            Label lbl = notOnMapLbl.Find(i => ((OperatorTemplate)i.DataContext).Id == currentDragOperator.Id);
            notOnMapImg.Remove(img);
            notOnMapLbl.Remove(lbl);
            currentDragOperator = null;
            canvasDirection.Visibility = Visibility.Hidden;
            gameManager.Continue();
            btnPauseOrContinue.IsEnabled = true;
        }
        private void BtnTurnRight_Click(object sender, RoutedEventArgs e)
        {
            Operator op = operatorManager.Deploying(currentDragOperator, Directions.Right, currentMapX, currentMapY, currentDeploymentType);
            op.AttackEvent += OperatorAttack;
            op.DieEvent += OperatorDie;
            AddOperatorProgressBar(op);
            currentOperatorImg.DataContext = op;
            ImageBehavior.AddAnimationCompletedHandler(currentOperatorImg, AnimationLoaded);
            currentOperatorImg = null;

            Image img = notOnMapImg.Find(i => ((OperatorTemplate)i.DataContext).Id == currentDragOperator.Id);
            Label lbl = notOnMapLbl.Find(i => ((OperatorTemplate)i.DataContext).Id == currentDragOperator.Id);
            notOnMapImg.Remove(img);
            notOnMapLbl.Remove(lbl);
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
            //Binding binding = new Binding();     // 由于status由装饰器控制，无法绑定，只能实时更新
            //binding.Source = op.Status;
            //binding.Path = new PropertyPath("CurrentLife");
            //lifeBar.SetBinding(ProgressBar.ValueProperty, binding);
            //lifeBar.Maximum = op.Status.MaxLife;
            //binding = new Binding();
            //binding.Source = op.Status;
            //binding.Path = new PropertyPath("SkillPoint");
            //skillBar.SetBinding(ProgressBar.ValueProperty, binding);
            lifeBar.Value = op.Status.CurrentLife;
            lifeBar.Maximum = op.Status.MaxLife;
            skillBar.Value = op.Status.SkillPoint;
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
            lifeBar.IsHitTestVisible = false;
            skillBar.IsHitTestVisible = false;
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
            //Binding binding = new Binding();  // 由于status由装饰器控制，无法绑定，只能实时更新
            //binding.Source = enemy.Status;
            //binding.Path = new PropertyPath("CurrentLife");
            //lifeBar.SetBinding(ProgressBar.ValueProperty, binding);

            lifeBar.Value = enemy.Status.CurrentLife;
            lifeBar.Maximum = enemy.Status.MaxLife;
            lifeBar.Background = new SolidColorBrush(Colors.Black);
            lifeBar.Foreground = new SolidColorBrush(Colors.DarkOrange);
            lifeBar.BorderThickness = new Thickness(0);
            lifeBar.IsHitTestVisible = false;
            var pos = mapManager.CurrentOperation.GetPosition(enemy.Position);
            //Image img = (Image)grid.FindName("enemy" + enemy.InstanceId);
            lifeBar.Margin = new Thickness(
                pos.X - 50,
                pos.Y + 55,
                grid.ActualWidth - pos.X - 50,
                grid.ActualHeight - pos.Y - 60
                );
            grid.RegisterName("enemylifeBar" + enemy.InstanceId, lifeBar);
            grid.Children.Add(lifeBar);
        }

        // 干员血条和技能条更新
        private void UpdateOperatorProgressBar(object sender, OperatorEventArgs e)
        {
            Operator op = e.Operator;
            ProgressBar lifebar = (ProgressBar)grid.FindName("lifeBar" + op.Template.Id.Replace(" ", "_"));
            ProgressBar skillBar = (ProgressBar)grid.FindName("skillBar" + op.Template.Id.Replace(" ", "_"));
            lifebar.Value = op.Status.CurrentLife;
            skillBar.Value = op.Status.SkillPoint;

        }

        // 敌人血条更新
        private void UpdateEnemyProgressBar(object sender, EnemyEventArgs e)
        {
            //Image img = (Image)grid.FindName("enemy" + enemy.InstanceId);
            Enemy enemy = e.EnemyMovement.Enemy;
            ProgressBar bar = (ProgressBar)grid.FindName("enemylifeBar" + enemy.InstanceId);
            bar.Value = enemy.Status.CurrentLife;
            if (enemy.Status.CurrentLife >= enemy.Status.MaxLife)
                bar.Visibility = Visibility.Hidden;
            else
                bar.Visibility = Visibility;

            var pos = mapManager.CurrentOperation.GetPosition(enemy.Position);
            //Image img = (Image)grid.FindName("enemy" + enemy.InstanceId);
            bar.Margin = new Thickness(
                pos.X - 50,
                pos.Y + 55,
                grid.ActualWidth - pos.X - 50,
                grid.ActualHeight - pos.Y - 60
                );
        }

        // 选中场上干员，可选择撤退或使用技能
        private void CurrentOperator_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (gameManager.IsGoingOn == false)
                return;
            if (canvasSkillOrWithdraw.Visibility == Visibility)
                return;

            if (currentDragOperatorImg == null && currentDragOperator == null)
            {
                btnPauseOrContinue.IsEnabled = false;
                gameManager.Pause();   // 选中干员时先暂停

                string id = ((Canvas)sender).Name.Substring(4).Replace("_", " ");
                OperatorTemplate opt = operatorManager.SelectedOperators.Find(o => o.Id == id);
                Operator op = operatorManager.OnMapOperators.Find(o => o.Template.Id == opt.Id);

                currentDragOperator = opt;

                if (op.Skill == null)
                {
                    btnSkill.Visibility = Visibility.Hidden;
                    canvasSkillOff.Visibility = Visibility.Hidden;
                    canvasSkillOn.Visibility = Visibility.Hidden;
                }
                else
                {
                    btnSkill.Visibility = Visibility.Visible;
                    string skillname = opt.SkillNames[opt.SkillChooseId - 1];
                    ImageBrush brush = new ImageBrush();
                    brush.ImageSource = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/Skills/" + skillname + ".png")));
                    btnSkill.Background = brush;
                    if(op.Skill.IsUsing == false && op.Status.SkillPoint < op.Skill.Cost)
                    {
                        canvasSkillOff.Visibility = Visibility.Visible;
                        lblSkillPoint.Content = op.Status.SkillPoint;
                        lblSkillCost.Content = op.Skill.Cost;
                    }
                    else
                    {
                        canvasSkillOff.Visibility = Visibility.Hidden;
                    }
                    if(op.Skill.IsUsing == true)
                    {
                        canvasSkillOn.Visibility = Visibility.Visible;
                        lblSkillCurrentTime.Content = op.Skill.CurrentTime;
                        lblSkillTime.Content = op.Skill.Time;
                    }
                    else
                    {
                        canvasSkillOn.Visibility = Visibility.Hidden;
                    }
                }

                // 网格可视化
                OnSelectMapOperator(op, opt);
                gridCanvas.Visibility = Visibility.Visible;

                imgMap.MouseDown += ImgMap_MouseDown;

                var pos = mapManager.CurrentOperation.GetPosition(new Point(op.MapX + 0.5, op.MapY + 0.5));
                canvasSkillOrWithdraw.Margin = new Thickness(
                    pos.X - 150,
                    pos.Y - 150,
                    grid.ActualWidth - pos.X - 250,
                    grid.ActualHeight - pos.Y - 250);
                canvasSkillOrWithdraw.Visibility = Visibility.Visible;

            }


        }

        // 取消选中场上干员
        private void ImgMap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            imgMap.MouseDown -= ImgMap_MouseDown;
            canvasSkillOrWithdraw.Visibility = Visibility.Hidden;
            gridCanvas.Visibility = Visibility.Hidden;
            currentDragOperator = null;
            HandleRange(currentOp.MapX, currentMapY, currentOp.Status.Range[currentOp.Template.EliteLevel], p =>
            {
                p.Fill = new SolidColorBrush(Colors.Green);
                p.Visibility = Visibility.Hidden;
            }, currentOp.Direction);
            gameManager.Continue();
            currentOp = null;
            btnPauseOrContinue.IsEnabled = true;

        }
        // 撤退干员
        private void BtnWithdraw_Click(object sender, RoutedEventArgs e)
        {
            Operator op = operatorManager.OnMapOperators.Find(o => o.Template.Id == currentDragOperator.Id);
            op.AttackEvent -= OperatorAttack;
            op.DieEvent -= OperatorDie;
            imgMap.MouseDown -= ImgMap_MouseDown;
            operatorManager.Withdrawing(op, false);

            OperatorRemove(this, new OperatorEventArgs(op));
        }
        // 使用技能
        private void BtnSkill_Click(object sender, RoutedEventArgs e)
        {
            imgMap.MouseDown -= ImgMap_MouseDown;
            canvasSkillOrWithdraw.Visibility = Visibility.Hidden;
            gridCanvas.Visibility = Visibility.Hidden;

            var op = operatorManager.OnMapOperators.Find(o => o.Template.Id == currentDragOperator.Id);
            operatorManager.SkillStart(op);

            currentDragOperator = null;
            gameManager.Continue();
            btnPauseOrContinue.IsEnabled = true;
        }

        // 干员攻击
        private void OperatorAttack(Operator op)
        {
            Image opImg = (Image)grid.FindName(op.Template.Id.Replace(" ", "_"));
            if (opImg == null)
                return;
            var controller = ImageBehavior.GetAnimationController(opImg);
            Duration? duration = ImageBehavior.GetAnimationDuration(opImg);
            if (!duration.HasValue || !(duration.Value.TimeSpan.TotalSeconds == op.Status.AttackTime / gameManager.Speed - 0.2))
            {
                op.IsChanged = true;
                ImageBehavior.SetAnimationDuration(opImg, TimeSpan.FromSeconds(op.Status.AttackTime / gameManager.Speed - 0.2));
            }
            controller.GotoFrame(0);
            controller.Play();

        }

        private void EnemyAttack(Enemy enemy)
        {
            Image enemyImg = (Image)grid.FindName("enemy" + enemy.InstanceId.ToString());
            if (enemyImg == null)
                return;
            ImageBehavior.SetAnimatedSource(enemyImg, new BitmapImage(new Uri(System.IO.Path.GetFullPath(enemy.Template.AttackPicture))));
            var controller = ImageBehavior.GetAnimationController(enemyImg);
            //Duration? duration = ImageBehavior.GetAnimationDuration(enemyImg);
            ImageBehavior.SetRepeatBehavior(enemyImg, new RepeatBehavior(1));
            ImageBehavior.SetAnimationDuration(enemyImg, TimeSpan.FromSeconds(1.0 / gameManager.Speed));
            //             if (!duration.HasValue || !(duration.Value.TimeSpan.TotalSeconds == 1.0 / gameManager.Speed))
            //             {
            //                 enemy.IsChanged = true;
            //                 ImageBehavior.SetAnimationDuration(enemyImg, TimeSpan.FromSeconds(1.0 / gameManager.Speed));
            //             }
            controller.GotoFrame(0);
            controller.Play();
        }

        // 干员死亡事件
        private void OperatorDie(Operator op)
        {
            OperatorRemove(this, new OperatorEventArgs(op));

            op.AttackEvent -= OperatorAttack;
            op.DieEvent -= OperatorDie;

            operatorManager.Withdrawing(op, true);
        }

        // 敌人死亡事件
        private void EnemyDie(Enemy enemy)
        {
            EnemyRemove(this, new EnemyEventArgs(enemy));
            enemy.AttackEvent -= EnemyAttack;
            enemy.DieEvent -= EnemyDie;

            // 从mapManager中删除
            mapManager.CurrentEnemyCount++;
            mapManager.EnemiesAppear.RemoveAll((em) => em.Enemy.InstanceId == enemy.InstanceId);
        }


    }
}
