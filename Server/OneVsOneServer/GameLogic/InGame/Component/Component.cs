using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OneVsOneServer.GameLogic.InGame.Component
{

    abstract class IComponent
    {
        GameObject ownObj;

        public GameObject gameObject {
            get { return ownObj; }
        }

        public virtual void OnCreate(GameObject ownObj) {
            this.ownObj = ownObj;
        }

        public virtual void Update(float deltatime) { }

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void OnDestroy() { }

        public IComponent GetComponent<T>() where T : IComponent {
            return this.gameObject.GetComponent<T>();
        }

        public IComponent AddComponent<T>() where T : IComponent, new(){
            return this.gameObject.AddComponent<T>();
        }

        public GameObject CreateGameObject(string name = "GameObject", bool active = true) {
            return this.gameObject.CreateGameObject(name, active);
        }
    }
}
