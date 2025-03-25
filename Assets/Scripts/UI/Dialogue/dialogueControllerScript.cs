using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class dialogueControllerScript : MonoBehaviour
{
    [SerializeField] private DialogueObjectHandler dialogueObjectHandler;
    //DIALOGUE CODE
    public TextMeshProUGUI DialogueText;
    public TextMeshProUGUI speakerText;
    [TextArea(3, 10)]
    public string[] Sentences;
    private int Index; // = 0;
    public float DialogueSpeed;
    //public float fasterSpeed;
    public GameObject dialogueCanvas, yesText, noText; // normalText; // normalBox;


    private void Awake()
    {
        if (dialogueObjectHandler == null)
        {
            Debug.LogError(gameObject.name + " is missing a dialogueObjectHandler");
        }
    }

    private void Start()
    {
        speakerText = DialogueText.transform.parent.Find("SpeakerText").GetComponent<TextMeshProUGUI>();
        if (dialogueObjectHandler.isAnyoneSpeaking == false)
        {
            speakerText.gameObject.SetActive(false);
        }
        //Start writing sentences
        DialogueText.text = string.Empty;
        speakerText.text = dialogueObjectHandler.whoIsSpeaking[0];
        startSentence();
    }

    private void Update()
    {
        /*
        //ANSWERS
        if (Input.GetKeyDown(KeyCode.Y))
        {
            //Answer is yes
            answerY();
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            //Answer is no
            answerN();
        }
       
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Turn off text boxes
            noText.SetActive(false);
            yesText.SetActive(false);
        }
        */
    }

    // move through sentences
    public void NextSentence(InputAction.CallbackContext context) // when space/a is pressed
    {
        if (!context.performed) return;
        if (!gameObject.activeSelf) return;
        
        if (DialogueText.text == dialogueObjectHandler.dialogueBodyText[Index])
        {
            // nextSen() moved here
            if(Index < dialogueObjectHandler.dialogueBodyText.Length - 1)
            {
                Index++;
                DialogueText.text = string.Empty;
                if (dialogueObjectHandler.whoIsSpeaking[Index] == null)
                {
                    dialogueObjectHandler.whoIsSpeaking[Index] = dialogueObjectHandler.whoIsSpeaking[Index - 1];
                }
                speakerText.text = dialogueObjectHandler.whoIsSpeaking[Index];
                StartCoroutine(typeSentence());
            }
            else
            {
                dialogueCanvas.SetActive(false);
                Index = 0;
            }
        }
        else
        {
            StopAllCoroutines();
            DialogueText.text = dialogueObjectHandler.dialogueBodyText[Index];
        }
    }

    void startSentence()
    {
        Index = 0;
        StartCoroutine(typeSentence());
    }

    IEnumerator typeSentence()
    {
        foreach (char Character in dialogueObjectHandler.dialogueBodyText[Index].ToCharArray())
        {
            DialogueText.text += Character;
            yield return new WaitForSeconds(DialogueSpeed);
        }
    }
    
    void answerY()
    {
        yesText.SetActive(true);
        noText.SetActive(false);
        dialogueCanvas.SetActive(false);
      //  Debug.Log("YES!");
    }

    void answerN()
    {
        noText.SetActive(true);
        yesText.SetActive(false);
        dialogueCanvas.SetActive(false);
    //    Debug.Log("NO");
    }

}

















/*
 * 
public TextMeshProUGUI DialogueText;
public string[] Sentences;
private int Index = 0;
public float DialogueSpeed;
public float fasterSpeed;
public GameObject dialogueCanvas, yesText, noText, normalText; // normalBox;

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

/*

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

*/

/*
void SkipDialogue()
{
    //Turns off canvas
    dialogueCanvas.SetActive(false);

   //Debug.Log("Text Skipped!");
}
*/


/*
void dialogueFinished()
{
    if (Index == Sentences.Length )
    {
        dialogueCanvas.SetActive(false);
     //   Debug.Log("Text Done!");
    }
}

void answerY()
{
    yesText.SetActive(true);
    noText.SetActive(false);
    normalText.SetActive(false);
    Debug.Log("YES!");
}

void answerN()
{
    noText.SetActive(true);
    yesText.SetActive(false);
    normalText.SetActive(false);
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

*/


