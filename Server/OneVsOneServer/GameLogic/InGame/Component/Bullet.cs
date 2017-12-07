using MyNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OneVsOneServer.GameLogic.InGame.Component
{
    class Bullet : IComponent
    {
        string ownerNickname;

        Vector3 direction;
        Vector3 position;

        readonly float flyingTime = 1.0f;
        readonly float speed = 15.0f;

        float firedTime;

        Circle2DCollider coll;

        public void Shot(Vector3 dir, Vector3 pos, string ownerNickname) {

            direction = dir;
            position = pos + (direction * speed * this.gameObject.DeltaTime);

            coll = this.AddComponent<Circle2DCollider>() as Circle2DCollider;
            coll.CollideCallback += OnCollide;
            coll.SetCollider(position, 0.2f);

            firedTime = this.gameObject.GameTime;

            Packet shotPacket = new Packet(Packet.HEADER.GAME_BULLET_FIRE, Config.MAX_SESSION_BUFFER_SIZE);

            shotPacket.Push(this.gameObject.id);
            shotPacket.Push(position.X);
            shotPacket.Push(position.Y);
            shotPacket.Push(direction.X);
            shotPacket.Push(direction.Y);

            this.gameObject.UserMediator.BroadCast(shotPacket);
            this.ownerNickname = ownerNickname;
        }

        private void OnCollide(GameObject obj) {
            if (obj.Tag == GameObjectManager.TAG.Character && ownerNickname != obj.Name) {
                UserCondition condition = obj.GetComponent<UserCondition>();
                condition.Damage(20, ownerNickname);
                DeleteThisBullet();
            }
        }

        public override void Update(float deltatime)
        {

            if ((this.gameObject.GameTime - this.firedTime) < flyingTime)
            {
                Packet shotPacket = new Packet(Packet.HEADER.GAME_BULLET_UPDATE, Config.MAX_SESSION_BUFFER_SIZE);

                position = position + (direction * speed * deltatime);

                coll.Circle.point.X = position.X;
                coll.Circle.point.Y = position.Y;

                shotPacket.Push(this.gameObject.id);
                shotPacket.Push(position.X);
                shotPacket.Push(position.Y);

                this.gameObject.UserMediator.BroadCast(shotPacket);
            }
            else {
                DeleteThisBullet();
            }

        }

        private void DeleteThisBullet() {
            Packet bulletDeletePacket = new Packet(Packet.HEADER.GAME_BULLET_DELETE, sizeof(uint));

            bulletDeletePacket.Push(this.gameObject.id);

            this.gameObject.UserMediator.BroadCast(bulletDeletePacket);

            this.gameObject.Release();
        }
    }
}
