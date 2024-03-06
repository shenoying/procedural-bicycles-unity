using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWheel : MonoBehaviour
{

    public float speed = 50.0f;
    public Vector3 axis = new Vector3(0.0f, 0.0f, 1.0f);
    public Vector3 eulerAngles;

    public void Start()
    {

    }

    public void Update()
    {
        eulerAngles += axis * Time.deltaTime * speed;
        transform.localEulerAngles = eulerAngles;
    }

}