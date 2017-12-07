using MyNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageUpdater : PacketHandle {

    [SerializeField]
    Text gameMessage;

    Coroutine showMessageCoroutine;

    protected override void OnEnable()
    {
        base.OnEnable();
        this.gameMessage.text = "";
    }

    public void ShowMessage(string msg) {
        if (showMessageCoroutine != null)
        {
            StopCoroutine(showMessageCoroutine);
        }
        showMessageCoroutine = StartCoroutine(ShowMessageCoroutine(msg));
    }

    IEnumerator ShowMessageCoroutine(string msg) {
        gameMessage.text = msg;
        yield return new WaitForSecondsRealtime(1.0f);
        this.showMessageCoroutine = null;
        gameMessage.text = "";
    }

    protected override void OnMessage(Packet packet)
    {
        switch (packet.Head)
        {
            case Packet.HEADER.GAME_MESSAGE:
                {
                    ShowMessage(packet.Pop_String(packet.Pop_Int32()));
                    break;
                }
        }
    }
}
