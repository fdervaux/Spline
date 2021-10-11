using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{
    //public Vector3 p0,p1,p2;
    public Vector3[] controlPoints = new Vector3[3];

    public Vector3 computeBezierPoint(float t)
    {
        Vector3 p01 = Vector3.Lerp(controlPoints[0],controlPoints[1],t);
        Vector3 p12 = Vector3.Lerp(controlPoints[1],controlPoints[2],t);

        Vector3 p = Vector3.Lerp(p01,p12,t); 

        return p;  
    }
}
