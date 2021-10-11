using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (CubicBezierCurve))]
public class CubicBezierCurveInspector : Editor
{
    public const int segmentNumber = 100;

    int selectedIndex = -1;

    Vector3[] controlPoints = new Vector3[4];

    Quaternion handleRotation = Quaternion.identity;
    CubicBezierCurve bezierCurve = null;
    Transform handleTransform = null;


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }

    private void OnSceneGUI()
    {
        bezierCurve = target as CubicBezierCurve;
        handleTransform = bezierCurve.transform;
        handleRotation = handleTransform.rotation;

        if (Tools.pivotRotation == PivotRotation.Global)
            handleRotation = Quaternion.identity;

        

        controlPoints[0] = handleTransform.TransformPoint(bezierCurve.controlPoints[0]);
        controlPoints[1] = handleTransform.TransformPoint(bezierCurve.controlPoints[1]);
        controlPoints[2] = handleTransform.TransformPoint(bezierCurve.controlPoints[2]);
        controlPoints[3] = handleTransform.TransformPoint(bezierCurve.controlPoints[3]);

        Handles.color = Color.white;

        /*Vector3 lastPoint = p0;
        for(float i = 0 ; i< segmentNumber; i++)
        {
            Vector3 currentPoint = handleTransform.TransformPoint(bezierCurve.computeBezierPoint(i/segmentNumber));
            Handles.DrawLine(lastPoint,currentPoint);
            
            lastPoint = currentPoint;
        }

        Handles.DrawLine(lastPoint,p3);*/
        List<Vector3> points = new List<Vector3>();
        points.Add (controlPoints[0]);


        for (float i = 0; i < segmentNumber; i++)
        {
            Vector3 currentPoint =
                handleTransform
                    .TransformPoint(bezierCurve
                        .computeBezierPoint(i / segmentNumber));
            points.Add (currentPoint);
        }
        points.Add (controlPoints[3]);

        Handles.DrawAAPolyLine(points.ToArray());

        Handles.color = Color.green;

        Handles.DrawLine (controlPoints[0], controlPoints[1]);
        Handles.DrawLine (controlPoints[2], controlPoints[3]);


        /*p0 = Handles.PositionHandle(p0, handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(bezierCurve, "Move Point");
            EditorUtility.SetDirty (bezierCurve);
            bezierCurve.p0 = handleTransform.InverseTransformPoint(p0);
        }*/

        /*EditorGUI.BeginChangeCheck();
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

        EditorGUI.BeginChangeCheck();
        p3 = Handles.PositionHandle(p3, handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(bezierCurve, "Move Point");
            EditorUtility.SetDirty (bezierCurve);
            bezierCurve.p3 = handleTransform.InverseTransformPoint(p3);
        }*/


        for (int i = 0; i < 4; i++)
        {
            showPoint(i);
        }


    }


    private void showPoint(int index)
    {
        if (Handles.Button(controlPoints[index], handleRotation, 0.1f, 0.2f, Handles.CubeHandleCap))
        {
            selectedIndex = index;
        }

        if( selectedIndex == index )
        {
            EditorGUI.BeginChangeCheck();
            controlPoints[index] = Handles.PositionHandle(controlPoints[index], handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(bezierCurve, "Move Point");
                EditorUtility.SetDirty (bezierCurve);
                bezierCurve.controlPoints[index] = handleTransform.InverseTransformPoint(controlPoints[index]);
            }
        }
    }

    private void drawCurve(BezierCurve curve, Transform handleTransform)
    {
    }
}
