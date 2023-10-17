using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SplineControlPoint2D
{
    public enum Mode
    {
        CONSTRAINT,
        FREE,
        NONE
    }

    public Vector2[] controlPoints;
    public Mode mode;
}