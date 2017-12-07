using System;
using System.Collections;
using System.Collections.Generic;
using MyNet;
using UnityEngine;

public class GameResultUpdater : PacketHandle {

    [SerializeField]
    ResultLabel resultLabelPrefab;

    [SerializeField]
    Transform top;

    [SerializeField]
    float interval;


    protected override void OnMessage(Packet packet)
    {
        switch (packet.Head) {
            case Packet.HEADER.GAME_RESULT_RANK: {
                    int playerCount = packet.Pop_Int32();

                    List<ResultLabel> resultLabels = new List<ResultLabel>(playerCount);

                    for (int i = 0; i < playerCount; i++) {
                        ResultLabel label = Instantiate(resultLabelPrefab, Vector3.zero, Quaternion.identity, this.transform);
                        label.transform.localPosition = new Vector3(0.0f, top.localPosition.y - (interval * i), 0.0f);
                        resultLabels.Add(label);
                    }

                    for (int i = resultLabels.Count; i > 0; i--) {
                        resultLabels[i - 1].Rank.text = i.ToString();
                        resultLabels[i - 1].Score.text = packet.Pop_Int32().ToString();
                        resultLabels[i - 1].Nickname.text = packet.Pop_String(packet.Pop_Int32());
                    }

                    break;
                }
        }
    }
}
