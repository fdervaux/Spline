using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformMeshOnSpline : MonoBehaviour
{
    public Mesh _mesh;
    private Mesh _baseMesh;
    public SplineBest _spline;

    public void UpdateMeshVertices(bool debug)
    {
        Mesh _mesh  = Instantiate(_baseMesh);

        Vector3[] vertices = _mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            float distance  = -vertices[i].x;
            if(debug)
                Debug.Log(vertices[i].x);

            Vector3 position = _spline.computePointWithLength(distance);
            Vector3 forward = _spline.computeVelocityWithLength(distance).normalized;
            Vector3 right = Vector3.Cross(forward , transform.up).normalized;
            Vector3 up = Vector3.Cross(forward, right).normalized;

            vertices[i] = vertices[i].y * -up + vertices[i].z * right + position;  
        }

        _mesh.vertices = vertices;

        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = _mesh;
    }

    private void Start() {
        _baseMesh = GetComponent<MeshFilter>().mesh;
        UpdateMeshVertices(true);
    }

    private void Update() {
        //UpdateMeshVertices(false);
    }


}