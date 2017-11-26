using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class InterpolateVector3
{
    public Vector3 To { get; private set; }
    public Vector3 From { get; private set; }

    public float NextTimeStamp { get; private set; }
    public float CurrentTimeStamp { get; private set; }

    public InterpolateVector3(Vector3 pos, float time)
    {
        this.To = pos;
        this.From = pos;

        this.CurrentTimeStamp = time;
        this.NextTimeStamp = Storage.interpolationPeriod;
    }

    public void SetNewVector3(Vector3 newVector) {
        float time = Time.time;

        this.From = To;
        this.To = newVector;
        this.NextTimeStamp = time + Storage.interpolationPeriod;
        this.CurrentTimeStamp = time;
    }

    public Vector3 Interpolated
    {
        get
        {
            float delta = this.NextTimeStamp - this.CurrentTimeStamp;
            float interpolatedDelta = (Time.time - this.CurrentTimeStamp) / delta;

            Vector3 momentum = this.To - this.From;

            Vector3 interpolatedMomentum = momentum * interpolatedDelta;

            float x = this.From.x + interpolatedMomentum.x;
            float y = this.From.y + interpolatedMomentum.y;
            float z = this.From.z + interpolatedMomentum.z;

            return new Vector3(x, y, z);
        }
    }
}

public class InterpolatedDegree {
    public float Next { get; private set; }
    public float Current { get; private set; }

    public float NextTimeStamp { get; private set; }
    public float CurrentTimeStamp { get; private set; }

    public void SetNewDegree(float newDegree) {
        float time = Time.time;

        this.Current = this.Next;
        this.Next = newDegree;
        this.CurrentTimeStamp = time;
        this.NextTimeStamp = time + Storage.interpolationPeriod;
    }

    public InterpolatedDegree(float value, float time) {
        this.Next = value;
        this.Current = value;

        this.NextTimeStamp = time + Storage.interpolationPeriod;
        this.CurrentTimeStamp = time;
    }

    public float InterpolatedAngle {
        get {
            float delta = this.NextTimeStamp - this.CurrentTimeStamp;
            float interpolatedDelta = (Time.time - this.CurrentTimeStamp) / delta;

            float adjustAngle = (this.Next - this.Current);

            adjustAngle = adjustAngle > 180.0f ? adjustAngle - 360.0f : adjustAngle;
            adjustAngle = adjustAngle < -180.0f ? adjustAngle + 360.0f : adjustAngle;

            float interPolatedAngle = (adjustAngle * interpolatedDelta) + this.Current;

            return interPolatedAngle;
        }
    }
} 
