using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class LineMeshGenerator : MonoBehaviour
{
    [SerializeField]
    private Vector2[] points;

    [SerializeField]
    private Vector3 offset;

    [SerializeField]
    private float thickness;

    [SerializeField]
    private bool useWorldSpace;

    private MeshFilter meshFilter;
    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    public void OnValidate()
    {
        GenerateMesh();
    }

    public void SetPoints(Vector2[] points)
    {
        this.points = points;
        GenerateMesh();
    }

    public void SetPoints(PathFinding.Path path)
    {
        if (path.Nodes == null)
        {
            points = new Vector2[0];
            GenerateMesh();
            return;
        }
        points = new Vector2[path.Nodes.Count];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = path.Nodes[i].pos;
        }
        GenerateMesh();
    }

    public void LateUpdate()
    {
        
    }

    [ContextMenu("Generate")]
    private void GenerateMesh()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();
        
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        

        if (points != null && points.Length > 1)
        {
            Vector2 uv00 = new Vector2(0, 0);
            Vector2 uv10 = new Vector2(1, 0);
            Vector2 uv01 = new Vector2(0, 1);
            Vector2 uv11 = new Vector2(1, 1);

            float halfThickness = thickness / 2;

            for (int i = 0; i < points.Length; i++)
            {
                if (i % 2 == 0)
                {
                    uvs.Add(uv00);
                    uvs.Add(uv10);
                }
                else
                {
                    uvs.Add(uv01);
                    uvs.Add(uv11);
                }

                if (i == 0)
                {
                    Vector2 vec = points[i + 1] - points[i];
                    vec.Normalize();
                    vec = new Vector2(vec.y, -vec.x);

                    Vector3 p0 = points[i] + vec * halfThickness;
                    Vector3 p1 = points[i] - vec * halfThickness;

                    vertices.Add(p0);
                    vertices.Add(p1);

                    triangles.Add(i * 2 + 0);
                    triangles.Add(i * 2 + 1);
                    triangles.Add(i * 2 + 2);
                    triangles.Add(i * 2 + 1);
                    triangles.Add(i * 2 + 3);
                    triangles.Add(i * 2 + 2);
                }
                else if (i == points.Length - 1)
                {
                    Vector2 vec = points[i] - points[i - 1];
                    vec.Normalize();
                    vec = new Vector2(vec.y, -vec.x);

                    Vector3 p0 = points[i] + vec * halfThickness;
                    Vector3 p1 = points[i] - vec * halfThickness;

                    vertices.Add(p0);
                    vertices.Add(p1);
                }
                else
                {
                    Vector2 vec0 = points[i] - points[i - 1];
                    vec0.Normalize();
                    vec0 = new Vector2(vec0.y, -vec0.x);

                    Vector2 vec1 = points[i + 1] - points[i];
                    vec1.Normalize();
                    vec1 = new Vector2(vec1.y, -vec1.x);

                    if (!LineIntersection(points[i - 1] + vec0 * halfThickness, points[i] + vec0 * halfThickness, points[i] + vec1 * halfThickness, points[i + 1] + vec1 * halfThickness, out Vector2 p0))
                        p0 = points[i] + vec0 * halfThickness;

                    if (!LineIntersection(points[i - 1] - vec0 * halfThickness, points[i] - vec0 * halfThickness, points[i] - vec1 * halfThickness, points[i + 1] - vec1 * halfThickness, out Vector2 p1))
                        p1 = points[i] - vec0 * halfThickness;

                    vertices.Add(p0);
                    vertices.Add(p1);

                    triangles.Add(i * 2 + 0);
                    triangles.Add(i * 2 + 1);
                    triangles.Add(i * 2 + 2);
                    triangles.Add(i * 2 + 1);
                    triangles.Add(i * 2 + 3);
                    triangles.Add(i * 2 + 2);
                }
            }

            for (int i = 0; i < vertices.Count; i++)
                vertices[i] = new Vector3(vertices[i].x, 0, vertices[i].y);

            for (int i = 0; i < vertices.Count; i++)
                vertices[i] += offset;

            if (useWorldSpace)
            {
                for (int i = 0; i < vertices.Count; i++)
                    vertices[i] = transform.InverseTransformPoint(vertices[i]);
            }
            
        }

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        //if (meshFilter.mesh)
        //    DestroyImmediate(meshFilter.mesh);

        meshFilter.sharedMesh = mesh;
    }

    public static bool LineIntersection(Vector2 s1, Vector2 e1, Vector2 s2, Vector2 e2, out Vector2 intersection)
    {
        float a1 = e1.y - s1.y;
        float b1 = s1.x - e1.x;
        float c1 = a1 * s1.x + b1 * s1.y;

        float a2 = e2.y - s2.y;
        float b2 = s2.x - e2.x;
        float c2 = a2 * s2.x + b2 * s2.y;

        float delta = a1 * b2 - a2 * b1;
        //If lines are parallel, the result will be (NaN, NaN).
        
        if (delta == 0)
        {
            intersection = new Vector3(float.NaN, float.NaN);
            return false;
        }

        intersection = new Vector2((b2 * c1 - b1 * c2) / delta, (a1 * c2 - a2 * c1) / delta);
        return true;
    }
}
