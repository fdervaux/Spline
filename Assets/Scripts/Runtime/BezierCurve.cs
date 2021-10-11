using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{
    public Vector3 p0,p1,p2;

    public Vector3 computeBezierPoint(float t)
    {
        Vector3 p01 = Vector3.Lerp(p0,p1,t);
        Vector3 p12 = Vector3.Lerp(p1,p2,t);

        Vector3 p = Vector3.Lerp(p01,p12,t); 

        return p;  
    }
}
