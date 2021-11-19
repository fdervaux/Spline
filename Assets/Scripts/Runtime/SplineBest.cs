using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct BezierInfo
{
    public Vector3 p0;
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;

    public float t;
}

public class SplineBest : MonoBehaviour
{
    [SerializeField] private List<SplineControlPoint> controlPointsList = new List<SplineControlPoint>();

    private const int _nbPointsToComputeLength = 1000;
    private float[] _lengths = new float[_nbPointsToComputeLength];

    private void Awake()
    {
        computeLengths();
    }

    private BezierInfo getCurrentBezierPoint( float t)
    {
        float totalFactor = t * (controlPointsList.Count - 1);
        int curveIndex = (int)Mathf.Floor(totalFactor);
        float curveFactor = totalFactor - curveIndex;

        BezierInfo bezierInfo;
        bezierInfo.p0 = controlPointsList[curveIndex].controlPoints[1];
        bezierInfo.p1 = controlPointsList[curveIndex].controlPoints[2];
        bezierInfo.p2 = controlPointsList[curveIndex + 1].controlPoints[0];
        bezierInfo.p3 = controlPointsList[curveIndex + 1].controlPoints[1];

        bezierInfo.t = totalFactor - curveIndex;
        return bezierInfo;
    }

    public Vector3 computeVelocity(float t)
    {
        if (t == 1)
            return controlPointsList[controlPointsList.Count - 1].controlPoints[1];

        BezierInfo bezierInfo = getCurrentBezierPoint(t);

        float tsquare = bezierInfo.t * bezierInfo.t;

        return bezierInfo.p0 * (-3 * tsquare + 6 * bezierInfo.t - 3) 
        + bezierInfo.p1 * (9 * tsquare - 12 * bezierInfo.t + 3) 
        + bezierInfo.p2 * (-9 * tsquare + 6 * bezierInfo.t) 
        + bezierInfo.p3 * 3 * tsquare;
    }

    public Vector3 computePoint(float t)
    {
        if (t == 1)
            return controlPointsList[controlPointsList.Count - 1].controlPoints[1];

        BezierInfo bezierInfo = getCurrentBezierPoint(t);

        Vector3 p01 = Vector3.Lerp(bezierInfo.p0, bezierInfo.p1, bezierInfo.t);
        Vector3 p12 = Vector3.Lerp(bezierInfo.p1, bezierInfo.p2, bezierInfo.t);
        Vector3 p23 = Vector3.Lerp(bezierInfo.p2, bezierInfo.p3, bezierInfo.t);

        Vector3 p0112 = Vector3.Lerp(p01, p12, bezierInfo.t);
        Vector3 p1223 = Vector3.Lerp(p12, p23, bezierInfo.t);

        return Vector3.Lerp(p0112, p1223, bezierInfo.t);
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

    private float getTFactorWithDistance(float distance)
    {
        int goodIndex = 0;

        if (distance > length())
            return 1f;

        for (int i = 0; i < _nbPointsToComputeLength; i++)
        {
            if (distance < _lengths[i])
            {
                goodIndex = i;
                break;
            }
        }

        return (float)goodIndex / _nbPointsToComputeLength;
    }

    public Vector3 computeVelocityWithLength(float distance)
    {
        return computeVelocity(getTFactorWithDistance(distance));
    }

    public Vector3 computePointWithLength(float distance)
    {
        return computePoint(getTFactorWithDistance(distance));
    }

    public float length()
    {
        return _lengths[_nbPointsToComputeLength - 1];
    }




}