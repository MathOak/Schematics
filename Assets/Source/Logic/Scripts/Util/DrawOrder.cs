using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawOrder : MonoBehaviour
{
    static List<string> surfaceKkeys = new List<string> { "surface_tree", "safe_t", "stuffing_box", "bop_haste", "adapter", "tubing_hanger", "wellhead" };

    public static bool IsFromSurface (string key)
    {
        return surfaceKkeys.Contains(key);
    }

    public static int GetGreater(string keyA, string keyB)
    {
        int keyAIndex = surfaceKkeys.IndexOf(keyA);
        int keyBIndex = surfaceKkeys.IndexOf(keyB);

        if (keyAIndex == -1)
            keyAIndex = surfaceKkeys.Count + 1;

        if (keyBIndex == -1)
            keyBIndex = surfaceKkeys.Count + 1;

        if (keyAIndex == -1 && keyBIndex == -1)
            return 0;

        if (keyAIndex > keyBIndex)
            return 1;

        if (keyAIndex < keyBIndex)
            return -1;

        return 0;
    }
}
