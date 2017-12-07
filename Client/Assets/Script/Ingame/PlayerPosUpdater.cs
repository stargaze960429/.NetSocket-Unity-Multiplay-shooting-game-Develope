using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyNet;
using MyNet.Shared_GameLogic;


public class PlayerPosUpdater : PacketHandle
{
    public class PlayerInterpolation
    {
        public InterpolateVector3 Position { get; set; }
        public InterpolatedDegree Rotation { get; set; }

        public string Nickname { get; set; }
        public GameObject gameObject { get; set; }

        public PlayerInterpolation(string nickname, GameObject gObj, Vector3 position, float rotation, float time)
        {
            this.Nickname = nickname;
            this.gameObject = gObj;
            this.Position = new InterpolateVector3(position, time);
            this.Rotation = new InterpolatedDegree(rotation, time);
        }
    }

    List<PlayerInterpolation> playerInterpolList;

    [SerializeField]
    InGameCamera gameCam;

    public void SetPlayers(List<GameObject> list) {

        playerInterpolList = new List<PlayerInterpolation>(list.Count);

        var playerIter = list.GetEnumerator();

        float time = Time.time;

        while (playerIter.MoveNext()) {
            PlayerInterpolation playerInterpol = new PlayerInterpolation(playerIter.Current.name, playerIter.Current.gameObject , playerIter.Current.transform.position, 0.0f ,time);
            playerInterpol.gameObject = playerIter.Current;
            playerInterpolList.Add(playerInterpol);

            if (playerIter.Current.name == Storage.Table["CurrentNickname"] as string)
            { //내가 보낸 닉네임과 일치하는 플레이어캐릭터면 내 캐릭터
                gameCam.target = playerInterpol.Position;
                gameCam.transform.Translate(0.0f, 0.0f, -2.0f); //카메라가 내 캐릭터 따라다니도록 설정
            }
        }
    }

    private void Update()
    {

        var iter = this.playerInterpolList.GetEnumerator();

        while (iter.MoveNext())
        {
            Vector3 pos = iter.Current.Position.Interpolated;

            if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y))
            {
                iter.Current.gameObject.transform.position = pos;
            }

            float interPolatedAngle = iter.Current.Rotation.InterpolatedAngle;

            if (!float.IsNaN(interPolatedAngle)) {
                iter.Current.gameObject.transform.rotation = Quaternion.AngleAxis(interPolatedAngle, Vector3.forward);
            }
        }
    }

    protected override void OnMessage(Packet packet)
    {
        switch (packet.Head) {
            case Packet.HEADER.GAME_UPDATE_PLAYER_POSITION:
                {
                    string nickname;
                    float x, y;
                    float rotation;

                    rotation = packet.Pop_Float();
                    y = packet.Pop_Float();
                    x = packet.Pop_Float();
                    nickname = packet.Pop_String(packet.Pop_Int32());

                    for (int i = 0; i < playerInterpolList.Count; i++) {
                        if (playerInterpolList[i].Nickname == nickname) {

                            playerInterpolList[i].gameObject.transform.position = playerInterpolList[i].Position.To;
                            playerInterpolList[i].gameObject.transform.rotation = Quaternion.AngleAxis(playerInterpolList[i].Rotation.Next, Vector3.forward);

                            playerInterpolList[i].Rotation.SetNewDegree(rotation);
                            playerInterpolList[i].Position.SetNewVector3(new Vector3(x, y, 0.0f));
                            break;
                        }
                    }
                    break;
                }
        }
    }
}

