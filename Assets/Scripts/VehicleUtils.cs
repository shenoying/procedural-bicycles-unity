using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleUtils
{
    
    public static Mesh BezierPatch(BezierCurve one, BezierCurve two, BezierCurve three, BezierCurve four, int detail)
    {
        Mesh mesh = new Mesh();
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> tris = new List<int>();

        float step = 1.0f / detail;
        
        for (float s = 0.0f; s <= 1.0f; s += step)
        {
            for (float t = 0.0f; t <= 1.0f; t += step)
            {
                Vector3 s0 = one.GetPoint(s);
                Vector3 s1 = two.GetPoint(s);
                Vector3 s2 = three.GetPoint(s);
                Vector3 s3 = four.GetPoint(s);

                BezierCurve q = new BezierCurve(s0, s1, s2, s3);
                Vector3 qs = q.GetPoint(t);

                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = qs;
                sphere.transform.localScale = 0.1f * Vector3.one;
                sphere.GetComponent<Renderer>().material.color = Color.green;

                vertices.Add(qs);
            }
        }

        /**
            (detail + 1) * (j - 1) + i - 1, 
            (detail + 1) * (j - 1) + i, 
            j * (detail + 1) + i, 
            j * (detail + 1) + i - 1
        **/

        for (int j = 1; j <= detail; j++)
        {
            for (int i = 1; i <= detail; i++) 
            {
                AddQuad (
                    tris, 
                    (detail + 1) * (j - 1) + i - 1,
                    j * (detail + 1) + i - 1,
                    j * (detail + 1) + i,  
                    (detail + 1) * (j - 1) + i 
                );
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }

    public Mesh Hood()
    {
        return null;
    }

    public Mesh Windshield()
    {
        return null;
    }

    public Mesh Bumper()
    {
        return null;
    }

    public Mesh CreateWheel()
    {
        return null;
    }

    public static Mesh CreateCylinder(Vector3 start, Vector3 end, float radius, int detail)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        Vector3 axis = (end - start).normalized;

        Vector3 cross = NonZeroCrossProduct(axis);

        Vector3 u = Vector3.Cross(axis, cross).normalized;
        Vector3 v = Vector3.Cross(axis, u).normalized;

        float theta = 0.0f;

        for (int i = 0; i < detail; i++) 
        {
            theta = ((i * 1.0f) / detail) * 2.0f * Mathf.PI;
            Vector3 p = radius * ((u * Mathf.Cos(theta)) + (v * Mathf.Sin(theta)));
            Vector3 vertex = p + start;
            vertices.Add(vertex);
        }

        theta = 0.0f;

        for (int i = 0; i < detail; i++) 
        {
            theta = ((i * 1.0f) / detail) * 2.0f * Mathf.PI;
            Vector3 p = radius * ((u * Mathf.Cos(theta)) + (v * Mathf.Sin(theta)));
            Vector3 vertex = p + end;
            vertices.Add(vertex);
        }
        
        for (int i = 0; i < detail; i++) 
        {
            int i1 = (i + 1) % detail;
            int i2 = i1 + detail;
            int i4 = i;
            int i3 = i4 + detail;

            AddQuad(triangles, i1, i2, i3, i4);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }

    public static void AddTriangle(List<int> tris, int i1, int i2, int i3) 
    {
        tris.Add(i1);
        tris.Add(i2);
        tris.Add(i3);
    }

    public static void AddQuad(List<int> tris, int i1, int i2, int i3, int i4) 
    {
        AddTriangle(tris, i1, i2, i3);
        AddTriangle(tris, i1, i3, i4);
    }

    public static Vector3 NonZeroCrossProduct(Vector3 vector) 
    {
        Vector3 cross = new Vector3(Random.Range(0.0f, 1.0f), 
                                    Random.Range(0.0f, 1.0f),
                                    Random.Range(0.0f, 1.0f)).normalized;
        
        while (Vector3.Cross(vector, cross) == Vector3.zero) {
            cross = new Vector3(Random.Range(0.0f, 1.0f), 
                                Random.Range(0.0f, 1.0f),
                                Random.Range(0.0f, 1.0f)).normalized;
        }

        return cross.normalized;
    }

}
