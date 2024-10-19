using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helpers
{
    //odift the x value of a vector3
    public static void SetX(Transform t, float x)
    {
        t.position = new Vector3(x, t.position.y, t.position.z);
    }
    public static void SetY(Transform t, float y)
    {
        t.position = new Vector3(t.position.x, y, t.position.z);
    }
    public static void SetZ(Transform t, float z)
    {
        t.position = new Vector3(t.position.x, t.position.y, z);
    }

 
}

