using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubSurfaceElementsUtil
{
    static List<string> surfaceKkeys = new List<string>
    {
        "coiled_tubing",
        "generic_element", 
        "column_casing", 
        "draw_bomb", 
        "production_packer",
        "downhole_safety_valve",
        "drill_string"
    };

    public static bool IsFromSubSurface(string key)
    {
        return surfaceKkeys.Contains(key);
    }
}
