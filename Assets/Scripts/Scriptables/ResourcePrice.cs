using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ResourcePrice{
    public int red;
    public int green;
    public int blue;

    public ResourcePrice(int r, int g, int b){
        red = r;
        green = g;
        blue = b; 
    }
}
