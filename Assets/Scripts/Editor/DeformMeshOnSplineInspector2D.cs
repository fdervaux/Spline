using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;


[CustomEditor(typeof(DeformMeshOnSpline2D))]
public class DeformMeshOnSplineInspector2D : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("DeformMesh"))
        {
            DeformMeshOnSpline2D deformMeshOnSpline = target as DeformMeshOnSpline2D;
            deformMeshOnSpline.deform();

        }
    }
}
