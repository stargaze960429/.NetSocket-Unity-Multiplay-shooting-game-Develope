using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyNet;
using MyNet.Shared_GameLogic;

/// <summary>
/// 주기적으로 플레이어의 입력을 받아서 제어
/// </summary>
public class InputManager : MonoBehaviour {

    [SerializeField]
    Camera gameCam;

    private void FixedUpdate()
    {
        KeyInputSnapshot snapshot = MakeInputSnapshot();

        Vector3 direction = Input.mousePosition - gameCam.WorldToScreenPoint(Storage.GameObjects["CurrentPlayerCharacter"].transform.position);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Packet inputPacket = new Packet(Packet.HEADER.GAME_INPUT, Config.MAX_SESSION_BUFFER_SIZE);
        inputPacket.Push(snapshot);
        inputPacket.Push(angle);
        MyNetUnityClient.Instance.Send(inputPacket);
    }

    private KeyInputSnapshot MakeInputSnapshot() {

        KeyInputSnapshot input = new KeyInputSnapshot(false, false, false, false, false);

        if (Input.GetKey(KeyCode.W)) input.upKey = true;
        if (Input.GetKey(KeyCode.A)) input.leftKey = true;
        if (Input.GetKey(KeyCode.S)) input.downKey = true;
        if (Input.GetKey(KeyCode.D)) input.rightKey = true;
        if (Input.GetMouseButton(0)) input.mouseLeftDown = true;

        return input;
    }
}
