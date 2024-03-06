using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class SplineUtils
{ 
    public enum SplineTypes
    {
        CUBIC_BEZIER,
        HERMITE,
        CATMULL_ROM,
        B_SPLINE,
        CARDINAL,
        LINEAR
    };

    public static readonly Matrix4x4 cubicBezierBasis = new Matrix4x4 (
        new Vector4(1, -3,  3, -1),
        new Vector4(0,  3, -6,  3),
        new Vector4(0,  0,  3, -3),
        new Vector4(0,  0,  0,  1)
    );

    public static readonly Matrix4x4 hermiteBasis = new Matrix4x4 (
        new Vector4(1,  0, -3,  2),
        new Vector4(0,  1, -2,  1),
        new Vector4(0,  0,  3, -2),
        new Vector4(0,  0, -1,  1)
    );

    public static readonly Matrix4x4 catmullRomBasis = new Matrix4x4 (
        new Vector4(0.0f, -0.5f,   1.0f, -0.5f),
        new Vector4(1.0f,  0.0f,  -2.5f,  1.5f),
        new Vector4(0.0f,  0.5f,   2.0f, -1.5f),
        new Vector4(0.0f,  0.0f,  -0.5f,  0.5f)
    );

    public static readonly Matrix4x4 catmullRomTangent = new Matrix4x4 (
        new Vector4(-0.5f,  2.0f, -1.5f, 0.0f),
        new Vector4( 0.0f, -5.0f,  4.5f, 0.0f),
        new Vector4( 0.5f,  4.0f, -4.5f, 0.0f),
        new Vector4( 0.0f, -1.0f,  1.5f, 0.0f)
    );

    public static readonly Matrix4x4 catmullRomNormal = new Matrix4x4 (
        new Vector4( 2.0f, -3.0f, 0.0f, 0.0f),
        new Vector4(-5.0f,  9.0f, 0.0f, 0.0f),
        new Vector4( 4.0f, -9.0f, 0.0f, 0.0f),
        new Vector4(-1.0f,  3.0f, 0.0f, 0.0f)
    );

    public static readonly Matrix4x4 catmullRomBinormal = new Matrix4x4 (
        new Vector4(-3.0f, 0.0f, 0.0f, 0.0f),
        new Vector4( 9.0f, 0.0f, 0.0f, 0.0f),
        new Vector4(-9.0f, 0.0f, 0.0f, 0.0f),
        new Vector4( 3.0f, 0.0f, 0.0f, 0.0f)
    );

    public static readonly Matrix4x4 bSplineBasis = new Matrix4x4 (
        new Vector4(1.0f / 6.0f, -0.5f,  0.5f, -1.0f / 6.0f),
        new Vector4(2.0f / 3.0f,  0.0f, -1.0f,         0.5f),
        new Vector4(1.0f / 6.0f,  0.5f,  0.5f,        -0.5f),
        new Vector4(0.0f,         0.0f,  0.0f,  1.0f / 6.0f)
    );

    public static float flatnessThreshold = 0.001f;
    public static float tIncrement = 0.005f; 

    static SplineTypes type;
    static float bSplineSmoothness = 1.0f;
    static float stickiness = 1.5f;
    static float tension = 0.75f;


    public static float GetFlatnessThresh()
    {
        return flatnessThreshold;
    }

    public static void SetFlatnessThresh(float f)
    {
        flatnessThreshold = f;
    }

    public static SplineTypes GetSplineType()
    {
        return type;
    }

    public static void SetSplineType(SplineTypes s)
    {
        type = s;
    }

    public static float GetBSplineSmoothness()
    {
        return bSplineSmoothness;
    }

    public static void SetBSplineSmoothness(float t) 
    {
        bSplineSmoothness = t;
    }
    public static float GetStickiness() 
    {
        return stickiness;
    }

    public static void SetStickiness(float s) 
    {
        stickiness = s;
    }

    public static float GetTension() 
    {
        return tension;
    }

    public static void SetTension(float t) 
    {
        tension = t;
    }

    public static Matrix4x4 GetBasisMatrix()
    {
        switch(type) 
        {
            case SplineTypes.CUBIC_BEZIER:      return cubicBezierBasis;
            case SplineTypes.HERMITE:           return hermiteBasis;
            case SplineTypes.CATMULL_ROM:       return catmullRomBasis;
            case SplineTypes.B_SPLINE:          return bSplineBasis;
            case SplineTypes.CARDINAL:          return GetCardinalBasis(); 
            default:                            return Matrix4x4.identity;
        }
    }

    public static void DrawSpline(Vector3[] controlPoints)
    {   
        if (type == SplineTypes.CUBIC_BEZIER) 
        {
            if (controlPoints is null || controlPoints.Length < 4) return;
            for (int i = 3; i < controlPoints.Length; i += 3)
            {
                RenderSplineSection (
                    new Vector3[] {
                        controlPoints[i - 3], 
                        controlPoints[i - 2], 
                        controlPoints[i - 1], 
                        controlPoints[i]
                    }
                );
            }
        } 
        else if (type == SplineTypes.CATMULL_ROM || type == SplineTypes.CARDINAL)
        {
            RenderCatmullRomSpline(controlPoints);
        } 
        else if (type == SplineTypes.HERMITE)
        {
            RenderHermiteSpline(controlPoints);
        }
        else if (type == SplineTypes.B_SPLINE)
        {
            RenderBSpline(controlPoints);
        }
        else 
        {
#if UNITY_EDITOR
            Handles.color = Color.green;
            for (int i = 1; i < controlPoints.Length; i++) {
                Handles.DrawLine(controlPoints[i - 1], controlPoints[i]);
            }
#endif
        }
    }

    static void RenderSplineSection(Vector3[] points)
    {
        if (points is null || points.Length != 4) return;

        if (FlatEnough(points))
        {
#if UNITY_EDITOR
            Handles.color = Color.green;
            Handles.DrawLine(points[0], points[3]);
#endif
        }
        else
        {
            (Vector3[] left, Vector3[] right) = SubdivideBezierCurve(points);
            if (left  != null) RenderSplineSection(left);
            if (right != null) RenderSplineSection(right);
        }
    }

    static void RenderCatmullRomSpline(Vector3[] points)
    {
        if (points is null || points.Length < 4) return;

        int l = points.Length;
        
        //create dummy start/end points to ensure
        //interpolation of all control points
        Vector3 s = points[0] - (points[1] - points[0]);
        Vector3 e = points[l - 1] + (points[l - 1] - points[l - 2]);

#if UNITY_EDITOR
        Handles.color = Color.yellow;
        
        Handles.SphereHandleCap(0, s, Quaternion.identity, 0.05f, EventType.Repaint);
        Handles.DrawLine(s, points[0]);
        
        Handles.SphereHandleCap(0, e, Quaternion.identity, 0.05f, EventType.Repaint);
        Handles.DrawLine(e, points[l - 1]);
        Handles.color = Color.green;
#endif

        Vector3[] p = new Vector3[] {
            s, points[0], points[1], points[2]
        };

        int segment = 1;
        float t_p = 0.0f;
        for (float t = 0.0f; t <= l - tIncrement; t += tIncrement)
        {
            if (segment < l - 2)
            {
                segment = (int) t + 1;
                p[0] = points[segment - 1];
                p[1] = points[segment];
                p[2] = points[segment + 1];
                p[3] = (segment == l - 2) ? e : points[segment + 2]; 
            }
            t_p = t % 1.0f;
            Vector3 p0 = GetPoint(t_p, p);
            Vector3 p1 = GetPoint(t_p + tIncrement, p);
#if UNITY_EDITOR
            Handles.DrawLine(p0, p1);
#endif
        }
#if UNITY_EDITOR
        Handles.color = Color.cyan;
        Handles.DrawLine(points[0], points[3]);
#endif
    }

    static void RenderHermiteSpline(Vector3[] points)
    {
        if (points is null || points.Length < 4) return;
        
        Vector3[] section = new Vector3[4];
        for (int i = 3; i < points.Length; i += 2)
        {
            section[0] = points[i - 3];
            section[1] = points[i - 3] + points[i - 2] / 3.0f;
            section[2] = points[i - 1] - points[i]     / 3.0f;
            section[3] = points[i - 1];

            RenderSplineSection(section);
        }
    }

    static void RenderBSpline(Vector3[] points)
    {
        if (points is null) return;

        float third     = (1.0f / 3.0f) * bSplineSmoothness;
        float twoThirds = (2.0f / 3.0f) * stickiness;

        Vector3 firstGap, secondGap, thirdGap;
        Vector3[] deBoor = new Vector3[4];

        for (int i = 3; i < points.Length; i++) 
        {
            firstGap  = points[i - 2] - points[i - 3];
            secondGap = points[i - 1] - points[i - 2];
            thirdGap  = points[i]     - points[i - 1];

            deBoor[0] = 0.5f * (points[i - 3] + (twoThirds * firstGap)   + points[i - 2] + (third * secondGap));
            deBoor[1] = points[i - 2] + (third * secondGap);
            deBoor[2] = points[i - 2] + (twoThirds * secondGap);
            deBoor[3] = 0.5f * (points[i - 2] + (twoThirds * secondGap)  + points[i - 1] + (third * thirdGap));
            
            RenderSplineSection(deBoor);
        }
    }

    static (Vector3[] left, Vector3[] right) SubdivideBezierCurve(Vector3[] points)
    {
        if (points is null || points.Length != 4 || FlatEnough(points)) return (null, null);

        Vector3 v0_p  = (points[0] + points[1]) / 2.0f;
        Vector3 v1_p  = (points[1] + points[2]) / 2.0f;
        Vector3 v2_p  = (points[2] + points[3]) / 2.0f;
        Vector3 v0_pp = (v0_p + v1_p) / 2.0f;
        Vector3 v1_pp = (v1_p + v2_p) / 2.0f;
        Vector3 r0 = (v0_pp + v1_pp) / 2.0f;

        return (
            new Vector3[] {points[0], v0_p, v0_pp, r0},
            new Vector3[] {r0, v1_pp, v2_p, points[3]}
        );
    }

    static Matrix4x4 GetCardinalBasis()
    {
        return new Matrix4x4 (
            new Vector4(0.0f, -tension,  2.0f * tension,        -tension),
            new Vector4(1.0f,  0.0f,     tension - 3.0f,         2.0f - tension),
            new Vector4(0.0f,  tension,  3.0f - 2.0f * tension,  tension - 2.0f),
            new Vector4(0.0f,  0.0f,    -tension,                tension)
        );
    }

    public static bool FlatEnough(Vector3[] section)
    {
        if (section is null || section.Length != 4) return false;

        float sum  =  (section[1] - section[0]).magnitude 
                    + (section[2] - section[1]).magnitude 
                    + (section[3] - section[2]).magnitude;
        float dist =  (section[3] - section[0]).magnitude;


        return  (dist < flatnessThreshold) || 
                ((sum / dist) < (1.0f + flatnessThreshold));
    }

    public static Vector3 GetBezierPoint(float t, Vector3[] controlPoints)
    {
        if (controlPoints is null || controlPoints.Length < 4 || type != SplineTypes.CUBIC_BEZIER) 
            return Vector3.zero;

        Vector3 v0_p  = (controlPoints[0] + controlPoints[1]) * t;
        Vector3 v1_p  = (controlPoints[1] + controlPoints[2]) * t;
        Vector3 v2_p  = (controlPoints[2] + controlPoints[3]) * t;
        Vector3 v0_pp = (v0_p + v1_p) * t;
        Vector3 v1_pp = (v1_p + v2_p) * t;
        Vector3 point = (v0_pp + v1_pp) * t;

        return point;
    }

    public static Vector3 GetPoint(float t, Vector3[] section) 
    {
        if (section is null || section.Length != 4) return Vector3.zero;
        Vector4 tM = new Vector4(1, t, t*t , t*t*t);
        return ComputeTMGMatrix(tM, section);
    }

    public static Vector3 GetTangent(float t, Vector3[] section)
    {        
        if (section is null || section.Length != 4) return Vector3.zero;
        if (type == SplineTypes.CATMULL_ROM)
        {
            Vector4 tM = new Vector4(1.0f, t, t*t, t*t*t);
            Vector3[] mg = MultiplyMatrix(catmullRomTangent, section);
            Vector3 tan = TVecMatMul(tM, mg);

            return tan;
        }
        else {
            float eps = 0.0001f;
            Vector3 e = (t == 1.0f) ? GetPoint(1.0f, section) : GetPoint(t + eps, section);
            Vector3 s = (t == 0.0f) ? GetPoint(0.0f, section) : GetPoint(t - eps, section);
            return (e - s).normalized;
        }
    }

    public static Vector3 GetNormal(float t, Vector3[] section)
    {        
        if (section is null || section.Length < 4) return Vector4.zero;
        
        if (type == SplineTypes.CATMULL_ROM)
        {
            Vector4 tM = new Vector4(1.0f, t, t*t, t*t*t);
            Vector3[] mg = MultiplyMatrix(catmullRomNormal, section);
            Vector3 tan = TVecMatMul(tM, mg);

            return tan;
        }
        else 
        {
            float eps = 0.0001f;
            Vector3 e = (t == 1.0f) ? GetTangent(1.0f, section) : GetTangent(t + eps, section);
            Vector3 s = (t == 0.0f) ? GetTangent(0.0f, section) : GetTangent(t - eps, section);
            return (e - s).normalized;
        }
        /**
        Vector3 tan = GetTangent(t, controlPoints);
        return NonZeroCrossProduct(tan).normalized;
        **/
    }

    public static Vector3 GetBinormal(float t, Vector3[] section) 
    {        
        if (section is null || section.Length < 4) return Vector4.zero;
        return Vector3.Cross(GetTangent(t, section), GetNormal(t, section)).normalized;
    }

    public static (Vector3 Tangent, Vector3 Normal, Vector3 Binormal) GetFrenetFrame(float t, Vector3[] controlPoints)
    {
        if (controlPoints is null || controlPoints.Length != 4) return (Vector3.zero, Vector3.zero, Vector3.zero);
        Vector3 tangent  = GetTangent(t, controlPoints);
        Vector3 normal   = GetNormal(t, controlPoints);
        Vector3 binormal = GetBinormal(t, controlPoints);
        return (tangent, normal, binormal);
    }

    static Vector3 ComputeTMGMatrix(Vector4 tMatrix, Vector3[] section)
    {
        if (section is null || section.Length != 4) return Vector3.zero;

        Matrix4x4 b = GetBasisMatrix();
        Vector3[] c = MultiplyMatrix(b, section); 

        return TVecMatMul(tMatrix, c);
    }

    static Vector3[] MultiplyMatrix(Matrix4x4 matrix, Vector3[] section)
    {
        if (section is null || section.Length != 4) return new Vector3[] {Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero};

        Vector3[] result =  new Vector3[] {
            new Vector3 (
                matrix[0, 0] * section[0].x + matrix[0, 1] * section[1].x + matrix[0, 2] * section[2].x + matrix[0, 3] * section[3].x,
                matrix[0, 0] * section[0].y + matrix[0, 1] * section[1].y + matrix[0, 2] * section[2].y + matrix[0, 3] * section[3].y,
                matrix[0, 0] * section[0].z + matrix[0, 1] * section[1].z + matrix[0, 2] * section[2].z + matrix[0, 3] * section[3].z
            ),
            new Vector3 (
                matrix[1, 0] * section[0].x + matrix[1, 1] * section[1].x + matrix[1, 2] * section[2].x + matrix[1, 3] * section[3].x,
                matrix[1, 0] * section[0].y + matrix[1, 1] * section[1].y + matrix[1, 2] * section[2].y + matrix[1, 3] * section[3].y,
                matrix[1, 0] * section[0].z + matrix[1, 1] * section[1].z + matrix[1, 2] * section[2].z + matrix[1, 3] * section[3].z
            ),
            new Vector3 (
                matrix[2, 0] * section[0].x + matrix[2, 1] * section[1].x + matrix[2, 2] * section[2].x + matrix[2, 3] * section[3].x,
                matrix[2, 0] * section[0].y + matrix[2, 1] * section[1].y + matrix[2, 2] * section[2].y + matrix[2, 3] * section[3].y,
                matrix[2, 0] * section[0].z + matrix[2, 1] * section[1].z + matrix[2, 2] * section[2].z + matrix[2, 3] * section[3].z
            ),
            new Vector3 (
                matrix[3, 0] * section[0].x + matrix[3, 1] * section[1].x + matrix[3, 2] * section[2].x + matrix[3, 3] * section[3].x,
                matrix[3, 0] * section[0].y + matrix[3, 1] * section[1].y + matrix[3, 2] * section[2].y + matrix[3, 3] * section[3].y,
                matrix[3, 0] * section[0].z + matrix[3, 1] * section[1].z + matrix[3, 2] * section[2].z + matrix[3, 3] * section[3].z
            )
        };

        return result;
    }

    static Vector3 TVecMatMul(Vector4 tMatrix, Vector3[] section)
    {
        if (section is null || section.Length != 4) return Vector3.zero;

        return new Vector3 (
            tMatrix.x * section[0].x + tMatrix.y * section[1].x + tMatrix.z * section[2].x + tMatrix.w * section[3].x,
            tMatrix.x * section[0].y + tMatrix.y * section[1].y + tMatrix.z * section[2].y + tMatrix.w * section[3].y,
            tMatrix.x * section[0].z + tMatrix.y * section[1].z + tMatrix.z * section[2].z + tMatrix.w * section[3].z
        );
    }

    public static float GetLength(Vector3[] controlPoints)
    {        
        if (controlPoints is null || controlPoints.Length < 4) return 0.0f;

        return 0.0f;
    }

    public static Vector3 NonZeroCrossProduct(Vector3 vec)
    {
        if (vec == Vector3.zero) return Vector3.zero;
        return Vector3.Cross(vec, new Vector3(vec.x + 0.01f, vec.y - 0.01f, vec.z + 0.01f));
    }
    
    /**
    public static Vector3 NonZeroCrossProduct(Vector3 vector) 
    {
        Vector3 cross = Vector3.Cross(vector, Vector3.up).normalized;
        if (cross.magnitude == 0.0f)
        {
            cross = Vector3.right;
        }

        return cross;
    }
    **/

    public static Matrix4x4 FrenetFrame(float t, Vector3[] controlPoints)
    {
        Vector3 position = GetPoint(t, controlPoints);
        (Vector3 tangent, Vector3 normal, Vector3 binormal) = GetFrenetFrame(t, controlPoints);
        return new Matrix4x4 (
            new Vector4(tangent.x,  tangent.y,  tangent.z,  0.0f),
            new Vector4(normal.x,   normal.y,   normal.z,   0.0f),
            new Vector4(binormal.x, binormal.y, binormal.z, 0.0f),
            new Vector4(position.x, position.y, position.z, 1.0f)
        );
    }
}
