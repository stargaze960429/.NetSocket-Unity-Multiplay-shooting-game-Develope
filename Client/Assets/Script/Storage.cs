using MyNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Storage {

    /// <summary>
    /// 보간 주기
    /// 5를 더해준 이유는 보간주기가 패킷주기보다 커야 하기 때문이다. 멀티플레이 게임 개발 282쪽 참조
    /// </summary>
    public static readonly float interpolationPeriod = ((Config.GAME_UPDATE_PERIOD + 5) / 1000.0f);

    static Dictionary<string, object> table = new Dictionary<string, object>(100);

    static Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>(100);

    public static Dictionary<string, object> Table
    {
        get
        {
            return table;
        }
    }
    
    public static Dictionary<string, GameObject> GameObjects {
        get {
            return gameObjects;
        }
    }
}
