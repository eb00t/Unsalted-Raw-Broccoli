using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dialogueAppear : MonoBehaviour
{
    public GameObject dialogueBox, playerChar, npcChar, enemyChar, indicator; // yesText, noText;
   // public bool playerNear = false;

    // Update is called once per frame
    void Update()
    {
        //prompt text appear
        if (Vector3.Distance(playerChar.transform.position, enemyChar.transform.position) <= 5f)
        {
            indicator.SetActive(true);
          //  Debug.Log("Box on!");
        }
        else if(Vector3.Distance(playerChar.transform.position, enemyChar.transform.position) <= 5f)
        {
            indicator.SetActive(false);
          //  Debug.Log("Box off!");
        }

        //Text appear
        if (Input.GetKeyDown(KeyCode.F))
        {
            dialogueBox.SetActive(true);
            indicator.SetActive(false);
        }

        /*
        //Activate Prompt
        if(playerNear)
        {
            indicator.SetActive(true);
        }
        else
        {
            indicator.SetActive(false);
        }
        */
    }
  
    /*
    private void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.tag == "Enemy")
        {
            playerNear = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.tag == "Enemy")
        {
            playerNear = false;
        }
    }
    */
}
