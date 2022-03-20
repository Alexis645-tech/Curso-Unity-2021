using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public static ColorManager SharedInstance;
    public Color selectedColor;
    public Color defaultColor = Color.black;

    private void Awake()
    {
        SharedInstance = this;
    }
    
    public Color barColor(float finalScale)
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

    public class StatusConditionColor
    {
        private static Dictionary<StatusConditionID, Color> colors = new Dictionary<StatusConditionID, Color>()
        {
            {StatusConditionID.none, Color.white},
            {StatusConditionID.brn, new Color(223f / 255, 134f / 255, 67f / 255)},
            {StatusConditionID.frz, new Color(168f / 255, 214f / 255, 215f / 255)},
            {StatusConditionID.par, new Color(241f / 255, 208f / 255, 83f / 255)},
            {StatusConditionID.psn, new Color(147f / 255, 73f / 255, 156f / 255)},
            {StatusConditionID.slp, new Color(163f / 255, 147f / 255, 234f / 255)}
        };

        public static Color GetColorFromStatusCondition(StatusConditionID id)
        {
            return colors[id];
        }
    }
}
