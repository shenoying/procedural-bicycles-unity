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

}