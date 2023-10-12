using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateVehicles : MonoBehaviour
{
    public int seed = 70;
    public GameObject parent;
    

    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(seed);

        GameObject go = new GameObject("Patch");
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<Renderer>().material.color = Color.white;

        BezierCurve one = new BezierCurve (
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0.3f),
            new Vector3(0.5f, 2.0f, 0.75f),
            new Vector3(1.0f, 2.5f, 1.0f)
        );
        BezierCurve two = new BezierCurve (
            new Vector3(1, 0, 0),
            new Vector3(1.3f, 0.1f, 0.3f),
            new Vector3(1.5f, 0.5f, 0.75f),
            new Vector3(1.9f, 2.5f, 1.0f)
        );
        BezierCurve three = new BezierCurve (
            new Vector3(2, 0, 0),
            new Vector3(2.2f, 1, 0.3f),
            new Vector3(2.5f, 1.7f, 0.75f),
            new Vector3(2.4f, 1.9f, 1.0f)
        );
        BezierCurve four = new BezierCurve (
            new Vector3(3, 0.1f, 0),
            new Vector3(3.2f, 0.4f, 0.3f),
            new Vector3(3.3f, 0.6f, 0.75f),
            new Vector3(3.2f, 1.3f, 1.0f)
        );

        go.GetComponent<MeshFilter>().mesh = VehicleUtils.BezierPatch(one, two, three, four, 3);
        //go.GetComponent<MeshFilter>().mesh = VehicleUtils.CreateCylinder(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f), 5.0f, 8);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
