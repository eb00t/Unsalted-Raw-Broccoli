using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class dialogueControllerScript : MonoBehaviour
{
    public bool replayable;
    public DialogueObjectHandler dialogueToLoad;
    public DialogueObjectHandler shopCloseDialogue;
    public LoreItemHandler loreToLoad;
    private ItemPickupHandler _itemPickupHandler;
    [SerializeField] private GameObject shopCanvas;
    public bool isShop, isEndText;
    [SerializeField] private float range;
    [SerializeField] public int dialogueID;
    private GameObject _player, _dialogueCanvas, _uiManager;
    private MenuHandler _menuHandler;
    public bool randomLore; // Usually true, false if the lore is spawned directly.
    
    public enum DialogueOrLore
    {
        Dialogue,
        Lore
    }
    public DialogueOrLore dialogueOrLore;

    //DIALOGUE CODE
    private TextMeshProUGUI dialogueText;
    private TextMeshProUGUI speakerText;
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
        _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _menuHandler = _uiManager.GetComponent<MenuHandler>();
        _dialogueCanvas = _menuHandler.dialogueGUI;
        int loreChoice = Random.Range(0, LoreReference.Instance.allLoreItems.Count);
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

        switch (dialogueOrLore)
        {
            case DialogueOrLore.Lore:
                if (randomLore)
                {
                    loreToLoad = LoreReference.Instance.allLoreItems[loreChoice];
                }
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
            switch (dialogueOrLore)
            {
                case DialogueOrLore.Dialogue:
                    _itemPickupHandler.isPlrNearDialogue = true;
                    break;
                case DialogueOrLore.Lore:
                    _itemPickupHandler.isPlrNearLore = true;
                    break;
            }
            
            if (_dialogueCanvas.activeSelf)
            {
                _itemPickupHandler.TogglePrompt("Next", true, ControlsManager.ButtonType.ProgressDialogue, "", null);
            }
            else
            {
                if (!isShop)
                {
                    _itemPickupHandler.TogglePrompt("Interact", true, ControlsManager.ButtonType.Interact, "", null);
                    _menuHandler.dialogueController = this;
                }
                else if (isShop && shopCanvas.activeSelf)
                {
                    _itemPickupHandler.TogglePrompt("Close Shop", true, ControlsManager.ButtonType.Back, "", null);
                    _itemPickupHandler.isPlrNearShop = true;
                    _menuHandler.dialogueController = this;
                }
                else
                {
                    _itemPickupHandler.TogglePrompt("Interact", true, ControlsManager.ButtonType.Interact, "", null);
                    _menuHandler.dialogueController = this;
                }
            }
        }
        else if (dist > range)
        {
            if (_menuHandler.dialogueController != this) return;
            switch (dialogueOrLore)
            {
                case DialogueOrLore.Dialogue:
                    _itemPickupHandler.isPlrNearDialogue = false;
                    break;
                case DialogueOrLore.Lore:
                    _itemPickupHandler.isPlrNearLore = false;
                    break;
            }
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

   public void LoadDialogue(DialogueObjectHandler dialogueHandler)
   {
       dialogueToLoad = dialogueHandler;
       
       if (isShop && isEndText)
       {
           DialogueHandler.Instance.LoadDialogueScriptableObject(shopCloseDialogue);
       }
       else
       {
           DialogueHandler.Instance.LoadDialogueScriptableObject(dialogueHandler);
       }
       
       DialogueHandler.Instance.StartSentence(this);
       if (replayable == false)
       {
           DialogueHandler.Instance.trigger = transform.gameObject;
       }
       DialogueHandler.Instance.currentNPC = gameObject.GetComponent<NPCHandler>();

   }

    public void LoadLore(LoreItemHandler loreItem)
    {
        DialogueHandler.Instance.LoadLoreScriptableObject(loreItem);
        DialogueHandler.Instance.StartSentence(this);
        if (replayable == false)
        {
            DialogueHandler.Instance.trigger = transform.parent.gameObject;
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


