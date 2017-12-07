using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNet;
using MyNet.ServerFramework;
using MyNet.Shared_GameLogic;
using System.Threading;
using System.Diagnostics;
using System.Numerics;

namespace OneVsOneServer.GameLogic.InGame
{
    class UserSessionMediator
    {
        Dictionary<string, UserSession> sessions = new Dictionary<string, UserSession>(Config.USER_COUNT_PER_ONE_GAME);
        object lockSessions = new object();

        public UserSession GetSession(string nickname) {
            UserSession session;
            while(!sessions.TryGetValue(nickname, out session));
            return session;
        }

        public void Add(UserSession session)
        {
            lock (lockSessions)
            {
                sessions.Add(session.UserNickname, session);
            }
        }

        public void Remove(UserSession session)
        {
            lock (lockSessions)
            {
                sessions.Remove(session.UserNickname);
            }
        }

        public bool GetPlayerPreparedFlag() {

            bool flag = false;

            lock (lockSessions) {
                var iter = sessions.GetEnumerator();
               
                while (iter.MoveNext()) {
                    flag = iter.Current.Value.IsGamePrepared;
                }
            }

            return flag;
        }

        public int GetSessionsCount() {
            return this.sessions.Count;
        }

        /// <summary>
        /// 저장된 모든 세션에 패킷을 보냄.
        /// </summary>
        /// <param name="packet"></param>
        public void BroadCast(Packet packet) {

            lock (lockSessions)
            {
                var iter = sessions.GetEnumerator();

                while (iter.MoveNext())
                {
                    iter.Current.Value.Send(packet);
                }
            }
        }
    }
}
