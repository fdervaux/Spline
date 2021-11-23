using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SplineControlPoint
{
    public enum Mode
    {
        CONSTRAINT,
        FREE,
        NONE
    }

    public Vector3[] controlPoints;
    public Mode mode;

    public float angle;

    [HideInInspector] public Vector3 UpAxis;
}