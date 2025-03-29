using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class dialogueControllerScript : MonoBehaviour
{
    private DialogueObjectHandler _dialogueObjectHandler;
    private LoreItemHandler _loreItemHandler;
    public bool isLore;
    public AllDialogue dialogueToLoad;
    private AllLore _loreToLoad;

    private int _dialogueID;

    //DIALOGUE CODE
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerText;
    [TextArea(3, 10)] public string[] sentences;
    private string _title;
    private int _index; // = 0;

    public float dialogueSpeed;

    //public float fasterSpeed;
    public GameObject dialogueCanvas, yesText, noText; // normalText; // normalBox;


    private void Awake()
    {
       /* if (_dialogueObjectHandler == null)
        {
            Debug.LogError(gameObject.name + " is missing a dialogueObjectHandler");
        }*/
    }

    private void Start()
    {
        dialogueCanvas = GameObject.FindWithTag("Dialogue");
        
        switch (isLore)
        {
            case false:
                _dialogueID = (int)dialogueToLoad;
                break;
            case true:
                _dialogueID = Random.Range(0, DialogueHandler.Instance.allLoreItems.Count);
                break;
        }
        
        LoadDialogue();
        speakerText = dialogueCanvas.transform.Find("Text box").transform.Find("SpeakerHolder").transform.Find("SpeakerText").GetComponent<TextMeshProUGUI>();
        dialogueText = dialogueCanvas.transform.Find("Text box").transform.Find("Normal Text").GetComponent<TextMeshProUGUI>();
        if (_dialogueObjectHandler.isAnyoneSpeaking == false)
        {
            speakerText.gameObject.SetActive(false);
        }

        //Start writing sentences
        dialogueText.text = string.Empty;
        speakerText.text = _dialogueObjectHandler.whoIsSpeaking[0];
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

        if (dialogueText.text == _dialogueObjectHandler.dialogueBodyText[_index])
        {
            // nextSen() moved here
            if (_index < _dialogueObjectHandler.dialogueBodyText.Length - 1)
            {
                _index++;
                dialogueText.text = string.Empty;
                if (_dialogueObjectHandler.whoIsSpeaking[_index] == null)
                {
                    _dialogueObjectHandler.whoIsSpeaking[_index] = _dialogueObjectHandler.whoIsSpeaking[_index - 1];
                }

                speakerText.text = _dialogueObjectHandler.whoIsSpeaking[_index];
                StartCoroutine(typeSentence());
            }
            else
            {
                _index = 0;
                dialogueCanvas.SetActive(false);
                
            }
        }
        else
        {
            StopAllCoroutines();
            dialogueText.text = _dialogueObjectHandler.dialogueBodyText[_index];
        }
    }

    void startSentence()
    {
        _index = 0;
        StartCoroutine(typeSentence());
    }

    IEnumerator typeSentence()
    {
        foreach (char Character in _dialogueObjectHandler.dialogueBodyText[_index].ToCharArray())
        {
            dialogueText.text += Character;
            yield return new WaitForSeconds(dialogueSpeed);
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

    void LoadDialogue()
    {
        switch (isLore)
        {
            case false:
                _dialogueObjectHandler = DialogueHandler.Instance.LoadDialogueScriptableObject(_dialogueID);
                break;
            case true:
                _loreItemHandler = DialogueHandler.Instance.LoadLoreScriptableObject(_dialogueID);
                break;
        }

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


