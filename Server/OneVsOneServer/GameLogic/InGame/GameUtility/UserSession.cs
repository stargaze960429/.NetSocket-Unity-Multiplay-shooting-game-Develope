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

namespace OneVsOneServer.GameLogic.InGame
{
    class UserInput {

        KeyInputSnapshot keyInput;
        float characterRotation;

        public KeyInputSnapshot KeyInput
        {
            get
            {
                return keyInput;
            }

            set
            {
                keyInput = value;
            }
        }

        public float CharacterRotation
        {
            get
            {
                return characterRotation;
            }

            set
            {
                characterRotation = value;
            }
        }

        public UserInput(KeyInputSnapshot key, float rotation) {
            keyInput = key.Clone() as KeyInputSnapshot;
            characterRotation = rotation;
        }

        public void Clear() {
            this.keyInput = new KeyInputSnapshot();
            this.characterRotation = 0.0f;
        }

    }

    class UserSession : SessionInterface
    {
        public delegate void Event_Disconnect(UserSession session);
        string nickname;

        public Event_Disconnect onDisconnect;

        UserInput input = new UserInput(new KeyInputSnapshot(), 0.0f);

        public string UserNickname
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

        public bool IsGamePrepared
        {
            get
            {
                return isGamePrepared;
            }
        }
        private bool isGamePrepared;

        public UserInput TakeInput() {
            UserInput result = new UserInput(this.input.KeyInput.Clone() as KeyInputSnapshot, this.input.CharacterRotation);
            this.input.Clear();
            return result;
        }

        
        public override void OnDisconnect()
        {
            this.onDisconnect?.Invoke(this);
        }

        public override void OnReceive(Packet packet)
        {
            switch (packet.Head)
            {
                case Packet.HEADER.REPLY_GAME_INIT_COMPLETE:
                    {
                        Console.WriteLine(this.nickname + "가 게임 클라이언트 초기화를 완료했습니다.");
                        isGamePrepared = true;
                        break;
                    }
                case Packet.HEADER.GAME_INPUT:
                    {
                        this.input.CharacterRotation = packet.Pop_Float();

                        KeyInputSnapshot input = new KeyInputSnapshot();
                        input.Deserialize(packet.Pop_Bytes(KeyInputSnapshot.MemberSize));
                        this.input.KeyInput += input;
                        break;
                    }
            }
        }
    }
}
