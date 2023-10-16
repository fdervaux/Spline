using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Orientation
{
    public Vector3 forward;
    public Vector3 upward;
    public Vector3 right;
}


public struct RMFAndLength
{
    public Vector3 origin;
    public Vector3 xAxis;
    public Vector3 yAxis;
    public float length;
}

public struct BezierInfo
{
    public Vector3 p0;
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;

    public float angle0;
    public float angle1;

    public float t;


}

public class Spline : MonoBehaviour
{
    [SerializeField, HideInInspector] private List<SplineControlPoint> controlPointsList = new List<SplineControlPoint>();

    private const int _nbPointsToComputeLength = 2000;
    private RMFAndLength[] _RMFAndLengths = new RMFAndLength[_nbPointsToComputeLength];

    public List<SplineControlPoint> ControlPointsList { get => controlPointsList; }

    public SplineControlPoint getControlPoint(int index)
    {
        return controlPointsList[index];
    }

    public void setControlPoint(int index, SplineControlPoint controlPoint)
    {
        controlPointsList[index] = controlPoint;
    }

    public void removeControlPoint(int index)
    {
        controlPointsList.RemoveAt(index);
    }

    private void Awake()
    {
        ComputeRMFAndLengths();
    }

    private BezierInfo getCurrentBezierPoint(float t)
    {
        float totalFactor = t * (controlPointsList.Count - 1);
        int curveIndex = (int)Mathf.Floor(totalFactor);
        float curveFactor = totalFactor - curveIndex;

        BezierInfo bezierInfo;
        bezierInfo.p0 = controlPointsList[curveIndex].controlPoints[1];
        bezierInfo.p1 = controlPointsList[curveIndex].controlPoints[2];
        bezierInfo.p2 = controlPointsList[curveIndex + 1].controlPoints[0];
        bezierInfo.p3 = controlPointsList[curveIndex + 1].controlPoints[1];

        bezierInfo.angle0 = controlPointsList[curveIndex].angle;
        bezierInfo.angle1 = controlPointsList[curveIndex + 1].angle;


        bezierInfo.t = totalFactor - curveIndex;
        return bezierInfo;
    }

    public Vector3 computeVelocity(float t)
    {
        if (t == 1)
            t -= 0.001f;

        BezierInfo bezierInfo = getCurrentBezierPoint(t);

        float tsquare = bezierInfo.t * bezierInfo.t;

        return bezierInfo.p0 * (-3 * tsquare + 6 * bezierInfo.t - 3)
        + bezierInfo.p1 * (9 * tsquare - 12 * bezierInfo.t + 3)
        + bezierInfo.p2 * (-9 * tsquare + 6 * bezierInfo.t)
        + bezierInfo.p3 * 3 * tsquare;
    }

