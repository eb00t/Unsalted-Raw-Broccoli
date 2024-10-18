using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class dialogueControllerScript : MonoBehaviour
{
    public TextMeshProUGUI DialogueText;
    public string[] Sentences;
    private int Index = 0;
    public float DialogueSpeed;
    public float fasterSpeed;
    public GameObject dialogueCanvas;

    // Start is called before the first frame update
    void Start()
    {
        NextSentence();
    }

    // Update is called once per frame
    void Update()
    {
        //Move to next sentence
        if(Input.GetKeyDown(KeyCode.Space))
        {
            NextSentence();
            DialogueSpeed = 0.05f;
        }

        /*
        //Skip Dialogue
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SkipDialogue();
        }
        */

        //Speed up dialogue
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            speedUptext();
        }
        
    }

    void NextSentence()
    {
        if(Index <= Sentences.Length - 1)
        {
            DialogueText.text = "";
            StartCoroutine(WriteSentence());
        }
    }

    void speedUptext()
    {
        DialogueSpeed = fasterSpeed;
    }

    /*
    void SkipDialogue()
    {
        //Turns off canvas
        dialogueCanvas.SetActive(false);

       //Debug.Log("Text Skipped!");
    }
    */

    IEnumerator WriteSentence()
    {
        foreach(char Character in Sentences[Index].ToCharArray())
        {
            DialogueText.text += Character;
            yield return new WaitForSeconds(DialogueSpeed);
        }

        Index++;
    }
}
