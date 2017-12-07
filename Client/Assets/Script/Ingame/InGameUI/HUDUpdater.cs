using MyNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDUpdater : MonoBehaviour {

    [SerializeField]
    Text prefabConnectedUserLabel;

    [SerializeField]
    GameObject connectedUserLabelParent;

    List<Text> usernicknames = new List<Text>(Config.USER_COUNT_PER_ONE_GAME);
    private readonly Vector3 TermConnectedUserLabel = new Vector3(0.0f, -28.0f, 0.0f);

    [SerializeField]
    Text playerNickname;

    Transform playerCharacterTr;

    private readonly Vector3 relativeNickNamePos = new Vector3(0.0f,0.9f,0.0f); //캐릭터 위치에서 닉네임의 상대위치

    public void AddConnectedUserNicknamelabel(string nickname)
    {
        Text label = Instantiate(prefabConnectedUserLabel, Vector3.zero, Quaternion.identity, connectedUserLabelParent.transform);

        label.transform.SetParent(connectedUserLabelParent.transform);
        label.text = nickname;
        label.transform.localPosition = (TermConnectedUserLabel * (usernicknames.Count + 1));

        this.usernicknames.Add(label);
    }

    public void Initialize()
    {
        playerCharacterTr = Storage.GameObjects["CurrentPlayerCharacter"].transform;
        playerNickname.text = playerCharacterTr.gameObject.name;
        this.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update () {
        playerNickname.transform.position = playerCharacterTr.position + relativeNickNamePos;
	}
}
