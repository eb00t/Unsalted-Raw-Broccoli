using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class onNow : MonoBehaviour
{
    public GameObject DEAD;

    // Update is called once per frame
    void Update()
    {
       if(Input.GetKeyDown(KeyCode.P))
        {
            DEAD.SetActive(true);
        }
       /*
       else if(Input.GetKeyDown(KeyCode.L))
        {
            DEAD.SetActive(false);
        }
       */
    }
}
