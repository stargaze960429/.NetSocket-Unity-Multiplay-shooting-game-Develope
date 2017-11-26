using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyNet;


public class UIMatchmaking : PacketHandle {

    [SerializeField]
    GameObject game;

    [SerializeField]
    GameObject[] circles;

    private static readonly Packet request_matching = new Packet(Packet.HEADER.REQUEST_MATCHMAKE);
    private static readonly Packet cancel_matching = new Packet(Packet.HEADER.CANCEL_MATCHMAKE);
    private static readonly Packet reply_complete_matching = new Packet(Packet.HEADER.REPLY_COMPLETE_MATCHMAKE);

    private bool matching;

    [SerializeField]
    Button matchingbutton;

    [SerializeField]
    Text matchingStateLabel;

    Text matchingButtonText;

    private void Start()
    {
        matchingButtonText = matchingbutton.transform.Find("Text").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (matching)
        {
            circles[0].transform.Rotate(Vector3.forward * 100.0f * Time.deltaTime, Space.Self);
            circles[1].transform.Rotate(Vector3.back * 125.0f * Time.deltaTime, Space.Self);
            circles[2].transform.Rotate(Vector3.forward * 200.0f * Time.deltaTime, Space.Self);
        }
    }

    public void OnMatchmakeButton() {
        if (matching)
        {
            MyNetUnityClient.Instance.Send(cancel_matching);
        }
        else {
            MyNetUnityClient.Instance.Send(request_matching);
        }
    }

    protected override void OnMessage(Packet packet) {
        switch (packet.Head) {
            case Packet.HEADER.ALLOW_MATCHMAKE: {
                    matching = true;
                    matchingButtonText.text = "Cancel";
                    matchingStateLabel.text = "게임을 검색 중...";
                    break;
                }
            case Packet.HEADER.ALLOW_CANCEL_MATCHMAKE: {
                    matching = false;
                    matchingButtonText.text = "Start";
                    matchingStateLabel.text = "게임 검색이 취소되었습니다.";
                    break;
                }
            case Packet.HEADER.COMPLETE_MATCHMAKE: {
                    matchingbutton.interactable = false;
                    matchingStateLabel.text = "게임 준비 완료.";
                    game.SetActive(true);
                    MyNetUnityClient.Instance.Send(reply_complete_matching);
                    break;
                }
            case Packet.HEADER.GAME_INIT_START: {
                    matchingStateLabel.text = "게임 로딩중...";
                    break;
                }
            case Packet.HEADER.SUDDENLY_QUIT_COMPLETED_MATCH: {
                    matchingbutton.interactable = true;
                    matchingStateLabel.text = "하나 이상의 플레이어가 준비되지않아 연결이 종료되었습니다. 다시 시도하십시오.";
                    matchingButtonText.text = "Start";
                    matching = false;
                    game.SetActive(false);
                    break;
                }
        }
    }
}
