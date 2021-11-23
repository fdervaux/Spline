using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceOnSpline : MonoBehaviour
{
    public SplineBest _spline;
    private float _distance = 0f;

    public float _step = 0.001f;

    public float speed = 0.1f; //m.s

    public Vector3 direction = Vector3.zero;

    public List<Vector3> vertices;

    public int xSize = 10;
    private int ySize = 10;

    public float xStep = 0.2f;
    public float yStep = 0.3f;

    private MeshFilter _meshFilter;

    public void updateVertices()
    {
        vertices.Clear();

        float distanceY = 0;

        ySize = 0;

        while  (distanceY < _spline.length())
        {

            Vector3 position = _spline.computePointWithLength(distanceY);
            Vector3 velocity = Vector3.Normalize(_spline.computeVelocityWithLength(distanceY));
            Vector3 xAxis = Vector3.Normalize(Vector3.Cross(velocity,transform.up));

            for (int x = 0; x <= xSize; x++)
            {  
                vertices.Add(  position + xAxis * (x - 0.5f * xSize) * xStep);
            }

            distanceY += yStep;
            ySize++;
        }

        _meshFilter.mesh.vertices = vertices.ToArray();
    }

    private void updateTriangles()
    {
        int[] triangles = new int[xSize * (ySize-1) * 12];

        int ti = 0;
        int vi = 0;

		for (int y = 0; y < ySize -1; y++) {
			for (int x = 0; x < xSize; x++) {

				triangles[ti] = vi;
                triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 2] = vi + 1;

				triangles[ti + 3] = vi + 1;
				triangles[ti + 4] = vi + xSize + 1;
				triangles[ti + 5] = vi + xSize + 2;

                ti += 6; 
                
                triangles[ti] = vi + 1;
                triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 2] = vi;
                
				triangles[ti + 3] = vi + xSize + 2;
				triangles[ti + 4] = vi + xSize + 1;
				triangles[ti + 5] = vi + 1;

                ti += 6; 
                vi++;
			}
            vi++;
		}

        
        _meshFilter.mesh.triangles = triangles;
    }

    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = new Mesh();

        _meshFilter = GetComponent<MeshFilter>();
        
        _meshFilter.mesh = mesh;
        mesh.name = "Procedural Grid";

        vertices = new List<Vector3>();

        updateVertices();
        updateTriangles();
        

        // draw grid
    }

    // Update is called once per frame
    void Update()
    {
        updateVertices();
        updateTriangles();
    }

    private void OnDrawGizmos()
    {
        if (vertices == null)
        {
            return;
        }

        Gizmos.color = Color.black;
        for (int i = 0; i < vertices.Count; i++)
        {
            Gizmos.DrawSphere(transform.TransformPoint(vertices[i]), 0.01f);
        }
    }
}
