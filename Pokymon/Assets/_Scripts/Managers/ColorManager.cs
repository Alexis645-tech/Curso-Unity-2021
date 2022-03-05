using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public static ColorManager SharedInstance;
    public Color selectedColor;

    private void Awake()
    {
        SharedInstance = this;
    }
    
    public Color barColor(float finalScale)
    {
        if (finalScale < 0.15f)
        {
            return Color.red;
        }
        else if (finalScale < 0.5f) 
        {
            return Color.yellow; 
        }
        else
        {
            return Color.green;
        }
    }
    
    public Color PpColor(float finalScale)
    {
        if (finalScale < 0.2f)
        {
            return Color.red;
        }
        else if (finalScale < 0.5f) 
        {
            return Color.yellow; 
        }
        else
        {
            return Color.black;
        }
    }
}
