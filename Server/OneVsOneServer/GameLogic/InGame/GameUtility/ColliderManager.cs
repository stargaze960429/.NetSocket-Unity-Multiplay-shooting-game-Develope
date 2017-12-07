using OneVsOneServer.GameLogic.InGame.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OneVsOneServer.GameLogic.InGame
{
    class Circle2DCollider : IComponent {

        ColliderArithmetic.Circle2D circle;
        ColliderManager.CollideCallback onCollide;

        bool isPrevColl = false;

        internal ColliderManager.CollideCallback CollideCallback
        {
            get
            {
                return onCollide;
            }

            set
            {
                onCollide = value;
            }
        }

        public ColliderArithmetic.Circle2D Circle
        {
            get
            {
                return circle;
            }
        }

        public uint GObjID
        {
            get
            {
                return this.gameObject.id;
            }
        }

        public void SetCollider(Vector3 center, float radius) {
            this.circle = new ColliderArithmetic.Circle2D(new Vector2(center.X, center.Y), radius);
        }

        public override void OnEnable()
        {
            this.gameObject.ColliderManager.AddCollider(this);
        }

        public override void OnDisable()
        {
            this.gameObject.ColliderManager.RemoveCollider(this);
        }

        public void OnCollide(GameObject obj) {
            onCollide?.Invoke(obj);
        }
        public void OnNotColide(GameObject obj) {
            
        }
    }

    class ColliderManager
    {
        public delegate void CollideCallback(GameObject collideObj);

        private List<Circle2DCollider> circle2DColliders = new List<Circle2DCollider>(20);
        private Stack<uint> beRemovedColliderIndexes = new Stack<uint>(10);

        public void AddCollider(Circle2DCollider coll) {
            circle2DColliders.Add(coll);
        }

        public void RemoveCollider(Circle2DCollider coll) {
            beRemovedColliderIndexes.Push(coll.GObjID);
        }

        public void Update()
        {
            Stack<int> removeIndexes = new Stack<int>();

            var iter = beRemovedColliderIndexes.GetEnumerator();

            while (iter.MoveNext())
            {
                for (int i = 0; i < circle2DColliders.Count; i++)
                {
                    if (iter.Current == circle2DColliders[i].GObjID)
                    {
                        removeIndexes.Push(i);
                    }
                }
            }


            var iter2 = removeIndexes.GetEnumerator();

            while (iter2.MoveNext())
            {
                circle2DColliders.RemoveAt(iter2.Current);
            }

            for (int i = 0; i < circle2DColliders.Count - 1; i++)
            {
                for (int j = i + 1; j < circle2DColliders.Count; j++)
                {
                    if (circle2DColliders[i].gameObject.Active && circle2DColliders[j].gameObject.Active)
                    {
                        if (ColliderArithmetic.IsCollision(circle2DColliders[i].Circle, circle2DColliders[j].Circle))
                        {
                            circle2DColliders[i].OnCollide(circle2DColliders[j].gameObject);
                            circle2DColliders[j].OnCollide(circle2DColliders[i].gameObject);
                        }
                        else
                        {
                            circle2DColliders[i].OnNotColide(circle2DColliders[j].gameObject);
                            circle2DColliders[j].OnNotColide(circle2DColliders[i].gameObject);
                        }
                    }
                }
            }
        }
    }
}
