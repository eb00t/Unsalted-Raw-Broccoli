using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dialogueAppear : MonoBehaviour
{
    public GameObject dialogueBox, dialogueConA, dialogueConB, playerChar, enemyChar, indicator; // yesText, noText, npcChar;
    public GameObject npcChar; // bossIndicator;

    //public bool enemyNear = false;
    //public bool npcNear = false;

    private void Start()
    {
       // npcChar = GameObject.Find("NPC");
    }

    // Update is called once per frame
    void Update()
    {
        npcChar = GameObject.Find("NPC");
       // bossIndicator = GameObject.Find("bossIndicator");

        //prompt text appear (ENEMY)
        if (Vector3.Distance(playerChar.transform.position, enemyChar.transform.position) <= 5f)
        {
            indicator.SetActive(true);
            dialogueConA.SetActive(true);
            dialogueConB.SetActive(false);

            //  enemyNear = true;
            // npcNear = false;
            //Debug.Log("ENEMY NEAR!");
        }
        else if(Vector3.Distance(playerChar.transform.position, enemyChar.transform.position) >= 5f)
        {
            indicator.SetActive(false);
            dialogueConA.SetActive(false);
            dialogueConB.SetActive(true);

            // enemyNear = false;
            // npcNear = true;
            //  Debug.Log("ENEMY GONE!");
        }
        
        //NPC Indicator
       else if (Vector3.Distance(playerChar.transform.position, npcChar.transform.position) <= 5f)
        {
           // bossIndicator.SetActive(true);
            dialogueConA.SetActive(true);
            dialogueConB.SetActive(false);

            //npcNear = true;
          //  Debug.Log("NPC NEAR!");
        }
        else if (Vector3.Distance(playerChar.transform.position, npcChar.transform.position) >= 5f)
        {
          //  bossIndicator.SetActive(false);
            dialogueConA.SetActive(false);
            dialogueConB.SetActive(true);

            // npcNear = false;
           // Debug.Log("NPC GONE!");
        }

        //Text appear
        if (Input.GetKeyDown(KeyCode.M))
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
