using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{

    public float yDistance;
    public float zDistance;
    public GameObject target;

    void Start()
    {
        target = GameObject.Find("Sphere");
    }

    void Update()
    {
        
    }

    void LateUpdate()
    {
        if (Input.GetKey(KeyCode.E))
        {
            //rotate the camera by 180 degrees over time
            transform.Rotate(Vector3.up, 189f * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(Vector3.down, 180 * Time.deltaTime);
        }
        //place the camera at the same position as the target
        transform.position = target.transform.position + transform.forward * zDistance;
        //translate
        transform.Translate(Vector3.forward * zDistance);
        transform.Translate(Vector3.up * yDistance);
    }
}
