using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OneVsOneServer.GameLogic.InGame
{
    class GameObjectManager
    {

        public enum TAG {
            Default,
            Character,
            Bullet
        }

        Dictionary<uint, GameObject> gameObjects = new Dictionary<uint, GameObject>(1000);
        Dictionary<uint, GameObject> beAddedObjs = new Dictionary<uint, GameObject>(100);
        Stack<GameObject> beRemovedObjs = new Stack<GameObject>(100);

        uint idSeed = 0;

        Game ownGame;

        public GameObjectManager(Game game) {
            this.ownGame = game;
        }

        public GameObject CreateGameObject(string name = "GameObject", bool active = true)
        {
#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
            GameObject obj = new GameObject(name, active, ++idSeed, ownGame);
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.
            beAddedObjs.Add(obj.id, obj);
            return obj;
        }

        public void RemoveGameObject(GameObject obj)
        {
            beRemovedObjs.Push(obj);
            obj.Stop();
            obj.OnDestroy();
        }

        public GameObject Find(uint Id) {

            GameObject result;

            if (gameObjects.TryGetValue(Id, out result))
            {
                return result;
            }
            else if(beAddedObjs.TryGetValue(Id, out result)) {
                return result;
            }
            return null;
        }

        public void UpdateAllGameObjects(float delta) {

            var removeIter = beRemovedObjs.GetEnumerator();

            while (removeIter.MoveNext())
            {
                gameObjects.Remove(removeIter.Current.id);
            }

            beRemovedObjs.Clear();


            var iter = gameObjects.GetEnumerator();

            while (iter.MoveNext())
            {
                iter.Current.Value.Update(delta);
            }

            var addIter = beAddedObjs.GetEnumerator();

            while (addIter.MoveNext()) {
                gameObjects.Add(addIter.Current.Key, addIter.Current.Value);
            }

            beAddedObjs.Clear();
        }
    }
}
