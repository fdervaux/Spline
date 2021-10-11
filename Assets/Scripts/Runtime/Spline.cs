using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spline : MonoBehaviour
{
    public Vector3[] points; 


    public Vector3 computeSplinePoint(float t)
    {
        return Vector3.zero;

        
    }

    public static Vector3 computeBezierPoint(Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3, float t )
    {
        Vector3 p01 = Vector3.Lerp(p0,p1,t);
        Vector3 p12 = Vector3.Lerp(p1,p2,t);
        Vector3 p23 = Vector3.Lerp(p2,p3,t);

        

        Vector3 p01_12 = Vector3.Lerp(p01,p12,t);
        
        Vector3 p12_23 = Vector3.Lerp(p12,p23,t);

        Vector3 p = Vector3.Lerp(p01_12,p12_23,t); 

        return p;  
    }
}
