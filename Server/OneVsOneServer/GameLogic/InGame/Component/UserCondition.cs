using MyNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneVsOneServer.GameLogic.InGame.Component
{
    class UserCondition : IComponent
    {
        static readonly int MaxHP = 100;
        static readonly float DefaultSpeed = 6.0f;
        static readonly float RespawnTime = 7.0f;

        public bool IsAlive {
            get;
            private set;
        }

        int hp;
        float moveSpeed;
        float dieTime;

        public int Hp
        {
            get
            {
                return hp;
            }
        }

        public float MoveSpeed
        {
            get
            {
                return moveSpeed;
            }
        }

        UserSession session;

        public override void OnCreate(GameObject ownObj)
        {
            base.OnCreate(ownObj);
            session = this.gameObject.UserMediator.GetSession(this.gameObject.Name);

            hp = MaxHP;
            moveSpeed = DefaultSpeed;
            IsAlive = true;
            
        }

        public override void Update(float deltatime)
        {
            if (this.gameObject.GameTime - dieTime > RespawnTime && IsAlive == false) {
                this.Respawn();
            }
        }

        private void Die() {
            Packet diePacket = new Packet(Packet.HEADER.GAME_UPDATE_PLAYER_DIE, Config.MAX_SESSION_BUFFER_SIZE);
            diePacket.Push(this.gameObject.Name);
            diePacket.Push(Encoding.ASCII.GetByteCount(this.gameObject.Name));
            diePacket.Push(RespawnTime);

            this.session.Send(diePacket);

            dieTime = gameObject.GameTime;
            IsAlive = false;
        }

        private void Respawn() {
            UserInputProcess user = this.GetComponent<UserInputProcess>() as UserInputProcess;
            user.Position = user.GetRandomValidPosition();

            Packet respawnPacket = new Packet(Packet.HEADER.GAME_UPDATE_PLAYER_RESPAWN, Config.MAX_SESSION_BUFFER_SIZE);

            respawnPacket.Push(this.gameObject.Name);
            respawnPacket.Push(Encoding.ASCII.GetByteCount(this.gameObject.Name));
            respawnPacket.Push(user.Position.X);
            respawnPacket.Push(user.Position.Y);

            this.session.Send(respawnPacket);

            hp = MaxHP;
            Packet hpPacket = new Packet(Packet.HEADER.GAME_UPDATE_OWN_HP, sizeof(int));
            hpPacket.Push(hp);

            this.session.Send(hpPacket);
            IsAlive = true;
        }

        public void Damage(int amount) {

            if (IsAlive)
            {
                hp -= amount;

                Packet p = new Packet(Packet.HEADER.GAME_UPDATE_OWN_HP, sizeof(int));
                p.Push(hp);

                session.Send(p);

                if (hp <= 0)
                {
                    Die();
                }
            }
        }
    }
}
