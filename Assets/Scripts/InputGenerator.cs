using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputGenerator : MonoBehaviour
{
    public Text displayVal;
    public Text displayVal2;

    public int val;
    public int val2;

    int prevVal = 0;
    int prevVal2 = 0;

    void FixedUpdate()
    {
        val = prevVal + Random.Range(-2, 3);
        val2 = prevVal2 + Random.Range(-2, 3);

        displayVal.text = "val = " + val.ToString();
        displayVal2.text = "val 2 = " + val2.ToString();

        prevVal = val;
        prevVal2 = val2;
    }
}
