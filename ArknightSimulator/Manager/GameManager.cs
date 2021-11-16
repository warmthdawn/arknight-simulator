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
        private int costRefresh;    // 每秒费用刷新次数
        private int interval = 50;  // 刷新间隔

        public int Speed => this.speed;
        public bool IsGoingOn => this.isGoingOn;
        public bool TimerTicking => this.timerTicking;
        public int CostRefresh => this.costRefresh;



        public MapManager MapManager { get; private set; } = new MapManager();
        public OperatorManager OperatorManager { get; private set; } = new OperatorManager();


        public EventHandler OnCostIncrease;
        //public EventHandler OnDeploymentDecrease;

        public GameManager(DispatcherTimer timer)
        {
            this.timer = timer;
            this.timer.Interval = TimeSpan.FromMilliseconds(interval);
            costRefresh = 1000 / interval;
            this.timer.Tick += Timer_Tick;
            speed = 1;
            isGoingOn = false;
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


            // 处理
            int nextCostUnit = (OperatorManager.CostUnit + 1 * speed) % costRefresh;
            if (nextCostUnit < OperatorManager.CostUnit)
                OperatorManager.CurrentCost++;
            OperatorManager.CostUnit = nextCostUnit;
            for(int i=0;i<speed;i++)
            {
                OnCostIncrease(this, null);
            }





            timerTicking = false;
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
        public void Quit()
        {
            timer.Stop();
            isGoingOn = false;
        }


        
    }
}
