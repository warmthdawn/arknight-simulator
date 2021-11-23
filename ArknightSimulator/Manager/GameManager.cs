using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ArknightSimulator.Manager
{
    public class GameManager
    {
        private DispatcherTimer timer;     // 计时器
        private int speed;          // 速度
        private bool isGoingOn;     // 游戏正在进行中
        private bool timerTicking;  // 计时器事件进行中
        private int refresh;        // 每秒刷新次数
        private int interval = 50;  // 刷新间隔
        private float totalTime;    // 总时间

        public int Speed => this.speed;
        public bool IsGoingOn => this.isGoingOn;
        public bool TimerTicking => this.timerTicking;
        public int Refresh => this.refresh;
        public float TotalTime => this.totalTime;


        public MapManager MapManager { get; private set; } = new MapManager();
        public OperatorManager OperatorManager { get; private set; } = new OperatorManager();



        public EventHandler OnLose;      // 作战失败事件
        public EventHandler OnWin;       // 作战完成事件


        public GameManager(DispatcherTimer timer)
        {
            this.timer = timer;
            this.timer.Interval = TimeSpan.FromMilliseconds(interval);
            refresh = 1000 / interval;
            this.timer.Tick += Timer_Tick;

        }

        public void Init()
        {
            speed = 1;
            isGoingOn = false;
            timerTicking = false;
            totalTime = 0;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if(timerTicking == true)   // 防止线程冲突
            {
                Pause();
                while (timerTicking == true) ;
                Continue();
            }

            timerTicking = true;


            for (int i = 0; i < speed; i++)
            {
                Update();
            }


            timerTicking = false;


            CheckResult();  // 检查结算
        }

        public void Update()
        {
            totalTime += interval * 1.0f / 1000;
            OperatorManager.Update(refresh);
            MapManager.Update(TotalTime, Refresh);
        }

        public void StartGame()
        {
            isGoingOn = true;
            timer.Start();
        }
        public void Pause()
        {
            if(isGoingOn == true)
            {
                while (timerTicking == true) ;

                timer.Stop();
                isGoingOn = false;
            }
        }
        public void Continue()
        {
            if (isGoingOn == false)
            {
                while (timerTicking == true) ;

                isGoingOn = true;
                timer.Start();
            }
        }
        public void SpeedChange(int speed)
        {
            if (speed == 1 || speed == 2)
            {
                while (timerTicking == true) ;

                this.speed = speed;
            }

        }
        public void GameOver()
        {
            timer.Stop();
            isGoingOn = false;
            OperatorManager.GameOver();
            MapManager.GameOver();
        }

        public void CheckResult()
        {
            if (MapManager.CurrentHomeLife <= 0)
                Lose();
            else if (MapManager.CurrentEnemyCount >= MapManager.EnemyTotalCount)
                Win();

        }

        public void Lose()
        {
            Pause();
            OnLose(this, null);
        }
        public void Win()
        {
            Pause();
            OnWin(this, null);
        }

    }
}
