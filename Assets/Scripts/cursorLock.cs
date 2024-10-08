using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cursorLock : MonoBehaviour
{
    public GameObject inventory, crosshair;

    // Update is called once per frame
    void Update()
    {
        if(inventory.activeInHierarchy == true) //Input.GetKeyDown(KeyCode.Tab))
        {
            crosshair.SetActive(false);
        }
        else if(inventory.activeInHierarchy == false)
        {
            crosshair.SetActive(true);
        }
    }
}
