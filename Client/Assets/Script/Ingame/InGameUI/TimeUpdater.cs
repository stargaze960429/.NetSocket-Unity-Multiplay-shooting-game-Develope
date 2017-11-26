using MyNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeUpdater : PacketHandle {

    [SerializeField]
    Text timelabel;

    protected override void OnMessage(Packet packet) {
        switch (packet.Head) {
            case Packet.HEADER.GAME_TIME: {
                    TimeRenew(packet.Pop_Int32());
                    break;
                }
        }
    }

    private void TimeRenew(int totalSeconds) {
        int minute = totalSeconds / 60;
        int second = totalSeconds % 60;
         
        timelabel.text = string.Format("{0} : {1}", minute, second);
    }
}
