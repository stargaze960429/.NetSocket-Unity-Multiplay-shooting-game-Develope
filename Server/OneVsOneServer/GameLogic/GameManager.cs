using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNet;
using MyNet.ServerFramework;
using System.Threading;
using System.Diagnostics;

namespace OneVsOneServer.GameLogic
{
    class GameManager : Singleton<GameManager>
    {
        private static readonly int MAX_GAME_INSTANCE = Config.MAX_CONNECTIONS / Config.USER_COUNT_PER_ONE_GAME;

        Stopwatch timer;

        Thread threadGameUpdate;

        List<Game> gameList;
        Stack<int> idleGameIndexPool;
        object gameListLock;

        private GameManager() {
            gameList = new List<Game>(MAX_GAME_INSTANCE);
            idleGameIndexPool = new Stack<int>(MAX_GAME_INSTANCE);
            gameListLock = new object();

            for (int i = 0; i < MAX_GAME_INSTANCE; i++) {
                gameList.Add(null);
                idleGameIndexPool.Push(i);
            }

            threadGameUpdate = new Thread(procedure_GameUpdate);
            threadGameUpdate.Start();
        }

        public void ReleaseGame(Game game) {
            lock (gameListLock) {
                idleGameIndexPool.Push(game.IndexInGameManager);
                gameList[game.IndexInGameManager] = null;
            }
        }

        public bool MakeGame(Stack<MatchmakeUser> users) {

            Game game = new Game();

            var iter = users.GetEnumerator();

            while (iter.MoveNext()) {
                game.Assign(iter.Current);
            }

            lock (gameListLock) {
                if (idleGameIndexPool.Count == 0) {
                    return false;
                }
                int index = idleGameIndexPool.Pop();
                gameList[index] = game;
                game.IndexInGameManager = index;
            }

            return true;
        }

        private void procedure_GameUpdate() {
            while (true) {
                timer = Stopwatch.StartNew();

                lock (gameListLock)
                {
                    for (int i = 0; i < gameList.Count; i++)
                    {
                        gameList[i]?.Update();
                    }
                }

                int sleeptime = (int)(Config.GAME_UPDATE_PERIOD - timer.ElapsedMilliseconds);

                if (sleeptime > 0)
                {
                    Thread.Sleep(sleeptime);
                }
                else {
                    Thread.Sleep(0);
                }

                timer.Stop();
            }
        }
    }
}
