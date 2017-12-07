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
    class MatchmakeUser : SessionInterface {

        bool beGameEnter = false; //매칭이 완료되었을때 true가 됨.(매칭 취소 불가능)
        bool gamePrepared = false; //매칭 완료 후 유저 대기 단계일때, 클라이언트가 Reply_complete_matchmake 패킷을 보내면 true가 된다.

        int index = -1;

        string nickname;

        public bool BeGameEnter
        {
            get
            {
                return beGameEnter;
            }

            set
            {
                beGameEnter = value;
            }
        }

        public bool GamePrepared
        {
            get
            {
                return gamePrepared;
            }
        }

        /// <summary>
        /// Matchmake 클래스 내 인덱스
        /// </summary>
        public int Index
        {
            get
            {
                return index;
            }

            set
            {
                index = value;
            }
        }

        public string Nickname
        {
            get
            {
                return nickname;
            }

            set
            {
                nickname = value;
            }
        }

        public override void OnDisconnect()
        {
            if (this.index != -1) {
                Matchmake.Instance.RemoveMatchingList(this, true);
            }
        }

        public override void OnReceive(Packet packet)
        {
            switch (packet.Head) {
                case Packet.HEADER.REQUEST_MATCHMAKE: {
                        Matchmake.Instance.StartMatching(this);
                        break;
                    }
                case Packet.HEADER.CANCEL_MATCHMAKE: {
                        if (beGameEnter == false)
                        {
                            Matchmake.Instance.CancelMatching(this);
                        }
                        break;
                    }
                case Packet.HEADER.REPLY_COMPLETE_MATCHMAKE: {
                        if (beGameEnter == true) {
                            gamePrepared = true;
                        }
                        break;
                    }
            }
        }
    }

    class MatchWaitingGroup {
        Stack<MatchmakeUser> users;
        int waitingCount;
        public int Index { get; set; }

        public int WaitingCount
        {
            get
            {
                return waitingCount;
            }

            set
            {
                waitingCount = value;
            }
        }

        internal Stack<MatchmakeUser> Users
        {
            get
            {
                return users;
            }

            set
            {
                users = value;
            }
        }

        public MatchWaitingGroup() {
            users = new Stack<MatchmakeUser>(Config.USER_COUNT_PER_ONE_GAME);
            waitingCount = 0;
            Index = -1;
        }
    }

    class Matchmake : Singleton<Matchmake> ,IContents
    {
        private static readonly Packet allow_matchmake = new Packet(Packet.HEADER.ALLOW_MATCHMAKE);
        private static readonly Packet allow_cancel_matchmake = new Packet(Packet.HEADER.ALLOW_CANCEL_MATCHMAKE);

        private static readonly Packet complete_matchmake = new Packet(Packet.HEADER.COMPLETE_MATCHMAKE);
        private static readonly Packet suddenly_quit_completed_match = new Packet(Packet.HEADER.SUDDENLY_QUIT_COMPLETED_MATCH);

        public static readonly int MATCHING_PERIOD = 1000; //milisecond 

        public static readonly int MAX_PREPARED_GROUP = 40;

         

        #region Matchmake 쓰레드

        Stack<int> matchingUserIndexPool;
        List<MatchmakeUser> matchingUserList;
        object lockMatchingList;

        Thread threadMatching;

        public bool AddMatchingList(MatchmakeUser user, bool flagLock)
        {
            if (flagLock)
            {
                lock (lockMatchingList)
                {
                    if (matchingUserIndexPool.Count > 0)
                    {
                        int index = matchingUserIndexPool.Pop();
                        matchingUserList[index] = user;
                        user.Index = index;
                        Console.WriteLine(user.Nickname + " 가 매치 검색을 시작했습니다.");
                    }
                    else {
                        return false;
                    }
                }
            }
            else {
                if (matchingUserIndexPool.Count > 0)
                {
                    int index = matchingUserIndexPool.Pop();
                    matchingUserList[index] = user;
                    user.Index = index;
                    Console.WriteLine(user.Nickname + " 가 매치 검색을 시작했습니다.");
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public void RemoveMatchingList(MatchmakeUser user, bool flagLock)
        {
            if (flagLock)
            {
                lock (lockMatchingList)
                {
                    matchingUserList[user.Index] = null;
                    matchingUserIndexPool.Push(user.Index);
                    user.Index = -1;
                    Console.WriteLine(user.Nickname + " 가 매치 검색을 취소했습니다.");
                }
            }
            else {
                matchingUserList[user.Index] = null;
                matchingUserIndexPool.Push(user.Index);
                user.Index = -1;
                Console.WriteLine(user.Nickname + " 가 매치 검색을 취소했습니다.");
            }
        }


        private void Procedure_Matchmaking()
        {

            /// 현재 n명의 유저를 스택에 집어넣고 5명 이상의 그룹이 완성되면 그것을 넘겨준다.

            Stopwatch watch;

            while (true)
            {

                watch = Stopwatch.StartNew();

                MatchWaitingGroup group = new MatchWaitingGroup();

                lock (lockMatchingList)
                {

                    for (int i = 0; i < Config.MAX_CONNECTIONS; i++)
                    {
                        if (matchingUserList[i] != null)
                        {
                            group.Users.Push(matchingUserList[i]);
                            if (group.Users.Count == Config.USER_COUNT_PER_ONE_GAME)
                            { // 그룹에 포함된 유저수가 한 게임당 유저수를 넘었다면
                                if (AddPreparedGroupList(group, true)) // 그룹을 '준비된그룹리스트' 에 넣고
                                {
                                    var iter = group.Users.GetEnumerator(); //모든 유저에게 매칭완료 패킷을 보내야함
                                    while (iter.MoveNext())
                                    {
                                        iter.Current.BeGameEnter = true;
                                        RemoveMatchingList(iter.Current, false);
                                        iter.Current.Send(complete_matchmake);
                                    }
                                }
                                group = new MatchWaitingGroup();
                            }
                        }
                    }

                }

                watch.Stop();

                Thread.Sleep((int)(MATCHING_PERIOD - watch.ElapsedMilliseconds));
            }
        }



        #endregion

        #region 대기중인 그룹 체크 쓰레드


        //pg is "Prepared Group"
        Stack<int> pgIndexPool;
        List<MatchWaitingGroup> pgList;
        object pglock;

        Thread threadCheckGroup;

        private bool AddPreparedGroupList(MatchWaitingGroup group, bool flagLock) {
            if (flagLock)
            {
                lock (pglock)
                {
                    if (pgIndexPool.Count > 0)
                    {
                        int index = pgIndexPool.Pop();
                        pgList[index] = group;
                        pgList[index].Index = index;
                        return true;
                    }
                }
            }
            else
            {
                if (pgIndexPool.Count > 0)
                {
                    int index = pgIndexPool.Pop();
                    pgList[index] = group;
                    pgList[index].Index = index;
                    return true;
                }
            }
            return false;
        }

        private void RemovePreparedGroupList(MatchWaitingGroup group, bool flagLock) {
            if (flagLock)
            {
                lock (pglock)
                {
                    pgList[group.Index] = null;
                    pgIndexPool.Push(group.Index);
                    group.Index = -1;
                }
            }
            else
            {
                pgList[group.Index] = null;
                pgIndexPool.Push(group.Index);
                group.Index = -1;
            }
        }


        /// <summary>
        /// 쓰레드에 의해 호출되는 함수
        /// </summary>
        private void Procedure_CheckPreparedGroup()
        {

            Stopwatch watch;

            while (true)
            {
                watch = Stopwatch.StartNew();

                lock (pglock)
                {

                    for (int i = 0; i < MAX_PREPARED_GROUP; i++)
                    {
                        if (pgList[i] != null)
                        {
                            if (pgList[i].WaitingCount < 3)
                            {
                                var iter = pgList[i].Users.GetEnumerator();
                                int preparedUserCount = 0; // 준비된 유저를 찾을 때 마다 +1
                                while (iter.MoveNext())
                                {
                                    if (iter.Current.GamePrepared) //준비했는지 검사
                                    {
                                        preparedUserCount += 1;
                                    }
                                    else
                                    {  //안했으면?
                                        pgList[i].WaitingCount++;
                                        break;
                                    }
                                }
                                if (preparedUserCount == Config.USER_COUNT_PER_ONE_GAME) // 한 매치당 유저수를 넘는다면 그것은 모두 준비한 것
                                {
                                    // 모두 준비되었을때 처리를 해주어야함.
                                    // 현재는 테스트를 위해 "갑작스러운 종료" 패킷을 보냄.
                                    // 이 패킷을 보내고 유저들을 모두 매칭쓰레드로 돌려줌
                                    string str = "";
                                    var iter2 = pgList[i].Users.GetEnumerator();
                                    while (iter2.MoveNext())
                                    {
                                        str += iter2.Current.Nickname + " ";
                                    }
                                    GameManager.Instance.MakeGame(pgList[i].Users);
                                    RemovePreparedGroupList(pgList[i], false);
                                    Console.WriteLine("새로운 매칭이 생성 되었습니다. " + str);
                                }
                            }
                            else
                            {
                                // 세번 체크했는데 응답이 없으면 문제가 생긴 것으로 판단
                                // 모든 유저들을 그룹에서 제거하고 다시 유저 풀 리스트로 옮김
                                // 그렇게 하여 다시 매칭이 가능하도록
                                var iter = pgList[i].Users.GetEnumerator();
                                while (iter.MoveNext())
                                {
                                    iter.Current.BeGameEnter = false;
                                    iter.Current.Send(suddenly_quit_completed_match);
                                }
                                RemovePreparedGroupList(pgList[i], false);
                            }
                        }
                    }
                }

                watch.Stop();

                Thread.Sleep((int)(MATCHING_PERIOD - watch.ElapsedMilliseconds));
            }
        }

        #endregion


        private Matchmake()
        {
            matchingUserList = new List<MatchmakeUser>(Config.MAX_CONNECTIONS);
            matchingUserIndexPool = new Stack<int>(Config.MAX_CONNECTIONS);
            lockMatchingList = new object();

            pgList = new List<MatchWaitingGroup>(MAX_PREPARED_GROUP);
            pgIndexPool = new Stack<int>(MAX_PREPARED_GROUP);
            pglock = new object();

            for (int j = 0; j < MAX_PREPARED_GROUP; j++)
            {
                pgList.Add(null);
                pgIndexPool.Push(j);
            }

            for (int i = 0; i < Config.MAX_CONNECTIONS; i++)
            {
                matchingUserList.Add(null);
                matchingUserIndexPool.Push(i);
            }

            threadMatching = new Thread(Procedure_Matchmaking);
            threadCheckGroup = new Thread(Procedure_CheckPreparedGroup);

            threadMatching.Start();
            threadCheckGroup.Start();
        }

        public void Assign(SessionInterface defaultInterface)
        {
            LoginUser src = defaultInterface as LoginUser;
            MatchmakeUser user = new MatchmakeUser();
            user.TakeOver(defaultInterface);
            user.Nickname = src.Nickname;
            Console.WriteLine("새로운 유저가 매치메이킹에 등록됨. " + user.Nickname);
        }


        public void StartMatching(MatchmakeUser user) {
            AddMatchingList(user, true);
            user.Send(allow_matchmake);
        }

        public void CancelMatching(MatchmakeUser user) {
            RemoveMatchingList(user, true);
            user.Send(allow_cancel_matchmake);
        }

    }
}
