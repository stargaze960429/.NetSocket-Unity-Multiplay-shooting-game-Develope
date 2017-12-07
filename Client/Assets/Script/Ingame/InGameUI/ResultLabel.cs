using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultLabel : MonoBehaviour {

    [SerializeField]
    Text rank;

    [SerializeField]
    Text nickname;

    [SerializeField]
    Text score;

	public Text Rank { get { return rank; } }
    public Text Nickname { get { return nickname; } }
    public Text Score { get { return score; } }
}
