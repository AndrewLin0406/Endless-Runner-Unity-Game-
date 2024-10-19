using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPhysics : MonoBehaviour
{

    //integers float boolean double struct char - value type
    //strings objecets classes interfaces delegates - reference type

    //value types directly contain their daata
    //refernce type gives which type of data it is

    public float speed = 20;
    private Rigidbody rigidbody;
    Transform tCamera; //get a reference to the transform componenet on the camera

    // Start is called before the first frame update
    void Start()
    {
        //GetComponenet is a gerneric type parameter 
        rigidbody = GetComponent<Rigidbody>();
        tCamera = GameObject.Find("Main Camera").transform;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        //Create local variables to determine the forward and right direction of main camera
        Vector3 forward = tCamera.forward;
        Vector3 right = tCamera.right;

        if (Input.GetKey(KeyCode.W))
        {
            rigidbody.AddForce(forward * speed);
        }
        if (Input.GetKey(KeyCode.A))
        {
            rigidbody.AddForce(-right * speed);
        }
        if (Input.GetKey(KeyCode.S))
        {
            rigidbody.AddForce(-forward * speed);
        }
        if (Input.GetKey(KeyCode.D))
        {
            rigidbody.AddForce(right * speed);
        }

    }
}
