using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;


[CustomEditor(typeof(Spline2D))]
public class SplineEditor2D : Editor
{
    public const float CapSize = 0.1f;
    public const float pickSize = 0.2f;

    private Spline2D spline = null;
    private Transform SplineTransform;

    private int selectedControlPoints;
    private int selectedIndex = -1;

    private SerializedProperty _controlPointProperty;

    ReorderableList _list;

    private bool shiftDown = false;

    void OnSelect(ReorderableList list)
    {
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
        SplineControlPoint2D controlPoint = getControlPoint(index);

        SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty mode = element.FindPropertyRelative("mode");
        SerializedProperty angle = element.FindPropertyRelative("angle");
        SerializedProperty controlPoints = element.FindPropertyRelative("controlPoints");

        EditorGUI.BeginChangeCheck();

        EditorGUI.PropertyField(
            new Rect(rect.x, rect.y + 0.5f * EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight),
            mode,
            new GUIContent("Mode "));

        ReorderableList controlPointsList = new ReorderableList(serializedObject, controlPoints, false, false, false, false);

        controlPointsList.drawElementCallback = (Rect rect, int indexControl, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty controlPoints = element.FindPropertyRelative("controlPoints");
            SerializedProperty vector2 = controlPoints.GetArrayElementAtIndex(indexControl);

            Vector3 oldPosition = vector2.vector2Value;


            EditorGUI.BeginChangeCheck();



            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                vector2,
                new GUIContent("point " + indexControl));

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                Vector2 newPosition = vector2.vector3Value;
                movePointWithConstraint(index, indexControl, newPosition, oldPosition);
            }
        };

        controlPointsList.DoList(new Rect(rect.x, rect.y + 3f * EditorGUIUtility.singleLineHeight, rect.width, 4f * EditorGUIUtility.singleLineHeight));
        controlPointsList.elementHeight = EditorGUIUtility.singleLineHeight;

        serializedObject.ApplyModifiedProperties();
    }

    void OnAdd(ReorderableList list)
    {
        if (_controlPointProperty.arraySize == 0)
        {
            AddPoint(spline.transform.position);
            return;
        }

        Vector3 basePosition = spline.getControlPoint(_controlPointProperty.arraySize - 1).controlPoints[1];
        Vector3 position = basePosition + spline.computeOrientation(1).right * 5f;
        AddPoint(position);
    }

    private void AddPoint(Vector3 position)
    {
        SplineControlPoint2D point = new SplineControlPoint2D();

        Vector3 forward = spline.transform.forward;
        if (spline.ControlPointsList.Count > 1)
        {
            forward = spline.computeOrientation(1).forward;
        }


        point.controlPoints = new Vector2[3];
        point.controlPoints[0] = spline.transform.InverseTransformPoint(position + forward * -5f);
        point.controlPoints[1] = spline.transform.InverseTransformPoint(position);
        point.controlPoints[2] = spline.transform.InverseTransformPoint(position + forward * 5f);

        point.mode = SplineControlPoint2D.Mode.CONSTRAINT;

        spline.ControlPointsList.Add(point);

        spline.ComputeLengths();
    }

    void OnRemove(ReorderableList list)
    {
        _controlPointProperty.DeleteArrayElementAtIndex(_list.index);
        spline.ComputeLengths();
    }


    private void OnEnable()
    {
        Debug.Log("OnEnable");
        spline = target as Spline2D;

        SplineTransform = spline.transform;

        spline.ComputeLengths();

        _controlPointProperty = serializedObject.FindProperty("controlPointsList");
        _list = new ReorderableList(serializedObject, _controlPointProperty, false, true, true, true);

        _list.drawHeaderCallback = DrawHeader;
        _list.drawElementCallback = DrawListItems;
        _list.onSelectCallback = OnSelect;
        _list.onAddCallback = OnAdd;
        _list.onRemoveCallback = OnRemove;

        _list.elementHeight = EditorGUIUtility.singleLineHeight * 7.5f;

    }

    private SplineControlPoint2D getControlPoint(int controlPointIndex)
    {
        SplineControlPoint2D controlPoint = spline.getControlPoint(controlPointIndex);
        return controlPoint;
    }


    private void setControlPoint(int controlPointIndex, SplineControlPoint2D controlPoint)
    {
        spline.setControlPoint(controlPointIndex, controlPoint);
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


        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftShift)
        {
            shiftDown = true;
        }
        if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.LeftShift)
        {
            shiftDown = false;
        }

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete)
        {
            Undo.RecordObject(spline, "Remove Point");
            spline.removeControlPoint(selectedControlPoints);
            Event.current.Use();
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && shiftDown)
        {
            Undo.RecordObject(spline, "Add Point");
            Ray screenRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            float distance = Vector3.Distance(spline.transform.position, Camera.current.transform.position);

            if (spline.ControlPointsList.Count > 0)
            {
                distance = Vector3.Distance(spline.transform.TransformPoint(getControlPoint(spline.ControlPointsList.Count - 1).controlPoints[1]), Camera.current.transform.position);
            }

            AddPoint(screenRay.origin + screenRay.direction * distance);
        }



        for (int i = 0; i < spline.ControlPointsList.Count; i++)
        {
            showControlPoint(i);
        }

        drawCurves();
    }

    private void drawCurves()
    {
        for (int i = 0; i < spline.ControlPointsList.Count - 1; i++)
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

        SplineControlPoint2D controlPoint = getControlPoint(controlPointIndex);

        newPos.z = spline.transform.position.z;

        controlPoint.controlPoints[vector3Index] = newPos;

        if (vector3Index == 1)
        {
            Vector3 displacement = newPos - oldPos;

            controlPoint.controlPoints[0] += (Vector2)displacement;
            controlPoint.controlPoints[2] += (Vector2)displacement;
        }

        if (vector3Index == 0 && controlPoint.mode == SplineControlPoint2D.Mode.CONSTRAINT)
        {
            Vector3 dist = controlPoint.controlPoints[1] - (Vector2)newPos;
            controlPoint.controlPoints[2] = controlPoint.controlPoints[1] + (Vector2)dist;
        }
        if (vector3Index == 2 && controlPoint.mode == SplineControlPoint2D.Mode.CONSTRAINT)
        {
            Vector3 dist = controlPoint.controlPoints[1] - (Vector2)newPos;
            controlPoint.controlPoints[0] = controlPoint.controlPoints[1] + (Vector2)dist;
        }

        setControlPoint(controlPointIndex, controlPoint);


        spline.ComputeLengths();
    }


    private void showControlPoint(int index)
    {

        SplineControlPoint2D point = getControlPoint(index);



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

            EditorGUI.BeginChangeCheck();

            int currentControlID = GUIUtility.GetControlID(FocusType.Passive);

            Vector3 position = Handles.FreeMoveHandle(currentControlID, worldPosition, sizeFactor * CapSize, Vector3.zero, Handles.CubeHandleCap);

            if (currentControlID == GUIUtility.hotControl)
            {
                selectedControlPoints = index;
            }

            if (EditorGUI.EndChangeCheck())
            {
                movePointWithConstraint(index, i, SplineTransform.InverseTransformPoint(position), SplineTransform.InverseTransformPoint(worldPosition));
            }
            // }
        }


    }
}