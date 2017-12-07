using MyNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 패킷을 처리하는 게임오브젝트들은 이것을 상속받음
/// 이유 : 메세지를 처리하는 코드가 기본적으로 이 형태를 띄기 때문에 중복을 방지하기 위해서
/// </summary>
public abstract class PacketHandle : MonoBehaviour {

    protected virtual void OnEnable() {
        MyNetUnityClient.Instance.onPacketReceive += OnMessage;
    }

    protected virtual void OnDisable() {
        MyNetUnityClient.Instance.onPacketReceive -= OnMessage;
    }

    protected abstract void OnMessage(Packet packet);
}
