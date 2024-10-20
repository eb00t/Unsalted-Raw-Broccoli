using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dialogueAppear : MonoBehaviour
{
    public GameObject dialogueBox, playerChar, npcChar, enemyChar;

    // Update is called once per frame
    void Update()
    {
        //enemy text appear
        if (Vector3.Distance(playerChar.transform.position, enemyChar.transform.position) <= 5f)
        {
            dialogueBox.SetActive(true);
            //Debug.Log("Box on!");
        }
        else
        {
            dialogueBox.SetActive(false);
            //Debug.Log("Box off!");
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
