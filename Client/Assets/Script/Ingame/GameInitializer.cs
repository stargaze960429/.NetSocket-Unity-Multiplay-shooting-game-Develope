using MyNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : PacketHandle {

    [SerializeField]
    GameObject prefabPlayer;

    List<GameObject> players =  new List<GameObject>();

    [SerializeField]
    GameObject mainCanvas;

    [SerializeField]
    PlayerPosUpdater gameupdater;

    [SerializeField]
    InGameCamera gameCam;

    [SerializeField]
    InputManager inputManager;

    [SerializeField]
    HUDUpdater hudUpdater;

    [SerializeField]
    BulletUpdater bulletUpdater;

    [SerializeField]
    RankUpdater rankUpdater;
 

    protected override void OnMessage(Packet packet) {
        switch (packet.Head) {
            case Packet.HEADER.GAME_INIT_PLAYER: {
                    float y = packet.Pop_Float();
                    float x = packet.Pop_Float();
                    int nickSize = packet.Pop_Int32();
                    string nickname = packet.Pop_String(nickSize);
                    CreatePlayer(nickname, new Vector3(x, y));
                    break;
                }
            case Packet.HEADER.GAME_INIT_ALL_PACKET_SENDED: {
                    MyNetUnityClient.Instance.Send(new Packet(Packet.HEADER.REPLY_GAME_INIT_COMPLETE));
                    break;
                }
            case Packet.HEADER.GAME_SIMULATION_START: {
                    //게임 시작
                    mainCanvas.SetActive(false);
                    gameupdater.SetPlayers(players);
                    gameupdater.gameObject.SetActive(true);
                    hudUpdater.Initialize();
                    bulletUpdater.gameObject.SetActive(true);
                    gameCam.gameObject.SetActive(true);
                    break;
                }
        }
    }

    void CreatePlayer(string nickname, Vector3 pos) {
        GameObject player = Instantiate(prefabPlayer, Vector3.zero, Quaternion.identity, this.transform) as GameObject;
        player.transform.position = pos;
        player.name = nickname;
        players.Add(player);
        hudUpdater.AddConnectedUserNicknamelabel(nickname);
        rankUpdater.AddNewPlayer(nickname);
        

        if (player.name == Storage.Table["CurrentNickname"] as string)
        {
            Storage.GameObjects.Add("CurrentPlayerCharacter", player);
        }
        inputManager.gameObject.SetActive(true);
    }
}
