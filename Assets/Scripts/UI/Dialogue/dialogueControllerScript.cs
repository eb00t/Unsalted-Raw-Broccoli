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
    public GameObject dialogueCanvas, yesText, noText; // normalText, normalBox;

    //Bools
    public bool canText = false;
   // public bool YesOrNo = false; //true = yes, false = no

    // Start is called before the first frame update
    void Start()
    {
        NextSentence();
       // Debug.Log("Sentence started");
    }

    // Update is called once per frame
    void Update()
    {
        //Move to next sentence
        if(Input.GetKeyDown(KeyCode.Tab) && dialogueCanvas.activeInHierarchy == true && canText == true)
        {
            NextSentence();
            DialogueSpeed = 0.05f;
           // Debug.Log("Next sentence started");
        }

        /*
        //Skip Dialogue
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SkipDialogue();
        }
        */

        dialogueFinished();

        //Speed up dialogue
        if (Input.GetKeyDown(KeyCode.LeftControl) && dialogueCanvas.activeInHierarchy == true)
        {
            speedUptext();
        }

        //Qusetion answers
        if(Input.GetKeyDown(KeyCode.Y)) //YesOrNo == true &&
        {
            //Answer is yes
            answerY();
           // normalText.SetActive(false);
            //normalBox.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.N)) //YesOrNo == false && 
        {
            //Answer is no
            answerN();
          //  normalText.SetActive(false);
        }
        else if(Input.GetKeyDown(KeyCode.Escape))
        {
            noText.SetActive(false);
            yesText.SetActive(false);
        }

        //dialogueFinished();
    }

    void NextSentence()
    {
        if(Index <= Sentences.Length - 1)
        {
            DialogueText.text = "";
            StartCoroutine(WriteSentence());

            canText = false;
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

    void dialogueFinished()
    {
        if (Index == Sentences.Length )
        {
            dialogueCanvas.SetActive(false);
            Debug.Log("Text Done!");
        }
    }

    void answerY()
    {
        yesText.SetActive(true);
        noText.SetActive(false);
        Debug.Log("YES!");
    }

    void answerN()
    {
        noText.SetActive(true);
        yesText.SetActive(false);
        Debug.Log("NO");
    }

    IEnumerator WriteSentence()
    {
        foreach(char Character in Sentences[Index].ToCharArray())
        {
            DialogueText.text += Character;
            yield return new WaitForSeconds(DialogueSpeed);
        }

        Index++;
        canText = true;
    }
}
