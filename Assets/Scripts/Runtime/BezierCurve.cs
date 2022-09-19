using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{
    //public Vector3 p0,p1,p2;
    public Vector3[] controlPoints = new Vector3[3];

    public Vector3 computeBezierPoint(float t)
    {
        Vector3[] points = controlPoints;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3[] resPoints = new Vector3[controlPoints.Length - 1];
            for (int j = 0; j < points.Length - 1; j++)
            {
                resPoints[j] = Vector3.Lerp(points[j], points[j + 1], t);
            }
            points = resPoints;
        }

        return points[0];  
    }
}
