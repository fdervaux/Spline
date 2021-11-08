using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BezierCurveN : MonoBehaviour
{
    [Range(2, 10)]
    public int n = 3;
    
    public Vector3[] controlPoints;

    private void Awake()
    {
        controlPoints = new Vector3[n];
    }


    public Vector3 computeBezierPoint(float t)
    {
        Vector3 p01 = Vector3.Lerp(controlPoints[0], controlPoints[1], t);
        Vector3 p12 = Vector3.Lerp(controlPoints[1], controlPoints[2], t);

        Vector3 p = Vector3.Lerp(p01, p12, t);

        return p;
    }
}