using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelegateExample : MonoBehaviour
{
    //to create delegate

    //delegate returntype delegatename (parameters)
    public delegate float DoMath(float a, float b);

    public float Add(float a, float b)
    {
        return a + b;
    }

    public float subtract(float a, float b)
    {
        return a - b;
    }


    // Start is called before the first frame update
    void Start()
    {
        DoMath doMathVariable = new DoMath(Add);
        DoMath doMathVariable2 = new DoMath(subtract);

        Debug.Log(doMathVariable(10, 8));

        doMathVariable = subtract;

        Debug.Log(doMathVariable(10, 8));

        doMathVariable = Add;

        Debug.Log(DoConfusingMath(doMathVariable, doMathVariable2));

        //store lamba expression function into a delegate 
        doMathVariable = (a, b) => a * b;
    }

    public float DoConfusingMath(DoMath math1, DoMath math2)
    {
        return math1(10, 8) + math2(10, 8);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    
}
