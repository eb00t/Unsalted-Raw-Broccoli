using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelTrigger : MonoBehaviour
{
    private CharacterMovement _characterMovement;
    private ItemPickupHandler _itemPickupHandler;
    private GameObject _player;
    private GameObject _uiManager;
    public float range = 5f;
    [SerializeField] private DataHolder dataHolder;

    public enum SceneToLoad
    {
        Intermission,
        NextFloor,
        TitleScreen
    }

    public SceneToLoad sceneToLoad;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _characterMovement = _player.GetComponent<CharacterMovement>();
        _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _uiManager.GetComponent<MenuHandler>().nextLevelTrigger = gameObject;
    }

    private void Update()
    {
        if (!_characterMovement.uiOpen)
        {
            var dist = Vector3.Distance(transform.position, _player.transform.position);

            if (dist <= range)
            {
                _itemPickupHandler.isPlrNearEnd = true;
                _itemPickupHandler.TogglePrompt("Continue ahead?", true, ControlsManager.ButtonType.ButtonEast);
            }
            else if (dist > range)
            {
                if (_itemPickupHandler.itemCount > 0) return;
                _itemPickupHandler.isPlrNearEnd = false;
                //_itemPickupHandler.TogglePrompt("", false, ControlsManager.ButtonType.ButtonEast);
            }
        }
    }

    public void LoadNextLevel()
    {
        string scene = "";
        switch (sceneToLoad)
        {
            case SceneToLoad.Intermission:
                scene = "Intermission";
                break;
            case SceneToLoad.NextFloor:
                if (SceneManager.GetActiveScene().name != "Tutorial")
                {
                    if (dataHolder.currentLevel == LevelBuilder.LevelMode.Floor1)
                    {
                        dataHolder.currentLevel = LevelBuilder.LevelMode.Floor2;
                        scene = "MainScene";
                    }
                    else if (dataHolder.currentLevel == LevelBuilder.LevelMode.Floor2)
                    {
                        scene = "StartScreen";
                    }
                }
                else
                {
                    dataHolder.currentLevel = LevelBuilder.LevelMode.Floor1;
                    scene = "MainScene";
                }
                break;
            case SceneToLoad.TitleScreen:
                scene = "StartScreen";
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
}
