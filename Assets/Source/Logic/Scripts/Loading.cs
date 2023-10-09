using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.ComponentModel;

public class Loading : MonoBehaviour
{
    public static Loading instance;
    private float progress = 0;
    private int maxProgress = 1;

    public float currentProgress => (float)progress / (float)maxProgress * 100;

    private void Awake()
    {
        instance = this;
        AddCounter();
    }

    public void AddMaxProgress(int value)
    {
        maxProgress += value;
    }

    public void AddCounter()
    {
        progress += 1;
    }
}
