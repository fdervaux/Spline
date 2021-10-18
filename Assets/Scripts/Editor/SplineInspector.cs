using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;


[CustomEditor(typeof(Spline))]
public class SplineInspector : Editor
{
    public const int segmentNumber = 100;
    public const float handleSize = 0.1f;
    public const float pickSize = 0.2f;

    private Spline spline = null;
    private Transform handleTransform = null;
    private Quaternion handleRotation = Quaternion.identity;
    public int selectedIndex = -1;

    private SerializedProperty _controlPointsProperty;

    ReorderableList _list;

    void OnSelect(ReorderableList list)
    {
        Debug.Log(list.index);

        selectedIndex = list.index;
        SceneView.RepaintAll();
    }

    void DrawHeader(Rect rect)
    {
        string name = "Wave";
        EditorGUI.LabelField(rect, name);
    }

    void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(index);

        Vector3 oldPosition = element.vector3Value;

        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = EditorGUI.Vector3Field(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element.displayName, oldPosition);

        if (EditorGUI.EndChangeCheck())
        {
            movePointWithConstraint(index, oldPosition, newPosition);
        }
    }


    void addPointAtEnd()
    {

        if (_controlPointsProperty.arraySize > 2)
        {
            Vector3 Tangent =
                _controlPointsProperty.GetArrayElementAtIndex(_controlPointsProperty.arraySize - 2).vector3Value -
                _controlPointsProperty.GetArrayElementAtIndex(_controlPointsProperty.arraySize - 3).vector3Value;
            _controlPointsProperty.GetArrayElementAtIndex(_controlPointsProperty.arraySize - 1).vector3Value =
                _controlPointsProperty.GetArrayElementAtIndex(_controlPointsProperty.arraySize - 2).vector3Value + Tangent;
        }

        _controlPointsProperty.InsertArrayElementAtIndex(_controlPointsProperty.arraySize);
        _controlPointsProperty.GetArrayElementAtIndex(_controlPointsProperty.arraySize - 1).vector3Value += Vector3.one;

        _controlPointsProperty.InsertArrayElementAtIndex(_controlPointsProperty.arraySize);
        _controlPointsProperty.GetArrayElementAtIndex(_controlPointsProperty.arraySize - 1).vector3Value += Vector3.one;
    }

    void AddPointAtStart()
    {
        _controlPointsProperty.InsertArrayElementAtIndex(_controlPointsProperty.arraySize);
        _controlPointsProperty.GetArrayElementAtIndex(_controlPointsProperty.arraySize - 1).vector3Value += Vector3.one;

        _controlPointsProperty.InsertArrayElementAtIndex(_controlPointsProperty.arraySize);
        _controlPointsProperty.GetArrayElementAtIndex(_controlPointsProperty.arraySize - 1).vector3Value += Vector3.one;
    }


    void OnAdd(ReorderableList list)
    {
        Debug.Log("add");

        if(_controlPointsProperty.arraySize == 0)
        {
            AddPointAtStart();
            return;
        }

        if(_list.index >= _controlPointsProperty.arraySize - 2)
        {
            addPointAtEnd();
        }


       
    }

    void OnRemove(ReorderableList list)
    {
        int indexToRemove = (_list.index + 1) / 3;

        if (indexToRemove == (_controlPointsProperty.arraySize-1) / 3)
        {
            Debug.Log("test");
            _controlPointsProperty.DeleteArrayElementAtIndex(indexToRemove * 3);
            _controlPointsProperty.DeleteArrayElementAtIndex(indexToRemove * 3 - 1);

            if(indexToRemove > 0)
            _controlPointsProperty.DeleteArrayElementAtIndex(indexToRemove * 3 - 2);
        }
        else if (indexToRemove == 0)
        {
            if (_controlPointsProperty.arraySize > 0)
                _controlPointsProperty.DeleteArrayElementAtIndex(indexToRemove * 3 + 2);

            _controlPointsProperty.DeleteArrayElementAtIndex(indexToRemove * 3 + 1);

            _controlPointsProperty.DeleteArrayElementAtIndex(indexToRemove * 3 );
        }
        else
        {
            _controlPointsProperty.DeleteArrayElementAtIndex(indexToRemove * 3 + 1);
            _controlPointsProperty.DeleteArrayElementAtIndex(indexToRemove * 3);
            _controlPointsProperty.DeleteArrayElementAtIndex(indexToRemove * 3 - 1);
        }
        
        
    }


    public void OnEnable()
    {
        _controlPointsProperty = serializedObject.FindProperty("controlPoints");
        _list = new ReorderableList(serializedObject, _controlPointsProperty, false, true, true, true);

        _list.drawHeaderCallback = DrawHeader;
        _list.drawElementCallback = DrawListItems;
        _list.onSelectCallback = OnSelect;
        _list.onAddCallback = OnAdd;
        _list.onRemoveCallback = OnRemove;
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
        spline = target as Spline;
        handleTransform = spline.transform;
        handleRotation = handleTransform.rotation;
        if (Tools.pivotRotation == PivotRotation.Global)
            handleRotation = Quaternion.identity;

        showControlPoints();
        drawControlLines();
        drawSpline();

        serializedObject.ApplyModifiedProperties();
    }

    private void showControlPoints()
    {
        for (int i = 0; i < _controlPointsProperty.arraySize; i++)
        {

            if ((i + 1) / 3 == (selectedIndex + 1) / 3)
                Handles.color = Color.green;
            else
                Handles.color = Color.gray;

            showPoint(i);
        }
    }

    private void drawControlLines()
    {
        for (int i = 0; i < _controlPointsProperty.arraySize; i += 3)
        {
            List<Vector3> points = new List<Vector3>();

            if ((i + 1) / 3 == (selectedIndex + 1) / 3)
                Handles.color = Color.green;
            else
                Handles.color = Color.gray;

            if (i > 0)
            {
                points.Add(handleTransform.TransformPoint(_controlPointsProperty.GetArrayElementAtIndex(i - 1).vector3Value));
            }

            points.Add(handleTransform.TransformPoint(_controlPointsProperty.GetArrayElementAtIndex(i).vector3Value));

            if (i < _controlPointsProperty.arraySize - 1)
            {
                points.Add(handleTransform.TransformPoint(_controlPointsProperty.GetArrayElementAtIndex(i + 1).vector3Value));
            }


            Handles.DrawAAPolyLine(points.ToArray());

        }
    }



    private void drawSpline()
    {
        int nbCurves = _controlPointsProperty.arraySize / 3;
        for (int i = 0; i < nbCurves; i++)
        {
            Handles.DrawBezier(
                handleTransform.TransformPoint(_controlPointsProperty.GetArrayElementAtIndex(i * 3).vector3Value),
                handleTransform.TransformPoint(_controlPointsProperty.GetArrayElementAtIndex(i * 3 + 3).vector3Value),
                handleTransform.TransformPoint(_controlPointsProperty.GetArrayElementAtIndex(i * 3 + 1).vector3Value),
                handleTransform.TransformPoint(_controlPointsProperty.GetArrayElementAtIndex(i * 3 + 2).vector3Value),
                Color.white,
                null,
                2
             );
        }
    }

    private void showPoint(int index)
    {
        Vector3 oldPosition = _controlPointsProperty.GetArrayElementAtIndex(index).vector3Value;
        Vector3 oldPointpointW = handleTransform.TransformPoint(oldPosition);

        float sizeFactor = HandleUtility.GetHandleSize(oldPointpointW);

        EditorGUI.BeginChangeCheck();

        if (Handles.Button(oldPointpointW, handleRotation, sizeFactor * handleSize, sizeFactor * pickSize, Handles.CubeHandleCap))
        {
            selectedIndex = index;
            _list.index = index;
            Repaint();

        }

        if (selectedIndex == index)
        {
            Vector3 newPoint = Handles.PositionHandle(oldPointpointW, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                movePointWithConstraint(index, oldPosition, handleTransform.InverseTransformPoint(newPoint));
            }
        }
    }

    private void movePointWithConstraint(int index, Vector3 oldPosition, Vector3 newPosition)
    {
        Undo.RecordObject(spline, "Move Point");
        EditorUtility.SetDirty(spline);

        if (index % 3 == 0)
        {
            Vector3 displacment = newPosition - oldPosition;

            if (index > 0)
            {
                _controlPointsProperty.GetArrayElementAtIndex(index - 1).vector3Value += displacment;
            }
            _controlPointsProperty.GetArrayElementAtIndex(index).vector3Value = newPosition;
            if (index < _controlPointsProperty.arraySize - 1)
            {
                _controlPointsProperty.GetArrayElementAtIndex(index + 1).vector3Value += displacment;
            }
        }


        if (index % 3 == 1)
        {
            _controlPointsProperty.GetArrayElementAtIndex(index).vector3Value = newPosition;
            if (index > 1)
            {
                Vector3 Tangent = _controlPointsProperty.GetArrayElementAtIndex(index).vector3Value - _controlPointsProperty.GetArrayElementAtIndex(index - 1).vector3Value;
                _controlPointsProperty.GetArrayElementAtIndex(index - 2).vector3Value = _controlPointsProperty.GetArrayElementAtIndex(index - 1).vector3Value - Tangent;
            }

        }

        if (index % 3 == 2)
        {
            _controlPointsProperty.GetArrayElementAtIndex(index).vector3Value = newPosition;
            if (index < _controlPointsProperty.arraySize - 2)
            {
                Vector3 Tangent = _controlPointsProperty.GetArrayElementAtIndex(index).vector3Value - _controlPointsProperty.GetArrayElementAtIndex(index + 1).vector3Value;
                _controlPointsProperty.GetArrayElementAtIndex(index + 2).vector3Value = _controlPointsProperty.GetArrayElementAtIndex(index + 1).vector3Value - Tangent;
            }

        }
    }
}


