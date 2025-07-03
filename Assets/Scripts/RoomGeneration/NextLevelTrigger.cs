using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class NextLevelTrigger : MonoBehaviour
{
    private CharacterMovement _characterMovement;
    private ItemPickupHandler _itemPickupHandler;
    private MenuHandler _menuHandler;
    private GameObject _player;
    private GameObject _uiManager;
    public float range = 5f;
    [SerializeField] private bool doesUseTrigger;
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private string promptText;
    [SerializeField] private bool doesDoorOpen;
    [SerializeField] private Sprite defaultSprite, openSprite;
    private SpriteRenderer _spriteRenderer;
    public bool inRange;

    public enum SceneToLoad
    {
        Intermission,
        NextFloor,
        TitleScreen,
        Credits,
        EndScreen,
        TheEnd,
        SecretScreen,
        Tutorial
    }

    public SceneToLoad sceneToLoad;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        if (_player != null)
        {
            _characterMovement = _player.GetComponent<CharacterMovement>();
            _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
        }

        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _menuHandler = _uiManager.GetComponent<MenuHandler>();

        if (_menuHandler != null)
        {
            _menuHandler.nextLevelTrigger = gameObject;
        }

        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != "creditsScene" && !doesUseTrigger)
        {
            if (!_characterMovement.uiOpen)
            {
                var dist = Vector3.Distance(transform.position, _player.transform.position);

                if (!inRange && dist <= range)
                {
                    inRange = true;
                    _itemPickupHandler.TogglePrompt(promptText, true, ControlsManager.ButtonType.Interact, "", null, false);
                    _menuHandler.nearestLevelTrigger = this;
                    if (doesDoorOpen)
                    {
                        _spriteRenderer.sprite = openSprite;
                    }
                }
                else if (inRange && dist > range)
                {
                    if (_menuHandler.nearestLevelTrigger != this) return;
                    if (doesDoorOpen)
                    {
                        _spriteRenderer.sprite = defaultSprite;
                    }
                    _menuHandler.nearestLevelTrigger = null;
                    inRange = false;
                    _itemPickupHandler.TogglePrompt("", false, ControlsManager.ButtonType.Interact, "", null, false);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!doesUseTrigger) return;
        LoadNextLevel();
    }

    public void LoadNextLevel()
    {
        string scene = "";
        switch (sceneToLoad)
        {
            case SceneToLoad.Intermission:
                scene = "Intermission";

                switch (dataHolder.currentLevel)
                {
                    case LevelBuilder.LevelMode.Floor1:
                        dataHolder.highestFloorCleared = 1;
                        SaveData.Instance.UpdateSave();
                        break;
                    case LevelBuilder.LevelMode.Floor2:
                        dataHolder.highestFloorCleared = 2;
                        SaveData.Instance.UpdateSave();
                        break;
                    case LevelBuilder.LevelMode.Floor3:
                        dataHolder.highestFloorCleared = 3;
                        SaveData.Instance.UpdateSave();
                        break;
                    case LevelBuilder.LevelMode.Floor4:
                        dataHolder.highestFloorCleared = 4;
                        SaveData.Instance.UpdateSave();
                        break;
                }
                break;
            case SceneToLoad.NextFloor:
                /*
                if (SceneManager.GetActiveScene().name != "Tutorial")
                {
                */
                switch (dataHolder.highestFloorCleared)
                {
                    case 0:
                        dataHolder.currentLevel = LevelBuilder.LevelMode.Floor1;
                        break;
                    case 1:
                        dataHolder.currentLevel = LevelBuilder.LevelMode.Floor2;
                        break;
                    case 2:
                        dataHolder.currentLevel = LevelBuilder.LevelMode.Floor3;
                        break;
                    case 3:
                        dataHolder.currentLevel = LevelBuilder.LevelMode.Floor4;
                        break;
                    case 4:
                        dataHolder.currentLevel = LevelBuilder.LevelMode.FinalBoss;
                        break;
                }
                scene = "MainScene";
                break;
            case SceneToLoad.TitleScreen:
                scene = "StartScreen";
                break;
            case SceneToLoad.Credits:
                scene = "creditsScene";
                break;
            case SceneToLoad.TheEnd:
                scene = "The End";
                break;
            case SceneToLoad.EndScreen:
                scene = "EndScreen";
                break;
            case SceneToLoad.Tutorial:
                scene = "Tutorial";
                break;
        }
        
        BlackoutManager.Instance.RaiseOpacity();
        gameObject.GetComponent<BoxCollider>().enabled = false;
        StartCoroutine(LoadNextScene(scene));
    }
    
     IEnumerator LoadNextScene(string scene)
    {
        yield return new WaitForSecondsRealtime(2f);
        SceneManager.LoadScene(scene);
    }

    public void ManuallyLoadTitleScreen() // This is just here for buttons
    {
        ButtonHandler.Instance.PlayConfirmSound();
        SceneManager.LoadScene("StartScreen");
    }

    public void ManuallyQuit() // Ditto
    {
        ButtonHandler.Instance.PlayBackSound();
        Application.Quit();
    }
}
