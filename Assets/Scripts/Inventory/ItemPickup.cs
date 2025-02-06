using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{

    [SerializeField] private GameObject popUpGUI;
    private Animator _animator;

    private void Start()
    {
        _animator = popUpGUI.GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name + " : " + other.gameObject.name);
        if (other.CompareTag("Player"))
        {
            _animator.SetTrigger("SlideIn");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _animator.SetTrigger("SlideOut");  
        }
    }
}
