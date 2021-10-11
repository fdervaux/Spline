using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (BezierCurve))]
public class BezierCurveInspector : Editor
{
    public const int segmentNumber = 100;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }

    private void OnSceneGUI()
    {
        BezierCurve bezierCurve = target as BezierCurve;

        Transform handleTransform = bezierCurve.transform;

        Quaternion handleRotation = handleTransform.rotation;

        if (Tools.pivotRotation == PivotRotation.Global)
            handleRotation = Quaternion.identity;

        Vector3 p0 = handleTransform.TransformPoint(bezierCurve.p0);
        Vector3 p1 = handleTransform.TransformPoint(bezierCurve.p1);
        Vector3 p2 = handleTransform.TransformPoint(bezierCurve.p2);

        Handles.color = Color.white;

        Vector3 lastPoint = p0;
        for (float i = 1; i < segmentNumber; i++)
        {
            Vector3 currentPoint =
                handleTransform
                    .TransformPoint(bezierCurve
                        .computeBezierPoint(i / segmentNumber));
            Handles.DrawLine (lastPoint, currentPoint);

            lastPoint = currentPoint;
        }

        Handles.DrawLine (lastPoint, p2);

        EditorGUI.BeginChangeCheck();
        p0 = Handles.PositionHandle(p0, handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(bezierCurve, "Move Point");
            EditorUtility.SetDirty (bezierCurve);
            bezierCurve.p0 = handleTransform.InverseTransformPoint(p0);
        }

        EditorGUI.BeginChangeCheck();
        p1 = Handles.PositionHandle(p1, handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(bezierCurve, "Move Point");
            EditorUtility.SetDirty (bezierCurve);
            bezierCurve.p1 = handleTransform.InverseTransformPoint(p1);
        }

        EditorGUI.BeginChangeCheck();
        p2 = Handles.PositionHandle(p2, handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(bezierCurve, "Move Point");
            EditorUtility.SetDirty (bezierCurve);
            bezierCurve.p2 = handleTransform.InverseTransformPoint(p2);
        }
    }

    private void drawCurve(BezierCurve curve, Transform handleTransform)
    {
    }
}
