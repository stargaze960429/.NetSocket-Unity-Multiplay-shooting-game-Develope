using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNet;
using MyNet.ServerFramework;
using System.Text.RegularExpressions;

namespace OneVsOneServer.GameLogic
{

    class LoginUser : SessionInterface
    {
        string nickname;
        bool verified;

        private static readonly Packet apply_Nickname = new Packet(Packet.HEADER.APPLY_NICKNAME);
        private static readonly Packet wrong_Nickname = new Packet(Packet.HEADER.WRONG_NICKNAME);

        private static readonly Packet allow_confirm_nickname = new Packet(Packet.HEADER.ALLOW_CONFIRM_NICKNAME);

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
            Console.WriteLine("연결이 종료되었습니다.");
        }

        public override void OnReceive(Packet packet)
        {
            switch (packet.Head)
            {
                case Packet.HEADER.REPLY_NICKNAME:
                    {
                        string str = packet.Pop_String(packet.Size);
                        Console.WriteLine("받은 닉네임 정보 : " + str);

                        if (Login.CheckNicknameValidation(str))
                        {
                            Console.WriteLine("유효한 닉네임 입니다.");
                            this.nickname = str;
                            this.verified = true;
                            this.Send(apply_Nickname);
                        }
                        else
                        {
                            Console.WriteLine("유효하지 않은 닉네임 입니다.");
                            this.verified = false;
                            this.Send(wrong_Nickname);
                        }
                        break;
                    }
                case Packet.HEADER.REQUEST_CONFIRM_NICKNAME:
                    {
                        if (verified && nickname != null)
                        {
                            Console.WriteLine("닉네임 승인 완료. " + nickname + "를 매치메이킹 인스턴스로 전송합니다.");
                            this.Send(allow_confirm_nickname);
                            Matchmake.Instance.Assign(this);
                        }
                        else {
                            Console.WriteLine("닉네임 거부. 잘못된 접근입니다.");
                            ForcedQuit();
                        }
                        break;
                    }
            }
        }
    }

    class Login : Singleton<Login>, IContents
    {
        Packet request_Nickname;

        private Login()
        {
            request_Nickname = new Packet(Packet.HEADER.REQUEST_NICKNAME);
        }

        public void Assign(SessionInterface defaultInterface)
        {
            LoginUser newUser = new LoginUser();
            newUser.TakeOver(defaultInterface);
            newUser.Send(request_Nickname);
        }

        public static bool CheckNicknameValidation(string nickname)
        {
            return (new Regex("^[a-zA-Z0-9]*$").IsMatch(nickname) && nickname.Length <= 20 && nickname.Length >= 6);
        }
    }
}
