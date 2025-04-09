using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorHide : MonoBehaviour
{
    private static readonly int Vanish = Animator.StringToHash("vanish");
    private Animator _animator;
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (BlackoutManager.Instance.blackoutComplete)
        {
            _animator.SetBool(Vanish, true);
        }
    }

    public void HideDoor()
    {
        gameObject.SetActive(false);
    }
}
