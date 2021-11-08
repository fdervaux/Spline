using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;


[CustomEditor(typeof(SplineBest))]
public class SplineBestInspector : Editor
{
    public const float CapSize = 0.1f;
    public const float pickSize = 0.2f;

    private SplineBest spline = null;
    private Transform SplineTransform;

    private int selectedControlPoints;
    private int selectedIndex = -1;

    private void OnEnable()
    {
        spline = target as SplineBest;
        SplineTransform = spline.transform;
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }

    private void OnSceneGUI()
    {
        for(int i = 0; i < spline.controlPointsList.Count; i++)
        {
            showControlPoint(i);
        }

        drawCurves();
    }

    private void drawCurves()
    {
        for(int i = 0; i < spline.controlPointsList.Count -1; i++)
        {
            Vector3 P1 = SplineTransform.TransformPoint( spline.controlPointsList[i].controlPoints[1]);
            Vector3 P2 = SplineTransform.TransformPoint( spline.controlPointsList[i].controlPoints[2]);
            Vector3 P3 = SplineTransform.TransformPoint( spline.controlPointsList[i+1].controlPoints[0]);
            Vector3 P4 = SplineTransform.TransformPoint( spline.controlPointsList[i+1].controlPoints[1]);

            Handles.DrawBezier(P1,P4,P2,P3,Color.white,null,1f);
        }
    }


    private void showControlPoint(int index)
    {

        SplineControlPoint point = spline.controlPointsList[index];

        EditorGUI.BeginChangeCheck();
        //Vector3[] worldPosition = new Vector3[3];

        if( selectedControlPoints == index)
        {
            Handles.color = Color.green;
        }
        else
        {
            Handles.color = Color.white;
        }

        Vector3[] worldPositions = new Vector3[3]; 
        for(int i = 0; i < 3; i++)
        {
            worldPositions[i] = SplineTransform.TransformPoint(point.controlPoints[i]);
        }

        Handles.DrawAAPolyLine(worldPositions);
        
        for (int i = 0; i < 3; i++)
        {
            Vector3 worldPosition = SplineTransform.TransformPoint(point.controlPoints[i]);
            float sizeFactor = HandleUtility.GetHandleSize(worldPosition);
            if (Handles.Button(worldPosition, Quaternion.identity, sizeFactor * CapSize, sizeFactor * pickSize, Handles.CubeHandleCap))
            {
                selectedIndex = i;
                selectedControlPoints = index;
            }

            if (selectedIndex == i && selectedControlPoints == index)
            {
                Vector3 position = Handles.PositionHandle(worldPosition, Quaternion.identity);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(spline, "Move Point");
                    EditorUtility.SetDirty(spline);

                    spline.controlPointsList[index].controlPoints[i] = SplineTransform.InverseTransformPoint(position);

                    if (i == 1)
                    {
                        Vector3 displacement = position - worldPosition;

                        spline.controlPointsList[index].controlPoints[0] += displacement;
                        spline.controlPointsList[index].controlPoints[2] += displacement;
                    }

                    if( i == 0 && spline.controlPointsList[index].mode == SplineControlPoint.Mode.CONSTRAINT)
                    {
                        Vector3 dist = spline.controlPointsList[index].controlPoints[1] - SplineTransform.InverseTransformPoint(position);
                        spline.controlPointsList[index].controlPoints[2] = spline.controlPointsList[index].controlPoints[1] + dist;
                    }
                    if( i == 2 && spline.controlPointsList[index].mode == SplineControlPoint.Mode.CONSTRAINT)
                    {
                        Vector3 dist = spline.controlPointsList[index].controlPoints[1] - SplineTransform.InverseTransformPoint(position);
                        spline.controlPointsList[index].controlPoints[0] = spline.controlPointsList[index].controlPoints[1] + dist;
                    }
                
                }
            }
        }


    }
}