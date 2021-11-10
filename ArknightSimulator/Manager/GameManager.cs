using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;

namespace ArknightSimulator.Manager
{
    public class GameManager
    {
        private DispatcherTimer timer;
        private int speed;

        public MapManager MapManager { get; private set; } = new MapManager();

        public GameManager(DispatcherTimer timer)
        {
            this.timer = timer;
        }

        public void StartGame()
        {

        }
        public void Pause()
        {

        }
        public void Continue()
        {

        }
        public void SpeedChange(int speed)
        {

        }
        public void Quit()
        {

        }


        
    }
}
