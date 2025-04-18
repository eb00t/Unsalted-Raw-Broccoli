using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelTrigger : MonoBehaviour
{
    private MenuHandler _menuHandler;
    private GameObject _player;
    private GameObject _uiManager;
    [SerializeField] private bool doesUseTrigger;
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private string promptText;

    public enum SceneToLoad
    {
        Intermission,
        NextFloor,
        TitleScreen,
        Credits,
        EndScreen,
        SecretScreen,
        Tutorial
    }

    public SceneToLoad sceneToLoad;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _menuHandler = _uiManager.GetComponent<MenuHandler>();
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
                if (dataHolder.highestFloorCleared < 0)
                {
                    dataHolder.highestFloorCleared = 0;
                }
                break;
            case SceneToLoad.NextFloor:
                if (SceneManager.GetActiveScene().name != "Tutorial" && SceneManager.GetActiveScene().name != "Intermission")
                {
                    if (dataHolder.currentLevel == LevelBuilder.LevelMode.Floor1)
                    {
                        dataHolder.currentLevel = LevelBuilder.LevelMode.Floor2;
                        scene = "MainScene";
                        if (dataHolder.highestFloorCleared < 1)
                        {
                            dataHolder.highestFloorCleared = 1;
                        }
                    }
                    else if (dataHolder.currentLevel == LevelBuilder.LevelMode.Floor2)
                    {
                        dataHolder.currentLevel = LevelBuilder.LevelMode.Floor3;
                        scene = "MainScene"; 
                        if (dataHolder.highestFloorCleared < 2)
                        {
                            dataHolder.highestFloorCleared = 2;
                        }
                    }
                    else if (dataHolder.currentLevel == LevelBuilder.LevelMode.Floor3)
                    {
                        dataHolder.currentLevel = LevelBuilder.LevelMode.FinalBoss;
                        scene = "MainScene"; 
                        if (dataHolder.highestFloorCleared < 3)
                        {
                            dataHolder.highestFloorCleared = 3;
                        }
                    }
                }
                else
                {
                    WipeData();
                    scene = "MainScene";
                }
                break;
            case SceneToLoad.TitleScreen:
                scene = "StartScreen";
                break;
            case SceneToLoad.Credits:
                scene = "creditsScene";
                if (dataHolder.highestFloorCleared < 3)
                {
                    dataHolder.highestFloorCleared = 3;
                }
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
    
    public void WipeData()
    {
        dataHolder.savedItems.Clear();
        dataHolder.savedItemCounts.Clear();
        dataHolder.equippedConsumables = new int[5];
        dataHolder.currencyHeld = 0;
        dataHolder.currentLevel = LevelBuilder.LevelMode.Floor1;
        dataHolder.highestFloorCleared = 0;
    }
}
