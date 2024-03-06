using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatmullRomCurve
{

    List<Vector3> controlPoints;
    public List<Vector3> ControlPoints { get => controlPoints; set => controlPoints = value; }

    public CatmullRomCurve()
    {
        controlPoints = new List<Vector3>();
    }

    public CatmullRomCurve(List<Vector3> controlPoints)
    {
        this.controlPoints = controlPoints;
    }

    public Vector3 GetPoint(float t)
    {
        if (t < 0.0f || t > controlPoints.Count - 3.0f) return Vector3.zero;
        SplineUtils.SetSplineType(SplineUtils.SplineTypes.CATMULL_ROM);
        Vector3[] p = GetSection(t);
        return SplineUtils.GetPoint(t % 1.0f, p);
    }

    public Vector3 GetTangent(float t)
    {
        if (t < 0.0f || t > controlPoints.Count - 3.0f) return Vector3.zero;
        SplineUtils.SetSplineType(SplineUtils.SplineTypes.CATMULL_ROM);
        Vector3[] p = GetSection(t);
        return SplineUtils.GetTangent(t % 1.0f, p).normalized;
    }

    public Vector3 GetNormal(float t)
    {
        if (t < 0.0f || t > controlPoints.Count - 3.0f) return Vector3.zero;
        SplineUtils.SetSplineType(SplineUtils.SplineTypes.CATMULL_ROM);
        Vector3[] p = GetSection(t);
        return SplineUtils.GetNormal(t % 1.0f, p).normalized;
    }

    public Vector3 GetBinormal(float t)
    {
        if (t < 0.0f || t > controlPoints.Count - 3.0f) return Vector3.zero;
        SplineUtils.SetSplineType(SplineUtils.SplineTypes.CATMULL_ROM);
        Vector3[] p = GetSection(t);
        return SplineUtils.GetBinormal(t % 1.0f, p).normalized;
    }

    Vector3[] GetSection(float t)
    {
        Vector3[] section = new Vector3[4];

        int segment = (int) t;
        int l = controlPoints.Count;
        
        if (t >= 0.0f && t < 1.0f)
        {
            section[0] = controlPoints[0] - (controlPoints[1] - controlPoints[0]);
            section[1] = controlPoints[1];
            section[2] = controlPoints[2];
            section[3] = controlPoints[3];
        }
        else 
        {
            section[0] = controlPoints[segment - 1];
            section[1] = controlPoints[segment];
            section[2] = controlPoints[segment + 1];
            section[3] = (segment == l - 2) ? controlPoints[l - 1] + (controlPoints[l - 1] - controlPoints[l - 2]) : controlPoints[segment + 2];    
        }

        return section;
    }



}