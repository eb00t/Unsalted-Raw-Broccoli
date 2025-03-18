using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dialogueAppear : MonoBehaviour
{
    public GameObject dialogueBox, playerChar, npcChar, enemyChar, indicator; // yesText, noText;
   // public bool isOn = false;

    // Update is called once per frame
    void Update()
    {
        //prompt text appear
        if (Vector3.Distance(playerChar.transform.position, enemyChar.transform.position) <= 5f)
        {
           // isOn = true;
          //  Debug.Log("TRUE");
            indicator.SetActive(true);
            //Debug.Log("Box on!");
        }
        else if(Vector3.Distance(playerChar.transform.position, enemyChar.transform.position) >= 5f)
        {
          //  isOn = false;
           // Debug.Log("FALSE");
              indicator.SetActive(false);
            //Debug.Log("Box off!");
        }

        //Text appear
        if(Input.GetKeyDown(KeyCode.M))
        {
            dialogueBox.SetActive(true);
            indicator.SetActive(false);
        }

        /*
        //NPC text appear
        if (Vector3.Distance(playerChar.transform.position, npcChar.transform.position) <= 1f)
        {
            dialogueBox.SetActive(true);
        }
        else
        {
            dialogueBox.SetActive(false);
        }
        */
    }
}
