using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.ComponentModel;

public class Loading : MonoBehaviour
{
    public float progress;
    public int maxProgress = 1;

    public float currentProgress => (float)progress / (float)maxProgress * 100;

    public bool isLoading = false;

    private void Start()
    {
        
    }

    void Update()
    {
        if (isLoading)
        {
            progress += Time.deltaTime;

            if (progress > maxProgress)
            {
                progress = 0.9f;
            }
        }

        Debug.Log(currentProgress);
    }
    void StartLoading()
    {
        isLoading = true;
    }

    void StopLoading()
    {
        progress = 1;
        isLoading = false;
    }
    void OnEnable()
    {
        SchematicGenerator.onStartLoading += StartLoading;
        SchematicGenerator.onEndLoading += StopLoading;
    }

    void OnDisable()
    {
        SchematicGenerator.onEndLoading -= StopLoading;
        SchematicGenerator.onStartLoading -= StartLoading;
    }
}
