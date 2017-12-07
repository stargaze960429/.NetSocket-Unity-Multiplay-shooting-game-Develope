using MyNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneVsOneServer.GameLogic.InGame
{
    
    class Scoreboard
    {
        public class PlayerScore : IComparable
        {
            public string Nickname { get; set; }

            public int Kill { get; set; }
            public int Death { get; set; }

            public int CompareTo(object obj)
            {
                if (obj is PlayerScore)
                {
                    PlayerScore score = obj as PlayerScore;

                    return score.Kill - this.Kill;
                }
                else {
                    throw new InvalidOperationException();
                }
            }
        }

        public class CallBack {
            public Predicate<PlayerScore> Condition { get; set; }
            public Action<PlayerScore> Action { get; set; }
        }

        CallBack onScoreChanged = new CallBack();

        Dictionary<string, PlayerScore> playerScores = new Dictionary<string, PlayerScore>(Config.USER_COUNT_PER_ONE_GAME);
        Game owngame;

        public Scoreboard(Game owngame) {
            this.owngame = owngame;
        }

        public void SetCallback(Predicate<PlayerScore> condition, Action<PlayerScore> action) {
            this.onScoreChanged.Condition = condition;
            this.onScoreChanged.Action = action;
        }

        public void AddNewEmptyPlayerScore(string nickname) {
            PlayerScore score = new PlayerScore();
            score.Nickname = nickname;

            playerScores.Add(nickname, score);
        }

        public void ReportKill(string killerName, string victimName)
        {
            playerScores[killerName].Kill++;
            playerScores[victimName].Death++;

            Packet diePacket = new Packet(Packet.HEADER.GAME_UPDATE_PLAYER_DIE, Config.MAX_SESSION_BUFFER_SIZE);
            diePacket.Push(victimName);
            diePacket.Push(Encoding.ASCII.GetByteCount(victimName));
            diePacket.Push(killerName);
            diePacket.Push(Encoding.ASCII.GetByteCount(killerName));

            if (onScoreChanged.Condition != null) {
                if (onScoreChanged.Condition.Invoke(playerScores[killerName])) {
                    onScoreChanged.Action.Invoke(playerScores[killerName]);
                }
            }

            this.owngame.UserMediator.BroadCast(diePacket);
        }


        public List<PlayerScore> GetSortedPlayerScores() {
            List<PlayerScore> scores = new List<PlayerScore>(this.playerScores.Count);

            var iter = playerScores.GetEnumerator();

            while (iter.MoveNext()) {
                scores.Add(iter.Current.Value);
            }

            scores.Sort();

            return scores;
        }
    }
}
