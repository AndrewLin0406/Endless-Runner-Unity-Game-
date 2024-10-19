using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyFirstScript : MonoBehaviour
{
    //Awake is also called before first frame, before start
    //init code

    void Awawke()
    {
        Debug.Log("Awake");
    }

    // Start is called before the first frame update, after awake
    void Start()
    {
        Debug.Log("Start");
    }

    //Onenable is called when the GmaeObject is active on the scene
    void OnEnable()
    {
        Debug.Log("OnEnable");
    }

    //FixedUpdate is called once per fixed time frame - physics
    void FixedUpdate()
    {
        Debug.Log("FixedUpdate");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Update");
    }

    //lateupdate is called once per frame - after everything completes/calculates - Cameras
    void LateUpdate()
    {
        Debug.Log("LateUpdate");
    }


}
