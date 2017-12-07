using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyNet;

public class UIbeginScreen : PacketHandle {

    [SerializeField]
    GameObject replyNicknameUI;

    public void OnGoButton() {
        MyNetUnityClient.Instance.Connect();
    }

    protected override void OnMessage(Packet packet)
    {
        if (packet.Head == Packet.HEADER.REQUEST_NICKNAME)
        { 
            replyNicknameUI.SetActive(true);
            MyNetUnityClient.Instance.StartHeartBeat();
            this.gameObject.SetActive(false);
        }
    }
}
