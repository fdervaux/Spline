using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SplineBest : MonoBehaviour
{
    [SerializeField] private List<SplineControlPoint> controlPointsList = new List<SplineControlPoint>();

    private const int _nbPointsToComputeLength = 1000;
    private float[] _lengths = new float[_nbPointsToComputeLength];

    public Vector3 computePoint(float t)
    {
        if (t == 1)
            return controlPointsList[controlPointsList.Count - 1].controlPoints[1];

        float totalFactor = t * (controlPointsList.Count - 1);
        int curveIndex = (int)Mathf.Floor(totalFactor);
        float curveFactor = totalFactor - curveIndex;

        Vector3 p1 = controlPointsList[curveIndex].controlPoints[1];
        Vector3 p2 = controlPointsList[curveIndex].controlPoints[2];
        Vector3 p3 = controlPointsList[curveIndex + 1].controlPoints[0];
        Vector3 p4 = controlPointsList[curveIndex + 1].controlPoints[1];

        Vector3 p12 = Vector3.Lerp(p1, p2, curveFactor);
        Vector3 p23 = Vector3.Lerp(p2, p3, curveFactor);
        Vector3 p34 = Vector3.Lerp(p3, p4, curveFactor);

        Vector3 p1223 = Vector3.Lerp(p12, p23, curveFactor);
        Vector3 p2334 = Vector3.Lerp(p23, p34, curveFactor);

        return Vector3.Lerp(p1223, p2334, curveFactor);
    }

    public void computeLengths()
    {
        _lengths = new float[_nbPointsToComputeLength];
        Vector3 lastPoint = controlPointsList[0].controlPoints[1];

        float length = 0;
        for (int i = 1; i <= _nbPointsToComputeLength; i++)
        {
            Vector3 point = computePoint( (float) i / _nbPointsToComputeLength);
            length += (lastPoint - point).magnitude;
            _lengths[i-1] = length;

            lastPoint = point;
        }
    }

    public float length()
    {
        return _lengths[_nbPointsToComputeLength - 1];
    }

    public Vector3 computePointWithLength(float distance)
    {
        int goodIndex = 0;

        if(distance > length())
            return controlPointsList[controlPointsList.Count - 1].controlPoints[1];

        for (int i = 0; i < _nbPointsToComputeLength; i++)
        {
            if( distance < _lengths[i])
            {
                goodIndex = i;
                break;
            }
        }

        float factor = (float) goodIndex / _nbPointsToComputeLength;
        return computePoint(factor);
    }




}