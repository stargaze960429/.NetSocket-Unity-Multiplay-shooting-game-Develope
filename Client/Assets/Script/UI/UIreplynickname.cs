using MyNet;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIreplynickname : PacketHandle {

    private static readonly Packet confirm_nickname = new Packet(Packet.HEADER.REQUEST_CONFIRM_NICKNAME);

    [SerializeField]
    InputField inputNickname;

    [SerializeField]
    GameObject matchMakeUI;

    [SerializeField]
    Button confrimNicknameButton;

    [SerializeField]
    Text errorLabel;

    string tryRequestedName;


    public void OnVerifyButton() {
        Packet replyNickname = new Packet(Packet.HEADER.REPLY_NICKNAME, Encoding.ASCII.GetByteCount(inputNickname.text));
        replyNickname.Push(inputNickname.text);
        tryRequestedName = inputNickname.text;
        MyNetUnityClient.Instance.Send(replyNickname);
    }

    protected override void OnMessage(Packet packet) {
        switch (packet.Head) {
            case Packet.HEADER.APPLY_NICKNAME: {
                    // 매치메이킹 화면으로 넘어가기
                    errorLabel.text = "사용 가능한 닉네임 입니다.";
                    confrimNicknameButton.interactable = true;
                    break;
                }
            case Packet.HEADER.WRONG_NICKNAME: {
                    errorLabel.text = "잘못된 닉네임 입니다.(6~20자, 영문과 숫자만 허용)";
                    confrimNicknameButton.interactable = false;
                    break;
                }
            case Packet.HEADER.ALLOW_CONFIRM_NICKNAME: {
                    Storage.Table.Add("CurrentNickname", tryRequestedName);
                    this.matchMakeUI.SetActive(true);
                    this.gameObject.SetActive(false);
                    break;
                }
        }
    }

    public void OnConfirmButton() {
        MyNetUnityClient.Instance.Send(confirm_nickname);
    }
}
