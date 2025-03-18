using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dialogueAppear : MonoBehaviour
{
    public GameObject dialogueBoxA, dialogueBoxB, playerChar, enemyChar, npcChar, indicator; // yesText, noText;
    //public bool enemyNear = false;
    //public bool npcNear = false;

    // Update is called once per frame
    void Update()
    {
        //prompt text appear (ENEMY)
        if (Vector3.Distance(playerChar.transform.position, enemyChar.transform.position) <= 5f)
        {
            indicator.SetActive(true);
          //  enemyNear = true;
           // npcNear = false;
            //Debug.Log("ENEMY NEAR!");
        }
        else if(Vector3.Distance(playerChar.transform.position, enemyChar.transform.position) >= 5f)
        {
            indicator.SetActive(false);
           // enemyNear = false;
           // npcNear = true;
            //  Debug.Log("ENEMY GONE!");
        }
        /*
        else if (Vector3.Distance(playerChar.transform.position, npcChar.transform.position) <= 5f)
        {
            indicator.SetActive(true);
            npcNear = true;
         //   Debug.Log("NPC NEAR!");
        }
        else if (Vector3.Distance(playerChar.transform.position, npcChar.transform.position) >= 5f)
        {
            indicator.SetActive(false);
            npcNear = false;
          //  Debug.Log("NPC GONE!");
        }
        */

        //Text appear
        if (Input.GetKeyDown(KeyCode.M))
        {
            dialogueBoxA.SetActive(true);
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