    public Vector3 computeAcceleration(float t)
    {
        if (t == 1)
            t -= 0.001f;

        BezierInfo bezierInfo = getCurrentBezierPoint(t);

        float tsquare = bezierInfo.t * bezierInfo.t;

        return bezierInfo.p0 * (-6 * bezierInfo.t + 6)
        + bezierInfo.p1 * (18 * bezierInfo.t - 12)
        + bezierInfo.p2 * (-18 * bezierInfo.t + 6)
        + bezierInfo.p3 * 6 * bezierInfo.t;
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

    public Orientation computeOrientationWithRMF(float t, bool withAngle = true)
    {
        if (t == 1)
            t -= 0.001f;


        int t1 = (int)Mathf.Floor(t * (_nbPointsToComputeLength - 1));
        int t2 = t1+1;

        float factor = t * (_nbPointsToComputeLength - 1) - t1;

        Vector3 right = Vector3.Lerp(_RMFAndLengths[t1].xAxis, _RMFAndLengths[t2].xAxis, factor);
        Vector3 upward = -Vector3.Lerp(_RMFAndLengths[t1].yAxis, _RMFAndLengths[t2].yAxis, factor);
        Vector3 forward = Vector3.Cross(upward, right);

        Orientation orientation;

        BezierInfo bezierInfo = getCurrentBezierPoint(t);
        float angle = Mathf.Lerp(bezierInfo.angle0, bezierInfo.angle1, bezierInfo.t);

        if(!withAngle)
        {
            orientation.forward = forward;
            orientation.upward = upward;
            orientation.right = right;
            return orientation;
        }

        orientation.forward = forward;
        orientation.upward = Quaternion.AngleAxis(angle, forward) * upward;
        orientation.right = Vector3.Cross(forward, orientation.upward);

        return orientation;
    }

    public Orientation computeOrientation(float t, Vector3 baseAxis)
    {
        if (t == 1)
            t -= 0.001f;




        BezierInfo bezierInfo = getCurrentBezierPoint(t);

        float angle = Mathf.Lerp(bezierInfo.angle0, bezierInfo.angle1, bezierInfo.t);

        float tsquare = bezierInfo.t * bezierInfo.t;

        Vector3 velocity = bezierInfo.p0 * (-3 * tsquare + 6 * bezierInfo.t - 3)
        + bezierInfo.p1 * (9 * tsquare - 12 * bezierInfo.t + 3)
        + bezierInfo.p2 * (-9 * tsquare + 6 * bezierInfo.t)
        + bezierInfo.p3 * 3 * tsquare;

        Vector3 acc = bezierInfo.p0 * (-6 * bezierInfo.t + 6)
        + bezierInfo.p1 * (18 * bezierInfo.t - 12)
        + bezierInfo.p2 * (-18 * bezierInfo.t + 6)
        + bezierInfo.p3 * 6 * bezierInfo.t;

        Orientation orientation;

        orientation.forward = velocity.normalized;

        Vector3 side = Vector3.Cross(baseAxis, orientation.forward).normalized;


        orientation.upward = Vector3.Cross(orientation.forward, side).normalized;
        orientation.upward = Quaternion.AngleAxis(angle, orientation.forward) * orientation.upward;

        orientation.right = Vector3.Cross(orientation.forward, orientation.upward);


        return orientation;
    }

    public void ComputeRMFAndLengths()
    {
        _RMFAndLengths = new RMFAndLength[_nbPointsToComputeLength];

        if (controlPointsList.Count < 2)
            return;

        Vector3 lastPoint = controlPointsList[0].controlPoints[1];


        RMFAndLength firstRMFAndLength;

        firstRMFAndLength.origin = lastPoint;
        Vector3 lastTangent = computeVelocity(0);
        Vector3 normal = Vector3.Cross(lastTangent, Vector3.up).normalized;
        firstRMFAndLength.yAxis = Vector3.Cross(lastTangent, normal).normalized;
        firstRMFAndLength.xAxis = Vector3.Cross(firstRMFAndLength.yAxis, lastTangent).normalized;
        firstRMFAndLength.length = 0;
        _RMFAndLengths[0] = firstRMFAndLength;


        for (int i = 1; i < _nbPointsToComputeLength; i++)
        {
            Vector3 point = computePoint((float)i / _nbPointsToComputeLength);
            Vector3 tangent = computeVelocity((float)i / _nbPointsToComputeLength);

            Vector3 v1 = point - lastPoint;
            float c1 = Vector3.Dot(v1, v1);

            Vector3 rLi = _RMFAndLengths[i - 1].xAxis - (2 / c1) * Vector3.Dot(v1, _RMFAndLengths[i - 1].xAxis) * v1;

            Vector3 tLi = lastTangent - (2 / c1) * Vector3.Dot(v1, lastTangent) * v1;

            Vector3 v2 = tangent - tLi;
            float c2 = Vector3.Dot(v2, v2);

            Vector3 rNext = rLi - (2 / c2) * Vector3.Dot(v2, rLi) * v2;
            Vector3 sNext = Vector3.Cross(tangent, rNext).normalized;


            RMFAndLength rmfAndLength;
            rmfAndLength.origin = point;
            rmfAndLength.xAxis = rNext;
            rmfAndLength.yAxis = sNext;
            rmfAndLength.length = _RMFAndLengths[i - 1].length + (lastPoint - point).magnitude;

            _RMFAndLengths[i] = rmfAndLength;

            lastPoint = point;
            lastTangent = tangent;
        }


    }

    public float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    private float getTFactorWithDistance(float distance)
    {
        int goodIndex = 0;

        if (distance > length())
            return 1f;

        for (int i = 0; i <= _nbPointsToComputeLength; i++)
        {
            if (distance < _RMFAndLengths[i].length)
            {
                goodIndex = i;
                break;
            }

        }

        if (goodIndex == 0)
        {
            return 0;
        }

        int lastindex = goodIndex - 1;
        float factor = Remap(distance, _RMFAndLengths[lastindex].length, _RMFAndLengths[goodIndex].length, 0, 1);
        return (lastindex + factor) / (_nbPointsToComputeLength-1);
    }

    public Vector3 computeVelocityWithLength(float distance)
    {
        return computeVelocity(getTFactorWithDistance(distance));
    }

    public Orientation computeOrientationWithRMFWithLength(float distance)
    {
        return computeOrientationWithRMF(getTFactorWithDistance(distance));
    }

    public Orientation computeOrientationWithLength(float distance, Vector3 baseAxis)
    {
        return computeOrientation(getTFactorWithDistance(distance), baseAxis);
    }

    public Vector3 computePointWithLength(float distance)
    {
        return computePoint(getTFactorWithDistance(distance));
    }

    public float length()
    {
        return _RMFAndLengths[_nbPointsToComputeLength - 1].length;
    }





}