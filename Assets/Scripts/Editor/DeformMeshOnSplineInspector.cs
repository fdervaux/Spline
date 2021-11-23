using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;


[CustomEditor(typeof(DeformMeshOnSpline))]
public class DeformMeshOnSplineInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("DeformMesh"))
        {
            DeformMeshOnSpline deformMeshOnSpline = target as DeformMeshOnSpline;
            deformMeshOnSpline.deform();

        }
    }
}
