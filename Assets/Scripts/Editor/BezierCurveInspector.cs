using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierCurve))]
public class BezierCurveInspector : Editor
{
    public const int segmentNumber = 100;

    Vector3[] controlPointsWorld = new Vector3[3];

    BezierCurve bezierCurve = null;
    Transform handleTransform = null;
    Quaternion handleRotation = Quaternion.identity;

    public const float CapSize = 0.1f;
    public const float pickSize = 0.2f;

    public int indexSelected = -1;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }

    private void OnSceneGUI()
    {
        bezierCurve = target as BezierCurve;
        handleTransform = bezierCurve.transform;
        handleRotation = handleTransform.rotation;
        if (Tools.pivotRotation == PivotRotation.Global)
            handleRotation = Quaternion.identity;
        
        convertControlPointToWorld();
        drawCurve();
        drawConstructLine();
        showControlPoints();
    }

    private void showControlPoints()
    {
        for (int i = 0; i < 3; i++)
        {
            showPoint(i);
        }
    }

    private void convertControlPointToWorld()
    {
        for (int i = 0; i < 3; i++)
        {
            controlPointsWorld[i] = handleTransform.TransformPoint(bezierCurve.controlPoints[i]);
        }
    }

    private void drawConstructLine()
    {
        Handles.color = Color.green;
        Vector3[] constructLinesPoints = new Vector3[3];
        for (int i = 0; i < 3; i++)
        {
            constructLinesPoints[i] = controlPointsWorld[i];
        }
        Handles.DrawAAPolyLine(constructLinesPoints);
    }

    private void drawCurve()
    {
        Handles.color = Color.white;
        Vector3[] points = new Vector3[segmentNumber + 1];
        points[0] = controlPointsWorld[0];
        for (int i = 1; i < segmentNumber; i++)
        {
            Vector3 currentPoint = handleTransform.TransformPoint(bezierCurve.computeBezierPoint((float)i / segmentNumber));
            points[i] = currentPoint;
        }
        points[segmentNumber] = controlPointsWorld[2];
        Handles.DrawAAPolyLine(points);
    }

    private void showPoint(int index)
    {
        EditorGUI.BeginChangeCheck();

        float sizeFactor = HandleUtility.GetHandleSize(controlPointsWorld[index]);

        if (Handles.Button(controlPointsWorld[index], handleRotation, sizeFactor*CapSize, sizeFactor*pickSize, Handles.CubeHandleCap))
        {
            indexSelected = index;
        }

        if( indexSelected == index)
        {
            controlPointsWorld[index] = Handles.PositionHandle(controlPointsWorld[index], handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(bezierCurve, "Move Point");
                EditorUtility.SetDirty (bezierCurve);
                bezierCurve.controlPoints[index] = handleTransform.InverseTransformPoint(controlPointsWorld[index]);
            }
        }
    }
}
