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

namespace OneVsOneServer.GameLogic.InGame.Component
{
    class UserInputProcess : IComponent
    {
        Vector3 mapCenter = new Vector3(0.0f);
        float height = 0.0f;
        float width = 0.0f;

        float firingInterval = 0.15f;

        float lastFired;

        Random random = new Random();

        private static readonly float Deg2Rad = 0.0174532924f;

        UserSessionMediator mediator;
        UserSession session;

        Vector3 position;

        UserCondition condition;
        Circle2DCollider coll;

        private readonly float CharacterRadius = 0.3f;

        public Vector3 Position
        {
            get
            {
                return position;
            }
            set {
                position = value;
            }
        }

        public void SetFence(Vector3 point, float height, float width) {
            this.mapCenter = new Vector3(point.X, point.Y, 0.0f);
            this.height = height; // 이 게임에서 y는 아랫쪽으로 향하기 때문에 마이너스로 바꿔줌
            this.width = width;
        }

        public override void OnCreate(GameObject gameObj)
        {
            base.OnCreate(gameObj);
            position = new Vector3((float)random.NextDouble() * width, (float)random.NextDouble() * height, 0.0f);
            this.mediator = this.gameObject.UserMediator;
            this.session = this.mediator.GetSession(this.gameObject.Name);
            condition = this.AddComponent<UserCondition>() as UserCondition;
            coll = this.AddComponent<Circle2DCollider>() as Circle2DCollider;
            coll.SetCollider(position, CharacterRadius);
        }

        public override void Update(float deltaTime)
        {
            if (condition.IsAlive)
            {
                UserInput input = this.session.TakeInput();

                ProcessMove(input, deltaTime);
                ProcessShoot(input, deltaTime);
            }
        }

        public Vector3 GetRandomValidPosition() {
            return new Vector3((float)random.NextDouble() * width, (float)random.NextDouble() * height, 0.0f);
        }

        public void ProcessShoot(UserInput input, float deltaTime)
        {
            if (input.KeyInput.mouseLeftDown && (this.gameObject.GameTime - lastFired) > firingInterval)
            {
                float radAngle = input.CharacterRotation * Deg2Rad;
                lastFired = this.gameObject.GameTime;

                Vector3 bulletDirection = Vector3.Normalize(new Vector3((float)Math.Cos(radAngle), (float)Math.Sin(radAngle), 0.0f));
                Vector3 bulletPosition = this.position + (bulletDirection * 5.0f * deltaTime);

                //총알 오브젝트 생성
                this.CreateGameObject("Bullet", true).AddComponent<Bullet>().Shot(bulletDirection, bulletPosition, this.gameObject.Name);
            }
        }

        public void ProcessMove(UserInput input, float deltaTime) {

            Packet movePacket = new Packet(Packet.HEADER.GAME_UPDATE_PLAYER_POSITION, Config.MAX_SESSION_BUFFER_SIZE);
            int vertical = 0; int horizontal = 0;

            vertical += input.KeyInput.upKey ? 1 : 0;
            vertical += input.KeyInput.downKey ? -1 : 0;

            horizontal += input.KeyInput.leftKey ? -1 : 0;
            horizontal += input.KeyInput.rightKey ? 1 : 0;

            float nextX = position.X + (horizontal * this.condition.MoveSpeed * deltaTime);
            float nextY = position.Y + (vertical * this.condition.MoveSpeed * deltaTime);

            nextX = nextX > width ? width : nextX;
            nextX = nextX < mapCenter.X ? mapCenter.X : nextX;

            nextY = nextY < mapCenter.Y ? mapCenter.Y : nextY;
            nextY = nextY > height ? height : nextY;

            position = new Vector3(nextX, nextY, position.Z);
            coll.Circle.point.X = position.X;
            coll.Circle.point.Y = position.Y;

            movePacket.Push(this.gameObject.Name);
            movePacket.Push(Encoding.ASCII.GetByteCount(this.gameObject.Name));
            movePacket.Push(this.position.X);
            movePacket.Push(this.position.Y);

            movePacket.Push(input.CharacterRotation); 

            mediator.BroadCast(movePacket);
        }
    }
}
