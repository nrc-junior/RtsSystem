using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class priv : MonoBehaviour
{
    Other wtfisthis = new Other();

    void Start()
    {
        var _barVariable = (string) typeof(Other).GetField("wtf", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wtfisthis);    
        Debug.Log(_barVariable);
    }

    
}

public class Other{
    private string wtf = "wttttf?";
}
