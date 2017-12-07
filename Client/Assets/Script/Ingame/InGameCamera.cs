using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameCamera : MonoBehaviour {

    public float dampTime = 0.15f;
    private Vector3 velocity = Vector3.zero;
    public InterpolateVector3 target;

    private Camera thisCam;

    private void Start()
    {
        thisCam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            Vector3 point = thisCam.WorldToViewportPoint(target.To);
            Vector3 delta = target.To - thisCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
            Vector3 destination = transform.position + delta;
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
        }

    }
}
