using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Algorithms;
public class TestScript : MonoBehaviour
{
    public int intToTest = int.MaxValue;
    // Update is called once per frame
    void Update()
    {
        Debug.Log(GetHashCode());
    }
}
