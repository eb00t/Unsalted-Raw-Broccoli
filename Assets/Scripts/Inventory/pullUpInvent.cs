using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pullUpInvent : MonoBehaviour
{
    private GameObject inventory;
    private bool inventTab;
    void Start()
    {
        inventory = GameObject.FindWithTag("inventory");
        inventory.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            inventTab = !inventTab;
            if (inventTab)
            {
                Cursor.lockState = CursorLockMode.None;
                inventory.SetActive(true);
            }
            else
            {
                inventory.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}
