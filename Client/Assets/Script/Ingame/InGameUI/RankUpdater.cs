using System;
using System.Collections;
using System.Collections.Generic;
using MyNet;
using UnityEngine;
using UnityEngine.UI;

public class RankUpdater : PacketHandle {

    //Top1 플레이어를 표시하는 색은 해당 텍스트레이블에서 직접 가져옴.
    [SerializeField]
    Color top2Color;
    [SerializeField]
    Color top3Color;
    [SerializeField]
    Color defaultRankColor;


    [SerializeField]
    Text top1PlayerRankLabel;
    [SerializeField]
    Text top1PlayerNameLabel;
    [SerializeField]
    Text top1PlayerKillCountLabel;
    int top1PlayerKillCount = 0;

    [SerializeField]
    Text currentPlayerRankLabel;
    [SerializeField]
    Text currentPlayerKillCountLabel;
    [SerializeField]
    Text currentPlayerNameLabel;

             //닉네임, 킬수
    Dictionary<string, int> playerScores = new Dictionary<string, int>(Config.USER_COUNT_PER_ONE_GAME);

    protected override void OnEnable()
    {
        base.OnEnable();
        currentPlayerNameLabel.text = Storage.Table["CurrentNickname"] as string;
    }

    public void AddNewPlayer(string nickname) {
        playerScores.Add(nickname, 0);
    }

    private void UpdateCurrentPlayerLabel() {
        currentPlayerKillCountLabel.text = playerScores[Storage.Table["CurrentNickname"] as string].ToString();

        int rank = 1;

        var iter = playerScores.GetEnumerator();

        while (iter.MoveNext())
        {
            if (iter.Current.Value > playerScores[Storage.Table["CurrentNickname"] as string])
            {
                rank++;
            }
        }

        currentPlayerRankLabel.text = rank.ToString();

        switch (rank)
        {
            case 1:
                {
                    currentPlayerRankLabel.color = top1PlayerRankLabel.color;
                    currentPlayerKillCountLabel.color = top1PlayerRankLabel.color;
                    break;
                }
            case 2:
                {
                    currentPlayerRankLabel.color = top2Color;
                    currentPlayerKillCountLabel.color = top2Color;
                    break;
                }
            case 3:
                {
                    currentPlayerRankLabel.color = top3Color;
                    currentPlayerKillCountLabel.color = top3Color;
                    break;
                }
            default:
                {
                    currentPlayerRankLabel.color = defaultRankColor;
                    currentPlayerKillCountLabel.color = defaultRankColor;
                    break;
                }
        }
    }


    protected override void OnMessage(Packet packet)
    {
        switch (packet.Head) {
            case Packet.HEADER.GAME_UPDATE_PLAYER_DIE:
                {
                    Packet recved = packet.Clone() as Packet;
                    string killer = recved.Pop_String(recved.Pop_Int32());
                    playerScores[killer]++;

                    if (playerScores[killer] > top1PlayerKillCount)
                    {
                        top1PlayerNameLabel.text = killer;
                        top1PlayerKillCount = playerScores[killer];
                        top1PlayerKillCountLabel.text = top1PlayerKillCount.ToString();
                    }

                    UpdateCurrentPlayerLabel();
                    break;
                }
        }
    }
}
