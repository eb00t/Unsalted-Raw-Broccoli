using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }
    public bool lookAtWholeMap;
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one CameraController script in the scene.");
        }

        Instance = this;
        
    }
}
