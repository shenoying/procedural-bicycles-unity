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

                vertices.Add(qs);
            }
        }
        
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
                
                AddQuad (
                    tris, 
                    (detail + 1) * (j - 1) + i,
                    j * (detail + 1) + i,  
                    j * (detail + 1) + i - 1,
                    (detail + 1) * (j - 1) + i - 1
                );
            
            }
        }

        // for (int i = 0; i < vertices.Count; i++)
        // {
        //     GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //     s.transform.position = vertices[i];
        //     s.transform.localScale = 0.1f * Vector3.one;
        //     s.GetComponent<Renderer>().material.color = Color.white;
        // }

        // List<Vector3> controlPoints = new List<Vector3>() {
        //     one.POne,   one.PTwo,   one.PThree,     one.PFour,
        //     two.POne,   two.PTwo,   two.PThree,     two.PFour,
        //     three.POne, three.PTwo, three.PThree,   three.PFour,
        //     four.POne,  four.PTwo,  four.PThree,    four.PFour
        // };

        // for (int i = 0; i < controlPoints.Count; i++)
        // {
        //     GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //     s.transform.position = controlPoints[i];
        //     s.transform.localScale = 0.1f * Vector3.one;
        //     s.GetComponent<Renderer>().material.color = Color.green;
        // }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }

    public static Mesh CreateCatmullRomTube(CatmullRomCurve cr, float radius, int detail, bool looped=false)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        int l = cr.ControlPoints.Count;
        
        for (int i = 0; i < l; i++)
        {
            ///TODO: find relationship between t value and control point index
            float t = (float) i;

            Vector3 point = cr.GetPoint(t);
            Vector3 tangent = cr.GetTangent(t);

            if (i == l - 1) tangent = (cr.ControlPoints[l - 1] - cr.ControlPoints[l - 2]).normalized;
            else if (i == l - 2) tangent = (cr.ControlPoints[i + 1] - cr.ControlPoints[i]).normalized;

            Vector3 normal = cr.GetNormal(t); 
            Vector3 binormal = cr.GetBinormal(t);
            
            normal = Vector3.Cross(tangent, NonZeroCrossProduct(tangent)).normalized;
            binormal = Vector3.Cross(tangent, normal).normalized;

            //Debug.Log("Point: " + point + ", t: " + t +  ", P(t): " + cr.GetPoint(t) + ", T: " + tangent + ", T(t): " + cr.GetTangent(t) + ", N: " + normal + ", N(t): " + cr.GetNormal(t) + ", B: " + binormal + ", B(t): " + cr.GetBinormal(t));

            float theta = 0.0f;

            for (int d = 0; d < detail; d++)
            {
                theta = ((d * 1.0f) / detail) * 2.0f * Mathf.PI;
                Vector3 p = radius * ((normal * Mathf.Cos(theta)) + (binormal * Mathf.Sin(theta)));
                vertices.Add(cr.ControlPoints[i] + p);
            }
        }

        for (int i = 0; i < (l - 1) * detail; i += detail)
        {
            for (int d = i; d < i + detail; d++)
            {
                int i1 = i + ((d + 1) % detail);
                int i2 = i1 + detail;
                int i3 = d + detail;
                int i4 = d;

                //AddQuad(triangles, i4, i3, i2, i1);
                AddQuad(triangles, i1, i2, i3, i4);
            }
        }

        if (looped)
        {
            for (int i = 0; i < detail; i++) 
            {
                int d2 = detail / 2;
                int i1 = (i + d2 + 1) % detail;
                int i2 = (detail * (l - 1)) + (i + d2 + 3) % detail;
                int i3 = i + (detail * (l - 1));
                int i4 = (i + d2 + 2) % detail; 

                AddQuad(triangles, i1, i2, i3, i4);
            }
        }

        /**
        for (int i = 0; i < vertices.Count; i++)
        {
            GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.transform.position = vertices[i];
            s.transform.localScale = 0.1f * Vector3.one;
        }
        **/

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
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
        Vector3 cross = Vector3.Cross(vector, Vector3.up).normalized;
        if (cross.magnitude == 0.0f)
        {
            cross = Vector3.right;
        }

        return cross;
    }

    public static Color GenerateRandomColor()
    {
        return new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
    }

    public static GameObject Handlebars(Vector3 position, float thickness, float width, int detail)
    {
        float choice = Random.Range(0.0f, 3.0f);
        //Debug.Log("handlebar choice: " + choice);
        if (choice > 0.0f && choice <= 1.0f) 
            return SinWaveHandlebars(position, thickness, width, detail);
        else if (choice > 1.0f && choice <= 2.0f) 
            return DropbarHandlebars(position, thickness, width, detail);
        else if (choice > 2.0f && choice <= 3.0f)
            return BullhornHandlebars(position, thickness, width, detail);
        else return SinWaveHandlebars(position, thickness, width, detail);
    }

    static GameObject SinWaveHandlebars(Vector3 position, float thickness, float width, int detail)
    {
        CatmullRomCurve curve;
        List<Vector3> verts = new List<Vector3>();

        GameObject go = new GameObject("Handlebars");
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.transform.rotation = Quaternion.Euler(0.0f, -90f, 0.0f);

        for (float i = 0.0f; i < 32.0f; i += 1.0f)
        {
            float theta = (i / 32.0f) * 2.0f * Mathf.PI;
            verts.Add (
                new Vector3 (
                    (theta / (2.0f * Mathf.PI) * width) - width / 2.0f, 
                    0.35f, 
                    -Mathf.Abs(Mathf.Sin(theta) * Mathf.Sin(theta)) // + width / 2.0f
                )
            );
        }

        curve = new CatmullRomCurve(verts);
        go.GetComponent<MeshFilter>().mesh = CreateCatmullRomTube(curve, thickness, detail, false);
        go.GetComponent<Renderer>().material.color = GenerateRandomColor();
        go.transform.position = position;

        return go;
    }

    static GameObject DropbarHandlebars(Vector3 position, float thickness, float width, int detail)
    {
        CatmullRomCurve curve;
        List<Vector3> verts = new List<Vector3>();

        GameObject go = new GameObject("Handlebars");
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.transform.rotation = Quaternion.Euler(0.0f, -90f, 0.0f);

        for (float i = 0.0f; i < 32.0f; i += 1.0f)
        {
            float theta = (i / 32.0f) * (Mathf.PI + 0.101325f);
            float offset = Mathf.Abs(Mathf.Sqrt(Mathf.Sqrt(Mathf.Sin(theta))));
            verts.Add (
                new Vector3 (
                    (theta / (Mathf.PI) * width) - width / 2.0f, 
                    -offset + 1.0f + 0.4f * (width / 1.5f), 
                    -offset + width / 1.85f
                )
            );
        }

        curve = new CatmullRomCurve(verts);
        go.GetComponent<MeshFilter>().mesh = CreateCatmullRomTube(curve, thickness, detail, false);
        go.GetComponent<Renderer>().material.color = GenerateRandomColor();
        go.transform.position = position;

        return go;
    }

    static GameObject BullhornHandlebars(Vector3 position, float thickness, float width, int detail)
    {
        CatmullRomCurve curve;
        List<Vector3> verts = new List<Vector3>();

        GameObject go = new GameObject("Handlebars");
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.transform.rotation = Quaternion.Euler(0.0f, -90f, 0.0f);

        for (float i = 0.0f; i < 32.0f; i += 1.0f)
        {
            float theta = (i / 32.0f) * (2.0f * Mathf.PI + 0.20265f);
            verts.Add (
                new Vector3 (
                    (theta / (2.0f * Mathf.PI) * width) - width / 2.0f, 
                    -Mathf.Pow(Mathf.Abs(Mathf.Sin(theta)), 0.25f) + 1.15f * (width / 1.5f), 
                    -Mathf.Abs(Mathf.Pow(Mathf.Sin(theta), 1.0f)) // + width / 3.0f
                )
            );
        }

        curve = new CatmullRomCurve(verts);
        go.GetComponent<MeshFilter>().mesh = CreateCatmullRomTube(curve, thickness, detail, false);
        go.GetComponent<Renderer>().material.color = GenerateRandomColor();
        go.transform.position = position;

        return go;
    }

    public static GameObject Seat(Vector3 position, int detail)
    {
        float choice = Random.Range(0.0f, 3.0f);
        //Debug.Log("seat choice: " + choice);
        if (choice > 0.0f && choice <= 1.0f)
            return CushionedSeat(position, detail);
        else if (choice > 1.0f && choice <= 2.0f)
            return DivetedSeat(position, detail);
        else if (choice > 2.0f && choice <= 3.0f)
            return RoundedSeat(position, detail);
        else return CushionedSeat(position, detail);
    }

    static GameObject CushionedSeat(Vector3 position, int detail)
    {
        BezierCurve s1 = new BezierCurve (
            new Vector3(0.4f, 0.0f,  0.5f),
            new Vector3(0.65f, 0.0f,  0.25f),
            new Vector3(0.65f, 0.0f, -0.25f),
            new Vector3(0.4f, 0.0f, -0.5f)
        );
        BezierCurve s2 = new BezierCurve (
            new Vector3(0.08f, 0.0f,  0.6f),
            new Vector3(0.24f, 0.5f,  0.1f),
            new Vector3(0.24f, 0.5f, -0.1f),
            new Vector3(0.08f, 0.0f, -0.6f)
        );
        BezierCurve s3 = new BezierCurve (
            new Vector3(-0.08f, 0.0f,  0.6f),
            new Vector3(-0.24f, 0.5f,  0.1f),
            new Vector3(-0.24f, 0.5f, -0.1f),
            new Vector3(-0.08f, 0.0f, -0.6f)
        );
        BezierCurve s4 = new BezierCurve (
            new Vector3(-0.4f, 0.0f,  0.5f),
            new Vector3(-0.65f, 0.0f,  0.25f),
            new Vector3(-0.65f, 0.0f, -0.25f),
            new Vector3(-0.4f, 0.0f, -0.5f)
        );

        BezierCurve u1 = new BezierCurve (
            new Vector3(0.4f, 0.0f,  0.5f),
            new Vector3(0.65f, 0.0f,  0.25f),
            new Vector3(0.65f, 0.0f, -0.25f),
            new Vector3(0.4f, 0.0f, -0.5f)
        );
        BezierCurve u2 = new BezierCurve (
            new Vector3(0.08f,  0.0f,   0.6f),
            new Vector3(0.24f, -0.25f,  0.1f),
            new Vector3(0.24f, -0.25f, -0.1f),
            new Vector3(0.08f,  0.0f,  -0.6f)
        );
        BezierCurve u3 = new BezierCurve (
            new Vector3(-0.08f,  0.0f,   0.6f),
            new Vector3(-0.24f, -0.25f,  0.1f),
            new Vector3(-0.24f, -0.25f, -0.1f),
            new Vector3(-0.08f,  0.0f,  -0.6f)
        );
        BezierCurve u4 = new BezierCurve (
            new Vector3(-0.4f, 0.0f,  0.5f),
            new Vector3(-0.65f, 0.0f,  0.25f),
            new Vector3(-0.65f, 0.0f, -0.25f),
            new Vector3(-0.4f, 0.0f, -0.50f)
        );

        BezierCurve m1 = new BezierCurve (
            new Vector3(0.18f,  0.12f, 0.13f),
            new Vector3(0.40f,  0.08f, 0.22f),
            new Vector3(0.40f, -0.08f, 0.22f),
            new Vector3(0.18f, -0.12f, 0.13f)
        );
        BezierCurve m2 = new BezierCurve (
            new Vector3(0.04f,  0.12f, 0.9f),
            new Vector3(0.06f,  0.08f, 1.6f),
            new Vector3(0.06f, -0.08f, 1.6f),
            new Vector3(0.04f, -0.12f, 0.9f)
        );
        BezierCurve m3 = new BezierCurve (
            new Vector3(-0.04f,  0.12f, 0.9f),
            new Vector3(-0.06f,  0.08f, 1.6f),
            new Vector3(-0.06f, -0.08f, 1.6f),
            new Vector3(-0.04f, -0.12f, 0.9f)
        );
        BezierCurve m4 = new BezierCurve (
            new Vector3(-0.18f,  0.12f, 0.13f),
            new Vector3(-0.40f,  0.08f, 0.22f),
            new Vector3(-0.40f, -0.08f, 0.22f),
            new Vector3(-0.18f, -0.12f, 0.13f)
        );

        Mesh frontCushion = BezierPatch(m1, m2, m3, m4, detail);
        Mesh backCushion = BezierPatch(s1, s2, s3, s4, detail);
        Mesh underCushion = BezierPatch(u1, u2, u3, u4, detail);

        GameObject go = new GameObject("SeatParent");
        
        GameObject fc = new GameObject("FrontCushion");
        fc.AddComponent<MeshFilter>();
        fc.AddComponent<MeshRenderer>();
        fc.transform.parent = go.transform;
        fc.GetComponent<MeshFilter>().mesh = frontCushion;
        fc.GetComponent<Renderer>().material.color = Color.black;

        GameObject bc = new GameObject("BackCushion");
        bc.AddComponent<MeshFilter>();
        bc.AddComponent<MeshRenderer>();
        bc.transform.parent = go.transform;
        bc.GetComponent<MeshFilter>().mesh = backCushion;
        bc.GetComponent<Renderer>().material.color = Color.black;

        GameObject uc = new GameObject("UnderCushion");
        uc.AddComponent<MeshFilter>();
        uc.AddComponent<MeshRenderer>();
        uc.transform.parent = go.transform;
        uc.GetComponent<MeshFilter>().mesh = underCushion;
        uc.GetComponent<Renderer>().material.color = Color.black;

        go.transform.rotation = Quaternion.Euler(0.0f, -90.0f, 0.0f);
        go.transform.position = position;
        
        return go;
    }

    static GameObject DivetedSeat(Vector3 position, int detail)
    {
        BezierCurve l1 = new BezierCurve (
            new Vector3(0.4f,  0.0f,  0.5f),
            new Vector3(0.65f, 0.0f,  0.25f),
            new Vector3(0.65f, 0.0f, -0.25f),
            new Vector3(0.4f,  0.0f, -0.5f)
        );
        BezierCurve l2 = new BezierCurve (
            new Vector3(0.24f,  0.0f,  0.6f),
            new Vector3(0.36f,  0.50f,  0.1f),
            new Vector3(0.36f,  0.50f, -0.1f),
            new Vector3(0.24f,  0.0f, -0.6f)
        );
        BezierCurve l3 = new BezierCurve (
            new Vector3(0.18f,  0.0f,  0.6f),
            new Vector3(0.24f,  0.50f,  0.1f),
            new Vector3(0.24f,  0.50f, -0.1f),
            new Vector3(0.18f,  0.0f, -0.6f)
        );
        BezierCurve l4 = new BezierCurve (
            new Vector3(0.08f, 0.0f, 0.5f),
            new Vector3(0.24f, 0.0f,  0.25f),
            new Vector3(0.24f, 0.0f, -0.25f),
            new Vector3(0.08f, 0.0f, -0.5f)
        );

        BezierCurve r1 = new BezierCurve (
            new Vector3(-0.4f,  0.0f,  0.5f),
            new Vector3(-0.65f, 0.0f,  0.25f),
            new Vector3(-0.65f, 0.0f, -0.25f),
            new Vector3(-0.4f,  0.0f, -0.5f)
        );
        BezierCurve r2 = new BezierCurve (
            new Vector3(-0.24f,  0.0f,  0.6f),
            new Vector3(-0.36f,  0.50f,  0.1f),
            new Vector3(-0.36f,  0.50f, -0.1f),
            new Vector3(-0.24f,  0.0f, -0.6f)
        );
        BezierCurve r3 = new BezierCurve (
            new Vector3(-0.18f,  0.0f,  0.6f),
            new Vector3(-0.30f,  0.50f,  0.1f),
            new Vector3(-0.30f,  0.50f, -0.1f),
            new Vector3(-0.18f,  0.0f, -0.6f)
        );
        BezierCurve r4 = new BezierCurve (
            new Vector3(-0.08f, 0.0f, 0.5f),
            new Vector3(-0.24f, 0.0f,  0.25f),
            new Vector3(-0.24f, 0.0f, -0.25f),
            new Vector3(-0.08f, 0.0f, -0.5f)
        );

        BezierCurve u1 = new BezierCurve (
            new Vector3(0.4f, 0.0f,  0.5f),
            new Vector3(0.65f, 0.0f,  0.25f),
            new Vector3(0.65f, 0.0f, -0.25f),
            new Vector3(0.4f, 0.0f, -0.5f)
        );
        BezierCurve u2 = new BezierCurve (
            new Vector3(0.08f,  0.0f,   0.6f),
            new Vector3(0.24f, -0.25f,  0.1f),
            new Vector3(0.24f, -0.25f, -0.1f),
            new Vector3(0.08f,  0.0f,  -0.6f)
        );
        BezierCurve u3 = new BezierCurve (
            new Vector3(-0.08f,  0.0f,   0.6f),
            new Vector3(-0.24f, -0.25f,  0.1f),
            new Vector3(-0.24f, -0.25f, -0.1f),
            new Vector3(-0.08f,  0.0f,  -0.6f)
        );
        BezierCurve u4 = new BezierCurve (
            new Vector3(-0.4f, 0.0f,  0.5f),
            new Vector3(-0.65f, 0.0f,  0.25f),
            new Vector3(-0.65f, 0.0f, -0.25f),
            new Vector3(-0.4f, 0.0f, -0.50f)
        );

        BezierCurve m1 = new BezierCurve (
            new Vector3(0.18f,  0.12f, 0.13f),
            new Vector3(0.40f,  0.08f, 0.22f),
            new Vector3(0.40f, -0.08f, 0.22f),
            new Vector3(0.18f, -0.12f, 0.13f)
        );
        BezierCurve m2 = new BezierCurve (
            new Vector3(0.04f,  0.12f, 0.9f),
            new Vector3(0.06f,  0.08f, 1.6f),
            new Vector3(0.06f, -0.08f, 1.6f),
            new Vector3(0.04f, -0.12f, 0.9f)
        );
        BezierCurve m3 = new BezierCurve (
            new Vector3(-0.04f,  0.12f, 0.9f),
            new Vector3(-0.06f,  0.08f, 1.6f),
            new Vector3(-0.06f, -0.08f, 1.6f),
            new Vector3(-0.04f, -0.12f, 0.9f)
        );
        BezierCurve m4 = new BezierCurve (
            new Vector3(-0.18f,  0.12f, 0.13f),
            new Vector3(-0.40f,  0.08f, 0.22f),
            new Vector3(-0.40f, -0.08f, 0.22f),
            new Vector3(-0.18f, -0.12f, 0.13f)
        );

        Mesh frontCushion = BezierPatch(m1, m2, m3, m4, detail);
        Mesh topLCushion = BezierPatch(l1, l2, l3, l4, detail);
        Mesh topRCushion = BezierPatch(r1, r2, r3, r4, detail);
        Mesh underCushion = BezierPatch(u1, u2, u3, u4, detail);

        GameObject go = new GameObject("SeatParent");
        
        GameObject fc = new GameObject("FrontCushion");
        fc.AddComponent<MeshFilter>();
        fc.AddComponent<MeshRenderer>();
        fc.transform.parent = go.transform;
        fc.GetComponent<MeshFilter>().mesh = frontCushion;
        fc.GetComponent<Renderer>().material.color = Color.black;

        GameObject tlc = new GameObject("TopLCushion");
        tlc.AddComponent<MeshFilter>();
        tlc.AddComponent<MeshRenderer>();
        tlc.transform.parent = go.transform;
        tlc.GetComponent<MeshFilter>().mesh = topLCushion;
        tlc.GetComponent<Renderer>().material.color = Color.black;

        GameObject trc = new GameObject("TopRCushion");
        trc.AddComponent<MeshFilter>();
        trc.AddComponent<MeshRenderer>();
        trc.transform.parent = go.transform;
        trc.GetComponent<MeshFilter>().mesh = topRCushion;
        trc.GetComponent<Renderer>().material.color = Color.black;

        GameObject uc = new GameObject("UnderCushion");
        uc.AddComponent<MeshFilter>();
        uc.AddComponent<MeshRenderer>();
        uc.transform.parent = go.transform;
        uc.GetComponent<MeshFilter>().mesh = underCushion;
        uc.GetComponent<Renderer>().material.color = Color.black;

        go.transform.rotation = Quaternion.Euler(0.0f, -90.0f, 0.0f);
        go.transform.position = position;

        return go;
    }

    static GameObject RoundedSeat(Vector3 position, int detail)
    {
        BezierCurve s1 = new BezierCurve (
            new Vector3(0.4f, 0.0f,  0.5f),
            new Vector3(0.65f, 0.0f,  0.25f),
            new Vector3(0.65f, 0.0f, -0.25f),
            new Vector3(0.4f, 0.0f, -0.5f)
        );
        BezierCurve s2 = new BezierCurve (
            new Vector3(0.08f, 0.0f,  0.6f),
            new Vector3(0.24f, 0.5f,  0.1f),
            new Vector3(0.24f, 0.5f, -0.1f),
            new Vector3(0.08f, 0.0f, -0.6f)
        );
        BezierCurve s3 = new BezierCurve (
            new Vector3(-0.08f, 0.0f,  0.6f),
            new Vector3(-0.24f, 0.5f,  0.1f),
            new Vector3(-0.24f, 0.5f, -0.1f),
            new Vector3(-0.08f, 0.0f, -0.6f)
        );
        BezierCurve s4 = new BezierCurve (
            new Vector3(-0.4f, 0.0f,  0.5f),
            new Vector3(-0.65f, 0.0f,  0.25f),
            new Vector3(-0.65f, 0.0f, -0.25f),
            new Vector3(-0.4f, 0.0f, -0.5f)
        );

        BezierCurve u1 = new BezierCurve (
            new Vector3(0.4f, 0.0f,  0.5f),
            new Vector3(0.65f, 0.0f,  0.25f),
            new Vector3(0.65f, 0.0f, -0.25f),
            new Vector3(0.4f, 0.0f, -0.5f)
        );
        BezierCurve u2 = new BezierCurve (
            new Vector3(0.08f,  0.0f,   0.6f),
            new Vector3(0.24f, -0.25f,  0.1f),
            new Vector3(0.24f, -0.25f, -0.1f),
            new Vector3(0.08f,  0.0f,  -0.6f)
        );
        BezierCurve u3 = new BezierCurve (
            new Vector3(-0.08f,  0.0f,   0.6f),
            new Vector3(-0.24f, -0.25f,  0.1f),
            new Vector3(-0.24f, -0.25f, -0.1f),
            new Vector3(-0.08f,  0.0f,  -0.6f)
        );
        BezierCurve u4 = new BezierCurve (
            new Vector3(-0.4f, 0.0f,  0.5f),
            new Vector3(-0.65f, 0.0f,  0.25f),
            new Vector3(-0.65f, 0.0f, -0.25f),
            new Vector3(-0.4f, 0.0f, -0.50f)
        );

        BezierCurve m1 = new BezierCurve (
            new Vector3(0.28f,  0.22f, 0.13f),
            new Vector3(0.55f,  0.18f, 0.22f),
            new Vector3(0.55f,  0.02f, 0.22f),
            new Vector3(0.28f, -0.02f, 0.13f)
        );
        BezierCurve m2 = new BezierCurve (
            new Vector3(0.04f,  0.22f, 0.9f),
            new Vector3(0.06f,  0.18f, 1.6f),
            new Vector3(0.06f,  0.02f, 1.6f),
            new Vector3(0.04f, -0.02f, 0.9f)
        );
        BezierCurve m3 = new BezierCurve (
            new Vector3(-0.04f,  0.22f, 0.9f),
            new Vector3(-0.06f,  0.18f, 1.6f),
            new Vector3(-0.06f,  0.02f, 1.6f),
            new Vector3(-0.04f, -0.02f, 0.9f)
        );
        BezierCurve m4 = new BezierCurve (
            new Vector3(-0.28f,  0.22f, 0.13f),
            new Vector3(-0.55f,  0.18f, 0.22f),
            new Vector3(-0.55f,  0.02f, 0.22f),
            new Vector3(-0.28f, -0.02f, 0.13f)
        );

        Mesh frontCushion = BezierPatch(m1, m2, m3, m4, detail);
        Mesh backCushion = BezierPatch(s1, s2, s3, s4, detail);
        Mesh underCushion = BezierPatch(u1, u2, u3, u4, detail);

        GameObject go = new GameObject("SeatParent");
        
        GameObject fc = new GameObject("FrontCushion");
        fc.AddComponent<MeshFilter>();
        fc.AddComponent<MeshRenderer>();
        fc.transform.parent = go.transform;
        fc.GetComponent<MeshFilter>().mesh = frontCushion;
        fc.GetComponent<Renderer>().material.color = Color.black;

        GameObject bc = new GameObject("BackCushion");
        bc.AddComponent<MeshFilter>();
        bc.AddComponent<MeshRenderer>();
        bc.transform.parent = go.transform;
        bc.GetComponent<MeshFilter>().mesh = backCushion;
        bc.GetComponent<Renderer>().material.color = Color.black;

        GameObject uc = new GameObject("UnderCushion");
        uc.AddComponent<MeshFilter>();
        uc.AddComponent<MeshRenderer>();
        uc.transform.parent = go.transform;
        uc.GetComponent<MeshFilter>().mesh = underCushion;
        uc.GetComponent<Renderer>().material.color = Color.black;

        go.transform.rotation = Quaternion.Euler(0.0f, -90.0f, 0.0f);
        go.transform.position = position;

        return go;
    }

    public static GameObject Body(Vector3 center, float width, float height)
    {
        Vector3 wheelOnePos = center - new Vector3(width / 2.0f, 0.0f, 0.0f);
        Vector3 wheelTwoPos = center + new Vector3(width / 2.0f, 0.0f, 0.0f);
        Vector3 handlebarPos = wheelOnePos + new Vector3(0.2f * width, height, 0.0f);
        Vector3 seatPos = wheelTwoPos + new Vector3(-0.15f * width, 0.85f * height, 0.0f);

        GameObject body = new GameObject("Body");

        Color c = GenerateRandomColor();

        GameObject ft = new GameObject("FrontTube");
        ft.AddComponent<MeshFilter>();
        ft.AddComponent<MeshRenderer>();
        ft.transform.parent = body.transform;
        ft.GetComponent<MeshFilter>().mesh = CreateCylinder(wheelOnePos, handlebarPos + 0.15f * (handlebarPos - wheelOnePos), 0.10f, 8);
        ft.GetComponent<Renderer>().material.color = c;

        GameObject tt = new GameObject("TopTube");
        tt.AddComponent<MeshFilter>();
        tt.AddComponent<MeshRenderer>();
        tt.transform.parent = body.transform;
        tt.GetComponent<MeshFilter>().mesh = CreateCylinder(handlebarPos, seatPos, 0.10f, 8);
        tt.GetComponent<Renderer>().material.color = c;

        GameObject ct = new GameObject("CenterTube");
        ct.AddComponent<MeshFilter>();
        ct.AddComponent<MeshRenderer>();
        ct.transform.parent = body.transform;
        ct.GetComponent<MeshFilter>().mesh = CreateCylinder(handlebarPos, center, 0.10f, 8);
        ct.GetComponent<Renderer>().material.color = c;

        GameObject st = new GameObject("SupportTube");
        st.AddComponent<MeshFilter>();
        st.AddComponent<MeshRenderer>();
        st.transform.parent = body.transform;
        st.GetComponent<MeshFilter>().mesh = CreateCylinder(center, seatPos + 0.15f * (seatPos - center), 0.10f, 8);
        st.GetComponent<Renderer>().material.color = c;

        GameObject rt = new GameObject("RearTube");
        rt.AddComponent<MeshFilter>();
        rt.AddComponent<MeshRenderer>();
        rt.transform.parent = body.transform;
        rt.GetComponent<MeshFilter>().mesh = CreateCylinder(wheelTwoPos, seatPos, 0.10f, 8);
        rt.GetComponent<Renderer>().material.color = c;

        GameObject bt = new GameObject("BottomTube");
        bt.AddComponent<MeshFilter>();
        bt.AddComponent<MeshRenderer>();
        bt.transform.parent = body.transform;
        bt.GetComponent<MeshFilter>().mesh = CreateCylinder(wheelTwoPos, center, 0.10f, 8);
        bt.GetComponent<Renderer>().material.color = c;

        return body;
    }

    public static GameObject CreatePedals(Vector3 position, float radius, float thickness, int detail)
    {
        CatmullRomCurve pedalWheel;
        List<Vector3> verts = new List<Vector3>();

        GameObject pedalsLeft = new GameObject("PedalsLeft");
        pedalsLeft.AddComponent<MeshFilter>();
        pedalsLeft.AddComponent<MeshRenderer>();

        for (float i = 0; i < 24.0f; i += 1.0f)
        {
            float t = (i / 24.0f) * (2.0f * Mathf.PI);
            verts.Add(new Vector3(position.x + radius * Mathf.Cos(t), position.y + radius * Mathf.Sin(t), position.z + 0.18f * radius));
        }
        
        pedalWheel = new CatmullRomCurve(verts);
        pedalsLeft.GetComponent<MeshFilter>().mesh = CreateCatmullRomTube(pedalWheel, radius, detail, true);
        pedalsLeft.GetComponent<Renderer>().material.color = Color.black;

        verts.Clear();

        GameObject pedalsRight = new GameObject("PedalsRight");
        pedalsRight.AddComponent<MeshFilter>();
        pedalsRight.AddComponent<MeshRenderer>();

        for (float i = 0; i < 24.0f; i += 1.0f)
        {
            float t = (i / 24.0f) * (2.0f * Mathf.PI);
            verts.Add(new Vector3(position.x + radius * Mathf.Cos(t), position.y + radius * Mathf.Sin(t), position.z - 0.18f * radius));
        }
        
        pedalWheel = new CatmullRomCurve(verts);
        pedalsRight.GetComponent<MeshFilter>().mesh = CreateCatmullRomTube(pedalWheel, radius, detail, true);
        pedalsRight.GetComponent<Renderer>().material.color = Color.black;

        float theta = Random.Range(0.0f, 2.0f * Mathf.PI);

        GameObject pedalLegL = new GameObject("PedalLegL");
        pedalLegL.AddComponent<MeshFilter>();
        pedalLegL.AddComponent<MeshRenderer>();
        pedalLegL.transform.parent = pedalsLeft.transform;

        GameObject pedalLegR = new GameObject("PedalLegR");
        pedalLegR.AddComponent<MeshFilter>();
        pedalLegR.AddComponent<MeshRenderer>();
        pedalLegR.transform.parent = pedalsLeft.transform;

        pedalLegL.GetComponent<MeshFilter>().mesh = CreateCylinder(
            3.0f * radius * (new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0.06f)),
            3.0f * radius * (new Vector3(Mathf.Cos(theta + Mathf.PI), Mathf.Sin(theta + Mathf.PI), 0.06f)),
            0.25f * radius,
            detail
        );
        pedalLegL.GetComponent<Renderer>().material.color = Color.black;
        pedalLegL.transform.position = position;
        
        pedalLegR.GetComponent<MeshFilter>().mesh = CreateCylinder(
            3.0f * radius * (new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), -0.06f)),
            3.0f * radius * (new Vector3(Mathf.Cos(theta + Mathf.PI), Mathf.Sin(theta + Mathf.PI), -0.06f)),
            0.25f * radius,
            detail
        );
        pedalLegR.GetComponent<Renderer>().material.color = Color.black;
        pedalLegR.transform.position = position;

        GameObject pedalOne = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pedalOne.transform.parent = pedalsLeft.transform;
        pedalOne.transform.localScale = radius * new Vector3(1.0f, 0.25f, 1.0f);
        pedalOne.transform.position = position + 3.0f * radius * (new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0.06f));
        //pedalOne.transform.position = position;

        GameObject pedalTwo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pedalTwo.transform.parent = pedalsLeft.transform; 
        pedalTwo.transform.localScale = radius * new Vector3(1.0f, 0.25f, 1.0f);
        pedalTwo.transform.position = position + 3.0f * radius * (new Vector3(Mathf.Cos(theta + Mathf.PI), Mathf.Sin(theta + Mathf.PI), -0.06f));
        //pedalTwo.transform.position = position;

        return pedalsLeft;
    }

    public static GameObject CreateWheel(Vector3 position, float radius, float thickness, int detail)
    {
        CatmullRomCurve curve; 
        List<Vector3> verts = new List<Vector3>();

        GameObject wheel = new GameObject("Wheel");
        wheel.AddComponent<MeshFilter>();
        wheel.AddComponent<MeshRenderer>();

        for (float i = 0; i < 24.0f; i += 1.0f)
        {
            float theta = (i / 24.0f) * (2.0f * Mathf.PI); // + 0.20265f);
            verts.Add(new Vector3(position.x + radius * Mathf.Cos(theta), position.y + radius * Mathf.Sin(theta), position.z));
        }

        curve = new CatmullRomCurve(verts);
        wheel.GetComponent<MeshFilter>().mesh = CreateCatmullRomTube(curve, thickness, detail, true);
        wheel.GetComponent<Renderer>().material.color = Color.black;

        for (int i = 0; i < 32; i++)
        {
            GameObject s = new GameObject("Spoke " + i);
            s.AddComponent<MeshFilter>();
            s.AddComponent<MeshRenderer>();

            float theta = (i / 32.0f) * (2.0f * Mathf.PI + 0.20265f);
            Vector3 off = new Vector3(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta), 0.0f);
            s.GetComponent<MeshFilter>().mesh = CreateCylinder(position, position + off, 0.01f, 3);
            s.GetComponent<Renderer>().material.color = GenerateRandomColor();
            s.transform.parent = wheel.transform;
        }

        return wheel;
    }

}
