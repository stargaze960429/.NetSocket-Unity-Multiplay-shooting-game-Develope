using MyNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletUpdater : PacketHandle {

    [SerializeField]
    GameObject prefabBullet;

    public class BulletInterPolation {
        public InterpolateVector3 Position { get; set; }

        public uint serverObjID { get; set; }
        public GameObject obj { get; set; }

        public BulletInterPolation(uint serverObjID, InterpolateVector3 pos, GameObject obj) {
            this.Position = pos;
            this.serverObjID = serverObjID;
            this.obj = obj;
        }
    }

    Dictionary<uint, BulletInterPolation> bullets = new Dictionary<uint, BulletInterPolation>(100);

    public void Update()
    {
        var iter = bullets.GetEnumerator();

        while (iter.MoveNext()) {
            Vector3 pos = iter.Current.Value.Position.Interpolated;

            if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y)) {
                iter.Current.Value.obj.transform.position = pos;
            }
        }
    }

    Quaternion GetRotationFromVector(Vector3 posStart, Vector3 posend)
    {
        return Quaternion.Euler(0.0f, 0.0f, -Mathf.Atan2(posend.x - posStart.x, posend.y - posStart.y) * Mathf.Rad2Deg);
    }

    public void CreateBullet(uint id, Vector3 pos, Vector3 dir) {
        GameObject bulletObj = Instantiate(prefabBullet, Vector3.zero, GetRotationFromVector(pos, dir));
        BulletInterPolation bullet = new BulletInterPolation(id, new InterpolateVector3(pos, Time.time), bulletObj);
        bullets.Add(id, bullet);
    }

    public void UpdateBullet(uint id, Vector3 pos) {

        bullets[id].obj.transform.position = this.bullets[id].Position.To;
        bullets[id].Position.SetNewVector3(pos);

        //BulletInterPolation bullet;

        //if (this.bullets.TryGetValue(id, out bullet))
        //{
        //    bullet.obj.transform.position = this.bullets[id].Position.To;
        //    bullet.Position.SetNewVector3(pos);
        //}
        //else {
        //    Debug.Log("생성되지 않은 총알을 업데이트 하려 함.");
        //}
    }

    public void DeleteBullet(uint id) {
        Destroy(this.bullets[id].obj);
        this.bullets.Remove(id);
    }


    protected override void OnMessage(Packet packet) {
        switch (packet.Head) {
            case Packet.HEADER.GAME_BULLET_FIRE: {
                    float dirY = packet.Pop_Float();
                    float dirX = packet.Pop_Float();
                    float posY = packet.Pop_Float();
                    float posX = packet.Pop_Float();
                    uint id = packet.Pop_UInt32();

                    CreateBullet(id, new Vector3(posX, posY), new Vector3(dirX, dirY));
                    break;
                }
            case Packet.HEADER.GAME_BULLET_UPDATE: {
                    float posY = packet.Pop_Float();
                    float posX = packet.Pop_Float();
                    uint id = packet.Pop_UInt32();

                    UpdateBullet(id, new Vector3(posX, posY));
                    break;
                }
            case Packet.HEADER.GAME_BULLET_DELETE: {
                    DeleteBullet(packet.Pop_UInt32());
                    break;
                }
        }
    }
}
