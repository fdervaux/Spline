using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformMeshOnSpline : MonoBehaviour
{
    public Mesh _baseMesh;
    public SplineBest _spline;
    public List<Vector3> points = new List<Vector3>();


    public void UpdateMeshVertices()
    {
        float distance = 0;

        int childs = transform.childCount;
        for (int i = childs - 1; i > 0; i--)
        {
            GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
        }

        Vector2[] uv = _baseMesh.uv;

        int cpt = 0;

        MeshRenderer _baseMeshRenderer = GetComponent<MeshRenderer>();


        while (distance < _spline.length())
        {
            GameObject gameObject = new GameObject("test");
            gameObject.transform.parent = transform;

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

            meshRenderer.sharedMaterials = _baseMeshRenderer.sharedMaterials;

            Mesh mesh = new Mesh();
            mesh.subMeshCount = _baseMesh.subMeshCount;

            Vector3[] vertices = _baseMesh.vertices;


            for (int i = 0; i < vertices.Length; i++)
            {
                float distanceOnCurve = -vertices[i].x + distance;

                Orientation orientation = _spline.computeOrientationWithLenght(distanceOnCurve, Vector3.up);

                Vector3 position = transform.TransformPoint(_spline.computePointWithLength(distanceOnCurve));
                Vector3 forward = transform.TransformDirection(orientation.forward);
                Vector3 right = transform.TransformDirection(orientation.right);
                Vector3 up = transform.TransformDirection(orientation.upward);

                vertices[i] = vertices[i].y * up + vertices[i].z * -right + position;
            }

            mesh.vertices = vertices;
            mesh.normals = _baseMesh.normals;

            for (int i = 0; i < _baseMesh.subMeshCount; i++)
            {
                int[] triangles = _baseMesh.GetTriangles(i);
                for (int j = 0; j < triangles.Length; j++)
                {
                    mesh.SetTriangles(triangles, i);
                }
            }

            mesh.uv = _baseMesh.uv;

            meshFilter.mesh = mesh;


            distance += _baseMesh.bounds.size.x;
            cpt++;

        }

    }

    public void deform()
    {
        if (_baseMesh != null) UpdateMeshVertices();
    }

    private void Start()
    {

    }




}