using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneVsOneServer.GameLogic.InGame
{
    class GameTimer
    {
        Stopwatch gameTimer;

        public float Time {
            get { return (float)(gameTimer.ElapsedMilliseconds / 1000.0f); }
        }

        public int TimeSecond
        {
            get { return (int)(gameTimer.ElapsedMilliseconds / 1000); }
        }
        public float deltaTime
        {
            get { return ((gameTimer.ElapsedMilliseconds - previousUpdateTime)) / 1000.0f; }
        }

        long previousUpdateTime;

        public void Start() {
            this.gameTimer = Stopwatch.StartNew();
        }

        public void Update() {
            previousUpdateTime = gameTimer.ElapsedMilliseconds;
        }
    }
}
