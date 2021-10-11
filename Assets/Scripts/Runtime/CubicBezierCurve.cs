using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubicBezierCurve : MonoBehaviour
{
    public Vector3[] controlPoints = new Vector3[4];

    public Vector3 computeBezierPoint(float t)
    {
        Vector3 p01 = Vector3.Lerp(controlPoints[0],controlPoints[1],t);
        Vector3 p12 = Vector3.Lerp(controlPoints[1],controlPoints[2],t);
        Vector3 p23 = Vector3.Lerp(controlPoints[2],controlPoints[3],t);

        Vector3 p01_12 = Vector3.Lerp(p01,p12,t);
        
        Vector3 p12_23 = Vector3.Lerp(p12,p23,t);

        Vector3 p = Vector3.Lerp(p01_12,p12_23,t); 

        return p;  
    }
}
