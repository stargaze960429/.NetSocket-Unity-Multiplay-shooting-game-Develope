using MyNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPUpdater : PacketHandle {

    [SerializeField]
    Slider hpBar;

    int CurrentHP {
        get { return (int)hpBar.value; }
        set {
            if (value > 0)
            {
                hpBar.value = value;
            }
            else {
                hpBar.value = 0;
            }
        }
    }

    int MaxHP {
        get { return (int)hpBar.maxValue; }
        set { hpBar.maxValue = value; }
    }


    protected override void OnMessage(Packet packet) {
        switch (packet.Head) {
            case Packet.HEADER.GAME_UPDATE_OWN_HP: {
                    this.CurrentHP = packet.Pop_Int32();
                    break;
            }
            case Packet.HEADER.GAME_UPDATE_OWN_MAXHP: {
                    this.MaxHP = packet.Pop_Int32();
                    break;
            }
        }
    }
}
