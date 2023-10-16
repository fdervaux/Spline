using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;


[CustomEditor(typeof(Spline))]
public class SplineEditor : Editor
{
    public const float CapSize = 0.1f;
    public const float pickSize = 0.2f;

    private Spline spline = null;
    private Transform SplineTransform;

    private int selectedControlPoints;
    private int selectedIndex = -1;

    private SerializedProperty _controlPointProperty;

    ReorderableList _list;

    private bool shiftDown = false;

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
        if (_controlPointProperty.arraySize == 0)
        {
            AddPoint(spline.transform.position);
            return;
        }

        Vector3 basePosition = spline.getControlPoint(_controlPointProperty.arraySize - 1).controlPoints[1];
        Vector3 position = basePosition + spline.computeOrientationWithRMF(1).forward * 5f;
        AddPoint(position);
    }

    private void AddPoint(Vector3 position)
    {
        SplineControlPoint point = new SplineControlPoint();

        Vector3 forward = spline.transform.forward;
        if (spline.ControlPointsList.Count > 1)
        {
            forward = spline.computeOrientationWithRMF(1).forward;
        }


        point.controlPoints = new Vector3[3];
        point.controlPoints[0] = spline.transform.InverseTransformPoint(position + forward * -5f);
        point.controlPoints[1] = spline.transform.InverseTransformPoint(position);
        point.controlPoints[2] = spline.transform.InverseTransformPoint(position + forward * 5f);

        point.mode = SplineControlPoint.Mode.CONSTRAINT;

        spline.ControlPointsList.Add(point);

        spline.ComputeRMFAndLengths();
    }

    void OnRemove(ReorderableList list)
    {
        _controlPointProperty.DeleteArrayElementAtIndex(_list.index);
        spline.ComputeRMFAndLengths();
    }


    private void OnEnable()
    {
        Debug.Log("OnEnable");
        spline = target as Spline;

        SplineTransform = spline.transform;

        spline.ComputeRMFAndLengths();

        _controlPointProperty = serializedObject.FindProperty("controlPointsList");
        _list = new ReorderableList(serializedObject, _controlPointProperty, false, true, true, true);

        _list.drawHeaderCallback = DrawHeader;
        _list.drawElementCallback = DrawListItems;
        _list.onSelectCallback = OnSelect;
        _list.onAddCallback = OnAdd;
        _list.onRemoveCallback = OnRemove;

        _list.elementHeight = EditorGUIUtility.singleLineHeight * 7.5f;

    }

    private SplineControlPoint getControlPoint(int controlPointIndex)
    {
        SplineControlPoint controlPoint = spline.getControlPoint(controlPointIndex);
        return controlPoint;
    }


    private void setControlPoint(int controlPointIndex, SplineControlPoint controlPoint)
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


        spline.ComputeRMFAndLengths();
    }


    private void showControlPoint(int index)
    {

        SplineControlPoint point = getControlPoint(index);



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


            Orientation orientation = spline.computeOrientationWithRMF((float)index / (spline.ControlPointsList.Count - 1));

            Orientation baseOrientation = spline.computeOrientationWithRMF((float)index / (spline.ControlPointsList.Count - 1), false);

            Quaternion quaternion = Quaternion.LookRotation(
                spline.transform.TransformDirection(orientation.forward),
                spline.transform.TransformDirection(orientation.upward)
                );

            Quaternion baseQuaternion = Quaternion.LookRotation(
                spline.transform.TransformDirection(baseOrientation.forward),
                spline.transform.TransformDirection(baseOrientation.upward)
                );



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

            if (i == 1 && selectedControlPoints == index)
            {

                Handles.ArrowHandleCap(
                    GUIUtility.GetControlID(FocusType.Passive),
                    worldPosition,
                    Quaternion.LookRotation(orientation.upward, orientation.forward),
                    sizeFactor * 0.6f,
                    EventType.Repaint
                );

                EditorGUI.BeginChangeCheck();
                quaternion = Handles.Disc(GUIUtility.GetControlID(FocusType.Passive), quaternion, worldPosition, orientation.forward, sizeFactor * 0.5f, false, 2f);

                if (EditorGUI.EndChangeCheck())
                {

                    Undo.RecordObject(spline, "Rotate Point");
                    EditorUtility.SetDirty(spline);
                    Vector3 upFirst = baseQuaternion * Vector3.up;
                    Vector3 upSecond = quaternion * Vector3.up;
                    float angle = Vector3.SignedAngle(upFirst, upSecond, orientation.forward);
                    SplineControlPoint controlPoint = getControlPoint(index);
                    controlPoint.angle = angle;
                    setControlPoint(index, controlPoint);
                }
            }
            // }
        }


    }
}