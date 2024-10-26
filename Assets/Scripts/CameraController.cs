using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public bool lookAtWholeMap;
    private CinemachineTargetGroup _targetGroup;
    // Start is called before the first frame update
    void Start()
    {
        _targetGroup = gameObject.GetComponent<CinemachineTargetGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
