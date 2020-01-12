using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Has an internal hashfunction
public interface IHashFunction
{
    int Hash { get; }
    int HashFunction();
    /// <summary>
    /// Returns -1 for less priority, 0 for equal priority and 1 for higher priority.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    int CompareTo(IHashFunction value);
}