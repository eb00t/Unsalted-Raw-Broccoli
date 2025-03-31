using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class dialogueControllerScript : MonoBehaviour
{
    public bool isLore;
    public AllDialogue dialogueToLoad;
    private AllLore _loreToLoad;
    private ItemPickupHandler _itemPickupHandler;
    [SerializeField] private float range;
    [SerializeField] private int _dialogueID;
    private GameObject _player, _dialogueCanvas, _uiManager;
    private MenuHandler _menuHandler;

    //DIALOGUE CODE
    private TextMeshProUGUI dialogueText;
    private TextMeshProUGUI speakerText;
    public List<string> speakers;
    [TextArea(3, 10)] public List<string> sentences;
    private string _title;
    [SerializeField] private int _index; // = 0;

    public float dialogueSpeed;

    //public float fasterSpeed;
    public GameObject yesText, noText; // normalText; // normalBox;


    private void Awake()
    {
       /* if (_dialogueObjectHandler == null)
        {
            Debug.LogError(gameObject.name + " is missing a dialogueObjectHandler");
        }*/
    }

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _dialogueCanvas = GameObject.FindWithTag("Dialogue");
        _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _menuHandler = _uiManager.GetComponent<MenuHandler>();
      /* foreach (var text in _dialogueCanvas.GetComponentsInChildren<TextMeshProUGUI>())
       {
           switch (text.name)
           {
               case "Normal Text":
                   dialogueText = text;
                   break;
               case "SpeakerText":
                   speakerText = text;
                   break;
           }
       }*/

       switch (isLore)
       {
           case false:
               _dialogueID = (int)dialogueToLoad;
               break;
           case true:
               _dialogueID = Random.Range(0, DialogueHandler.Instance.allLoreItems.Count);
               break;
       }
       //Start writing sentences
       //startSentence();
    }

    private void Update()
    {
        var dist = Vector3.Distance(transform.position, _player.transform.position);

        if (dist <= range)
        {
            _itemPickupHandler.isPlrNearDialogue = true;
            
            if (_dialogueCanvas.activeSelf)
            {
                _itemPickupHandler.TogglePrompt("Next", true, ControlsManager.ButtonType.ButtonSouth);
            }
            else
            {
                _itemPickupHandler.TogglePrompt("Interact", true, ControlsManager.ButtonType.ButtonEast);
                _menuHandler.dialogueController = this;
            }
        }
        else if (dist > range)
        {
            if (_menuHandler.dialogueController != this) return;
            _itemPickupHandler.isPlrNearDialogue = false;
        }
        
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
    

   

    void answerY()
    {
        yesText.SetActive(true);
        noText.SetActive(false);
        _dialogueCanvas.SetActive(false);
        //  Debug.Log("YES!");
    }

    void answerN()
    {
        noText.SetActive(true);
        yesText.SetActive(false);
        _dialogueCanvas.SetActive(false);
        //    Debug.Log("NO");
    }

   public void LoadDialogue()
    {
        switch (isLore)
        {
            case false:
                DialogueHandler.Instance.LoadDialogueScriptableObject(_dialogueID);
                DialogueHandler.Instance.StartSentence();
                break;
            case true:
                DialogueHandler.Instance.LoadLoreScriptableObject(_dialogueID);
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


