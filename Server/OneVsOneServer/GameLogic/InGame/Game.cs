using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNet;
using MyNet.ServerFramework;
using MyNet.Shared_GameLogic;
using System.Threading;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using OneVsOneServer.GameLogic.InGame.Component;
using OneVsOneServer.GameLogic.InGame;
using static OneVsOneServer.GameLogic.InGame.GameObjectManager;

namespace OneVsOneServer.GameLogic
{

    class Game : IContents
    {
        enum STATE { IDLE, INITIALIZE, WAIT_ALL_PLAYER_INITIALIZED ,SIMULATION }

        Queue<Packet> initializedPacketQueue = new Queue<Packet>(200);

        STATE state = STATE.IDLE;

        int prevRenewedTime = 0;

        UserSessionMediator userMediator;
        GameObjectManager gameObjectManager;
        ColliderManager colliderManager;
        GameTimer timer;

        Vector3 mapCenter = new Vector3(0.0f);
        float mapHeight = 20.0f;
        float mapWidth = 20.0f;

        List<uint> UserGameObjectIndexes = new List<uint>(Config.USER_COUNT_PER_ONE_GAME);

        public GameTimer Time {
            get { return timer; }    
        }

        public GameObjectManager GameObjectManager {
            get { return this.gameObjectManager; }
        }

        public UserSessionMediator UserMediator {
            get { return this.userMediator; }
        }

        public ColliderManager ColliderManager {
            get { return this.colliderManager; }
        }
        
        public Game() {
            userMediator = new UserSessionMediator();
            gameObjectManager = new GameObjectManager(this);
            timer = new GameTimer();
            colliderManager = new ColliderManager();
        }

        public void Assign(SessionInterface defaultInterface)
        {
            MatchmakeUser matchedUser = defaultInterface as MatchmakeUser;

            GameObject newPlayer = gameObjectManager.CreateGameObject(matchedUser.Nickname, false);

            UserSession userSession = new UserSession();

            userSession.UserNickname = matchedUser.Nickname;
            userSession.TakeOver(matchedUser);

            userMediator.Add(userSession);

            UserGameObjectIndexes.Add(newPlayer.id);

            newPlayer.Tag = TAG.Character;
            UserInputProcess userInputProcess = newPlayer.AddComponent<UserInputProcess>();

            userInputProcess.SetFence(this.mapCenter, this.mapHeight, this.mapWidth);

            Console.WriteLine(matchedUser.Nickname + "이가 게임에 참가했습니다.");

            if (userMediator.GetSessionsCount() == Config.USER_COUNT_PER_ONE_GAME) {
                Start();
            }
        }

        public void DeleteSession(SessionInterface userSession) {
            UserSession session = userSession as UserSession;
            session.onDisconnect -= DeleteSession;
            userMediator.Remove(session);
        }

        /// <summary>
        /// 이니셜라이즈 패킷을 채운다.
        /// </summary>
        private void PrepareInitPackets() {
            var iter = UserGameObjectIndexes.GetEnumerator();

            while (iter.MoveNext()) {
                Packet playerInfo = new Packet(Packet.HEADER.GAME_INIT_PLAYER, Config.MAX_SESSION_BUFFER_SIZE);
                GameObject userObj = gameObjectManager.Find(iter.Current);

                playerInfo.Push(userObj.Name);
                playerInfo.Push(Encoding.ASCII.GetByteCount(userObj.Name));
                UserInputProcess usermove = userObj.GetComponent<UserInputProcess>();
                playerInfo.Push(usermove.Position.X);
                playerInfo.Push(usermove.Position.Y);

                initializedPacketQueue.Enqueue(playerInfo);
            }

            //모든 패킷을 채우고 마지막에는 "모든 초기화 패킷 보냄" 패킷을 넣어줘야함.
            initializedPacketQueue.Enqueue(new Packet(Packet.HEADER.GAME_INIT_ALL_PACKET_SENDED));
        }

        public void Start() {
            PrepareInitPackets();
            timer.Start();
            state = STATE.INITIALIZE;
        }

        private void SendInitializePackets() {
            if (initializedPacketQueue.Count > 0)
            {
                Packet p = initializedPacketQueue.Peek();

                userMediator.BroadCast(p);
                if (p.Head == Packet.HEADER.GAME_INIT_ALL_PACKET_SENDED)
                {
                    state = STATE.WAIT_ALL_PLAYER_INITIALIZED;
                }
                initializedPacketQueue.Dequeue();
            }
        }

        private void UpdateSimulation() {

            colliderManager.Update();

            int time = timer.TimeSecond;
            if (time > prevRenewedTime)
            { //시간 패킷 보내주기
                Packet timePacket = new Packet(Packet.HEADER.GAME_TIME, sizeof(Int32));
                timePacket.Push(time);
                userMediator.BroadCast(timePacket);
                prevRenewedTime = time;
            }

            this.GameObjectManager.UpdateAllGameObjects(this.Time.deltaTime);
        }

        private void CheckAllUserPrepared() {
            bool isAllPlayerInit = userMediator.GetPlayerPreparedFlag();

            if (isAllPlayerInit == true)
            {
                Console.WriteLine("게임 시뮬레이션을 시작합니다.");

                Packet gameStartPacket = new Packet(Packet.HEADER.GAME_SIMULATION_START);

                userMediator.BroadCast(gameStartPacket);

                for (int i = 0; i < UserGameObjectIndexes.Count; i++)
                {
                    this.gameObjectManager.Find(UserGameObjectIndexes[i]).Wakeup();
                }

                state = STATE.SIMULATION;
            }
        }

        public void Update() {

            if (state == STATE.IDLE) { return; }

            else if (state == STATE.INITIALIZE)
            {
                SendInitializePackets();
            }

            else if (state == STATE.WAIT_ALL_PLAYER_INITIALIZED) {
                CheckAllUserPrepared();
            }

            else if (state == STATE.SIMULATION) {
                UpdateSimulation();
            }

            timer.Update();
        }
    }
}
