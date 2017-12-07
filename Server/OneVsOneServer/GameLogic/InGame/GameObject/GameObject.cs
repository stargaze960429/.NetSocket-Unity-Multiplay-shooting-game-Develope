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
using OneVsOneServer.GameLogic.InGame.Component;
using OneVsOneServer.GameLogic.InGame;

namespace OneVsOneServer.GameLogic
{

    class GameObject
    {
        string name;
        int active;
        Dictionary<Type, IComponent> components;
        Dictionary<Type, IComponent> beAddedComponents;
        Game ownGame;
        bool isBeDeleted;
        GameObjectManager.TAG tag;

        public float DeltaTime {
            get {
                return ownGame.Time.deltaTime;
            }
        }

        public float GameTime {
            get {
                return ownGame.Time.Time;
            }
        }

        public UserSessionMediator UserMediator {
            get {
                return ownGame.UserMediator;
            }
        }

        public ColliderManager ColliderManager {
            get { return ownGame.ColliderManager; }
        }

        public Scoreboard ScoreBoard {
            get { return ownGame.Scoreboard; }
        }

        public GameObject CreateGameObject(string name = "GameObject", bool active = true)
        {
            return this.ownGame.GameObjectManager.CreateGameObject(name, active);
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }
        public bool Active
        {
            get
            {
                return active == 1;
            }

            private set
            {
                int originValue = Interlocked.Exchange(ref this.active, value ? 1 : 0);
                if (this.Active && originValue != (value ? 1 : 0))
                {
                    var iter = beAddedComponents.GetEnumerator();

                    while (iter.MoveNext()) {
                        iter.Current.Value.OnEnable();
                    }

                    iter = components.GetEnumerator();

                    while (iter.MoveNext())
                    {
                        iter.Current.Value.OnEnable();
                    }
                }
                else if(!this.Active && originValue != (value ? 1 : 0))
                {
                    var iter = beAddedComponents.GetEnumerator();

                    while (iter.MoveNext())
                    {
                        iter.Current.Value.OnDisable();
                    }

                    iter = components.GetEnumerator();

                    while (iter.MoveNext())
                    {
                        iter.Current.Value.OnDisable();
                    }
                }
            }
        }

        internal GameObjectManager.TAG Tag
        {
            get
            {
                return tag;
            }

            set
            {
                tag = value;
            }
        }

        public readonly uint id;

        [System.Obsolete("이 메소드는 사용되지 않습니다. GameObjectManager의 CreateGameObject 메소드를 사용하십시오.")]
        public GameObject(string name, bool active, uint id, Game game)
        {
            components = new Dictionary<Type, IComponent>(5);
            beAddedComponents = new Dictionary<Type, IComponent>(3);
            this.name = name;
            this.Active = active;
            this.id = id;
            this.isBeDeleted = false;
            ownGame = game;
        }

        public void Wakeup()
        {
            if (isBeDeleted) {
                throw new Exception("This GameObject Already Deleted");
            }
            Active = true;
        }

        public void Stop()
        {
            if (isBeDeleted)
            {
                throw new Exception("This GameObject Already Deleted");
            }
            Active = false;
        }

        public void Release()
        {
            if (isBeDeleted)
            {
                throw new Exception("This GameObject Already Deleted");
            }
            this.ownGame.GameObjectManager.RemoveGameObject(this);
        }

        public void OnDestroy()
        {
            if (isBeDeleted)
            {
                throw new Exception("This GameObject Already Deleted");
            }
            var iter = components.GetEnumerator();

            while (iter.MoveNext())
            {
                iter.Current.Value.OnDestroy();
            }
            isBeDeleted = true;
        }

        public void Update(float deltaTime)
        {
            if (!isBeDeleted && this.Active) {

                var iter = components.GetEnumerator();

                while (iter.MoveNext())
                {
                    iter.Current.Value.Update(deltaTime);
                }

                var addIter = this.beAddedComponents.GetEnumerator();

                while (addIter.MoveNext())
                {
                    components.Add(addIter.Current.Key, addIter.Current.Value);
                }

                beAddedComponents.Clear();
            }
        }

        public T AddComponent<T>() where T : IComponent, new()
        {
            if (isBeDeleted)
            {
                throw new Exception("This GameObject Already Deleted");
            }

            T t = new T();
            t.OnCreate(this);
            if (this.Active) {
                t.OnEnable();
            }
            this.beAddedComponents.Add(typeof(T), t);
            return t;
        }

        public T GetComponent<T>() where T : IComponent
        {
            if (isBeDeleted)
            {
                throw new Exception("This GameObject Already Deleted");
            }
            IComponent result;

            if (components.TryGetValue(typeof(T), out result))
            {
                return (T)result;
            }
            else if (beAddedComponents.TryGetValue(typeof(T), out result))
            {
                return (T)result;
            }
            else
            {
                return null;
            }
        }
    }
    
}
