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

        for (int i = 0; i < 10; i++)
        {
            float w = Random.Range(3.5f, 5.0f);
            CreateBicycle(
                new Vector3(-15.0f + 3.0f * i, 0.0f, 2.0f * i - 10.0f),
                w,
                w * 0.7f
            );
        }
    }

    void CreateBicycle(Vector3 center, float width, float height)
    {
        GameObject bike = new GameObject("Bicycle");

        Vector3 wheelOnePos = center - new Vector3(width / 2.0f, 0.0f, 0.0f);
        Vector3 wheelTwoPos = center + new Vector3(width / 2.0f, 0.0f, 0.0f);
        Vector3 handlebarPos = wheelOnePos + new Vector3(0.2f * width, height, 0.0f);
        Vector3 seatPos = wheelTwoPos + new Vector3(-0.15f * width, 0.85f * height, 0.0f);

        GameObject body = VehicleUtils.Body(center, width, height);
        float wheelScale = Random.Range(0.85f, 1.15f);


        GameObject wheelOneParent = new GameObject("WheelOneParent");
        wheelOneParent.transform.position = wheelOnePos;
        GameObject wheelOne = VehicleUtils.CreateWheel(wheelOnePos, 0.3f * width * wheelScale, 0.2f, 8);
        wheelOne.transform.parent = wheelOneParent.transform;
        wheelOneParent.AddComponent<MoveWheel>();
        
        GameObject wheelTwoParent = new GameObject("WheelTwoParent");
        wheelTwoParent.transform.position = wheelTwoPos;
        GameObject wheelTwo = VehicleUtils.CreateWheel(wheelTwoPos, 0.3f * width * wheelScale, 0.2f, 8);
        wheelTwo.transform.parent = wheelTwoParent.transform;
        wheelTwoParent.AddComponent<MoveWheel>();
        
        GameObject handlebars = VehicleUtils.Handlebars(handlebarPos, Random.Range(0.10f, 0.25f), 0.3f * width, 8);
        GameObject seat = VehicleUtils.Seat(seatPos + 0.15f * (seatPos - center), 3);
        ///TODO: fix pedal issue
        //GameObject pedalsOne = VehicleUtils.CreatePedals(center + width * new Vector3(0.0f, 0.0f, 0.08f), 0.25f, 0.25f, 8);
        //GameObject pedalsTwo = VehicleUtils.CreatePedals(center - width * new Vector3(0.0f, 0.0f, 0.08f), 0.25f, 0.25f, 8);
        GameObject pedalsParent = new GameObject("PedalsParent");
        pedalsParent.transform.position = center;
        GameObject pedals = VehicleUtils.CreatePedals(center, 0.25f, Random.Range(0.15f, 0.35f), 8);
        pedals.transform.parent = pedalsParent.transform;
        pedalsParent.AddComponent<MoveWheel>();

        body.transform.parent = bike.transform;
        wheelOneParent.transform.parent = bike.transform;
        wheelTwoParent.transform.parent = bike.transform;
        handlebars.transform.parent = bike.transform;
        seat.transform.parent = bike.transform;
        pedalsParent.transform.parent = bike.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
