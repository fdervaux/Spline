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

    private SerializedProperty _controlPointProperty;

    ReorderableList _list;

    void OnSelect(ReorderableList list)
    {
        Debug.Log(list.index);

        selectedControlPoints = list.index;
        SceneView.RepaintAll();
    }

    void DrawHeader(Rect rect)
    {
        string name = "ControlPoints";
        EditorGUI.LabelField(rect, name);
    }

    void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        SplineControlPoint controlPoint = getControlPoint(index);

        SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty mode = element.FindPropertyRelative("mode");
        SerializedProperty angle = element.FindPropertyRelative("angle");
        SerializedProperty controlPoints = element.FindPropertyRelative("controlPoints");

        EditorGUI.BeginChangeCheck();

        EditorGUI.PropertyField(
            new Rect(rect.x, rect.y + 0.5f * EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight),
            mode,
            new GUIContent("Mode "));

        EditorGUI.PropertyField(
            new Rect(rect.x, rect.y + 1.5f * EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight),
            angle,
            new GUIContent("Angle "));

        ReorderableList controlPointsList = new ReorderableList(serializedObject, controlPoints, false, false, false, false);

        controlPointsList.drawElementCallback = (Rect rect, int indexControl, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty controlPoints = element.FindPropertyRelative("controlPoints");
            SerializedProperty vector3 = controlPoints.GetArrayElementAtIndex(indexControl);

            Vector3 oldPosition = vector3.vector3Value;


            EditorGUI.BeginChangeCheck();



            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                vector3,
                new GUIContent("point " + indexControl));

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                Vector3 newPosition = vector3.vector3Value;
                movePointWithConstraint(index, indexControl, newPosition, oldPosition);
            }
        };

        controlPointsList.DoList(new Rect(rect.x, rect.y + 3f * EditorGUIUtility.singleLineHeight, rect.width, 4f * EditorGUIUtility.singleLineHeight));
        controlPointsList.elementHeight = EditorGUIUtility.singleLineHeight;

        serializedObject.ApplyModifiedProperties();
    }

    void OnAdd(ReorderableList list)
    {
        int index = _controlPointProperty.arraySize;

        SplineControlPoint point = new SplineControlPoint();
        point.mode = SplineControlPoint.Mode.CONSTRAINT;

        point.controlPoints = new Vector3[3];
        point.controlPoints[0] = Vector3.zero;
        point.controlPoints[1] = Vector3.zero;
        point.controlPoints[2] = Vector3.zero;

        spline.ControlPointsList.Add(point);


        spline.computeLengths();
    }

    void OnRemove(ReorderableList list)
    {
        _controlPointProperty.DeleteArrayElementAtIndex(_list.index);
        spline.computeLengths();
    }


    private void OnEnable()
    {
        Debug.Log("OnEnable");
        spline = target as SplineBest;

        SplineTransform = spline.transform;

        spline.computeLengths();

        _controlPointProperty = serializedObject.FindProperty("controlPointsList");
        _list = new ReorderableList(serializedObject, _controlPointProperty, false, true, true, true);

        _list.drawHeaderCallback = DrawHeader;
        _list.drawElementCallback = DrawListItems;
        _list.onSelectCallback = OnSelect;
        _list.onAddCallback = OnAdd;
        _list.onRemoveCallback = OnRemove;

        _list.elementHeight = EditorGUIUtility.singleLineHeight * 7f;

    }

    private SplineControlPoint getControlPoint(int controlPointIndex)
    {
        SerializedProperty controlPointProperty = _controlPointProperty.GetArrayElementAtIndex(controlPointIndex);
        SerializedProperty controlPoints = controlPointProperty.FindPropertyRelative("controlPoints");
        SerializedProperty controlMode = controlPointProperty.FindPropertyRelative("mode");

        SplineControlPoint controlPoint = new SplineControlPoint();

        controlPoint.controlPoints = new Vector3[3];

        for (int i = 0; i < 3; ++i)
        {
            controlPoint.controlPoints[i] = controlPoints.GetArrayElementAtIndex(i).vector3Value;
        }

        controlPoint.mode = (SplineControlPoint.Mode)controlMode.intValue;

        return controlPoint;
    }


    private void setControlPoint(int controlPointIndex, SplineControlPoint controlPoint)
    {
        SerializedProperty controlPointProperty = _controlPointProperty.GetArrayElementAtIndex(controlPointIndex);
        SerializedProperty controlPoints = controlPointProperty.FindPropertyRelative("controlPoints");
        SerializedProperty controlMode = controlPointProperty.FindPropertyRelative("mode");

        for (int i = 0; i < 3; ++i)
        {
            controlPoints.GetArrayElementAtIndex(i).vector3Value = controlPoint.controlPoints[i];
        }

        controlMode.intValue = (int)controlPoint.mode;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update(); // Update the array property's representation in the inspector
        _list.DoLayoutList(); // Have the ReorderableList do its work
        // We need to call this so that changes on the Inspector are saved by Unity.
        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        for (int i = 0; i < _controlPointProperty.arraySize; i++)
        {
            showControlPoint(i);
        }

        drawCurves();
    }

    private void drawCurves()
    {
        for (int i = 0; i < _controlPointProperty.arraySize - 1; i++)
        {
            Vector3 P1 = SplineTransform.TransformPoint(getControlPoint(i).controlPoints[1]);
            Vector3 P2 = SplineTransform.TransformPoint(getControlPoint(i).controlPoints[2]);
            Vector3 P3 = SplineTransform.TransformPoint(getControlPoint(i + 1).controlPoints[0]);
            Vector3 P4 = SplineTransform.TransformPoint(getControlPoint(i + 1).controlPoints[1]);

            Handles.DrawBezier(P1, P4, P2, P3, Color.white, null, 1f);
        }
    }

    private void movePointWithConstraint(int controlPointIndex, int vector3Index, Vector3 newPos, Vector3 oldPos)
    {
        Undo.RecordObject(spline, "Move Point");

        EditorUtility.SetDirty(spline);

        SplineControlPoint controlPoint = getControlPoint(controlPointIndex);

        controlPoint.controlPoints[vector3Index] = newPos;

        if (vector3Index == 1)
        {
            Vector3 displacement = newPos - oldPos;

            controlPoint.controlPoints[0] += displacement;
            controlPoint.controlPoints[2] += displacement;
        }

        if (vector3Index == 0 && controlPoint.mode == SplineControlPoint.Mode.CONSTRAINT)
        {
            Vector3 dist = controlPoint.controlPoints[1] - newPos;
            controlPoint.controlPoints[2] = controlPoint.controlPoints[1] + dist;
        }
        if (vector3Index == 2 && controlPoint.mode == SplineControlPoint.Mode.CONSTRAINT)
        {
            Vector3 dist = controlPoint.controlPoints[1] - newPos;
            controlPoint.controlPoints[0] = controlPoint.controlPoints[1] + dist;
        }

        setControlPoint(controlPointIndex, controlPoint);

        serializedObject.ApplyModifiedProperties();

        spline.computeLengths();
    }


    private void showControlPoint(int index)
    {

        SplineControlPoint point = getControlPoint(index);

        EditorGUI.BeginChangeCheck();

        if (selectedControlPoints == index)
        {
            Handles.color = Color.green;
        }
        else
        {
            Handles.color = Color.white;
        }

        Vector3[] worldPositions = new Vector3[3];
        for (int i = 0; i < 3; i++)
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
                _list.index = index;
                Repaint();
            }

            if (selectedIndex == i && selectedControlPoints == index)
            {
                Vector3 position = Handles.PositionHandle(worldPosition, Quaternion.identity);

                if (EditorGUI.EndChangeCheck())
                {
                    movePointWithConstraint(index, i, SplineTransform.InverseTransformPoint(position), SplineTransform.InverseTransformPoint(worldPosition));
                }
            }
        }


    }
}