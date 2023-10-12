using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve
{

    Vector3 pOne, pTwo, pThree, pFour;
    public Vector3 POne { get => pOne; set => pOne = value; }
    
    public Vector3 PTwo { get => pTwo; set => pTwo = value; }
    
    public Vector3 PThree { get => pThree; set => pThree = value; }
    
    public Vector3 PFour { get => pFour; set => pFour = value; }


    public BezierCurve(Vector3 pOne, Vector3 pTwo, Vector3 pThree, Vector3 pFour)
    {
        this.pOne = pOne;
        this.pTwo = pTwo;
        this.pThree = pThree;
        this.pFour = pFour;
    }

    public Vector3 GetPoint(float t)
    {
        Vector3[] section = new Vector3[] {pOne, pTwo, pThree, pFour};

        return SplineUtils.GetPoint(t, section);
    }

    public Vector3 GetTangent(float t)
    {
        Vector3[] section = new Vector3[] {pOne, pTwo, pThree, pFour};

        return SplineUtils.GetTangent(t, section);    
    }

}