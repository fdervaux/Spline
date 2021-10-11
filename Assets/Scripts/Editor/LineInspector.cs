using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Line))]
public class LineInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }

    private void OnSceneGUI()
    {
        Line line = target as Line;

        Transform handleTransform = line.transform;
        Quaternion handleRotation = handleTransform.rotation;
        if(Tools.pivotRotation == PivotRotation.Global)
            handleRotation = Quaternion.identity;

        Vector3 p0 = handleTransform.TransformPoint(line.p0);
        Vector3 p1 = handleTransform.TransformPoint(line.p1);


        Handles.color = Color.white;
        Handles.DrawLine(p0, p1);

        EditorGUI.BeginChangeCheck();
        p0 = Handles.PositionHandle(p0, handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(line, "Move Point");
            EditorUtility.SetDirty(line);
            line.p0 = handleTransform.InverseTransformPoint(p0);
        }


        EditorGUI.BeginChangeCheck();
        p1 = Handles.PositionHandle(p1, handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(line, "Move Point");
            EditorUtility.SetDirty(line);
            line.p1 = handleTransform.InverseTransformPoint(p1);
            
        }


    }



}   

