using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public struct BezierInfo2D
{
    public Vector2 p0;
    public Vector2 p1;
    public Vector2 p2;
    public Vector2 p3;

    public float t;


}

public class Spline2D : MonoBehaviour
{

    [SerializeField, HideInInspector] private List<SplineControlPoint2D> controlPointsList = new List<SplineControlPoint2D>();

    private const int _nbPointsToComputeLength = 2000;

    private float[] _Lengths = new float[_nbPointsToComputeLength];

    public List<SplineControlPoint2D> ControlPointsList { get => controlPointsList; }

    public SplineControlPoint2D getControlPoint(int index)
    {
        return controlPointsList[index];
    }

    public void setControlPoint(int index, SplineControlPoint2D controlPoint)
    {
        controlPointsList[index] = controlPoint;
    }

    public void removeControlPoint(int index)
    {
        controlPointsList.RemoveAt(index);
    }

    private void Awake()
    {
        ComputeLengths();
    }

    private BezierInfo2D getCurrentBezierPoint(float t)
    {
        float totalFactor = t * (controlPointsList.Count - 1);
        int curveIndex = (int)Mathf.Floor(totalFactor);

        BezierInfo2D bezierInfo;
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
            t -= 0.001f;

        BezierInfo2D bezierInfo = getCurrentBezierPoint(t);

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

        BezierInfo2D bezierInfo = getCurrentBezierPoint(t);

        return bezierInfo.p0 * (-6 * bezierInfo.t + 6)
        + bezierInfo.p1 * (18 * bezierInfo.t - 12)
        + bezierInfo.p2 * (-18 * bezierInfo.t + 6)
        + bezierInfo.p3 * 6 * bezierInfo.t;
    }

    public Vector3 computePoint(float t)
    {
        if (t == 1)
            return controlPointsList[controlPointsList.Count - 1].controlPoints[1];

        BezierInfo2D bezierInfo = getCurrentBezierPoint(t);

        Vector3 p01 = Vector3.Lerp(bezierInfo.p0, bezierInfo.p1, bezierInfo.t);
        Vector3 p12 = Vector3.Lerp(bezierInfo.p1, bezierInfo.p2, bezierInfo.t);
        Vector3 p23 = Vector3.Lerp(bezierInfo.p2, bezierInfo.p3, bezierInfo.t);

        Vector3 p0112 = Vector3.Lerp(p01, p12, bezierInfo.t);
        Vector3 p1223 = Vector3.Lerp(p12, p23, bezierInfo.t);

        return Vector3.Lerp(p0112, p1223, bezierInfo.t);
    }

    public Orientation computeOrientation(float t)
    {
        if (t == 1)
            t -= 0.001f;

        BezierInfo2D bezierInfo = getCurrentBezierPoint(t);

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

        Vector3 side = Vector3.Cross(Vector3.forward, orientation.forward).normalized;

        orientation.upward = Vector3.Cross(orientation.forward, side).normalized;
        orientation.right = Vector3.Cross(orientation.forward, orientation.upward);

        return orientation;
    }

    public void ComputeLengths()
    {
        _Lengths = new float[_nbPointsToComputeLength];

        if (controlPointsList.Count < 2)
            return;

        Vector3 lastPoint = controlPointsList[0].controlPoints[1];

        _Lengths[0] = 0;

        for (int i = 1; i < _nbPointsToComputeLength; i++)
        {
            Vector3 point = computePoint((float)i / _nbPointsToComputeLength);
            _Lengths[i] = _Lengths[i-1] + (lastPoint - point).magnitude;
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
            if (distance < _Lengths[i])
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
        float factor = Remap(distance, _Lengths[lastindex], _Lengths[goodIndex], 0, 1);
        return (lastindex + factor) / (_nbPointsToComputeLength-1);
    }

    public float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public float length()
    {
        return _Lengths[_nbPointsToComputeLength - 1];
    }

     public Vector3 computeVelocityWithLength(float distance)
    {
        return computeVelocity(getTFactorWithDistance(distance));
    }

    public Orientation computeOrientationWithLength(float distance)
    {
        return computeOrientation(getTFactorWithDistance(distance));
    }

    public Vector3 computePointWithLength(float distance)
    {
        return computePoint(getTFactorWithDistance(distance));
    }


    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
