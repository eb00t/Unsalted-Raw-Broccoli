using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverHandler : MonoBehaviour
{
    private RoomInfo _roomInfo;
    private Rigidbody _rigidbody;
    private Renderer _renderer;
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        _rigidbody.isKinematic = true;
       _roomInfo = transform.root.GetComponent<RoomInfo>();
       transform.localScale = new Vector3(_roomInfo.roomLength, _roomInfo.roomHeight, 0.25f);
    }

    void Update()
    {
        if (_roomInfo.coveredUp == false)
        {
           _rigidbody.isKinematic = false;
           //_rigidbody.AddForce(new Vector3(0, 0, -1f), ForceMode.Impulse);
           if (_renderer.isVisible == false)
           {
              
              gameObject.SetActive(false);
           }
        }
    }
}
