using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Algorithms;
public class TestScript : MonoBehaviour
{
    public int intToTest = int.MaxValue;
    public int shift = 0;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log(System.Convert.ToString(intToTest, 2));
            intToTest = (intToTest << shift);
            Debug.Log(System.Convert.ToString(intToTest, 2));
        }
        
    }
}
