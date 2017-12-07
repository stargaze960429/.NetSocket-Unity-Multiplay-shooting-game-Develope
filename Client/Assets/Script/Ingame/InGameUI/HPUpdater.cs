using MyNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPUpdater : PacketHandle {

    [SerializeField]
    Slider hpBar;

    [SerializeField]
    Image bloodyScreen;

    Coroutine bleedingCoroutine;

    int CurrentHP {
        get { return (int)hpBar.value; }
        set {
            if (value > 0)
            {
                if (value < hpBar.value) {
                    BloodScreen();
                }
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


    private void BloodScreen() {
        if (bleedingCoroutine != null) {
            StopCoroutine(bleedingCoroutine);
            bleedingCoroutine = null;
        }

        bleedingCoroutine = StartCoroutine(ScreenBleeding());
    }

    IEnumerator ScreenBleeding() {
        bloodyScreen.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        float alpha = 1.0f;
        float t = 4.0f;

        while (true) {
            alpha -= Time.fixedUnscaledDeltaTime * t;
            t += Time.fixedUnscaledDeltaTime * 5.0f;
            if (alpha < 0.0f)
            {
                bloodyScreen.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                yield break;
            }
            bloodyScreen.color = new Color(1.0f, 1.0f, 1.0f, alpha);
            
            yield return new WaitForFixedUpdate();
        }
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
