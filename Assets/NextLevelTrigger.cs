using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelTrigger : MonoBehaviour
{
    
    public enum SceneToLoad
    {
        Intermission,
        NextFloor,
        TitleScreen
    }
    public SceneToLoad sceneToLoad;
    
    void OnTriggerEnter(Collider other)
    {
        string scene = "";
        switch (sceneToLoad)
        {
            case SceneToLoad.Intermission:
                scene = "Intermission";
                break;
            case SceneToLoad.NextFloor:
                scene = "MainScene";
                break;
            case SceneToLoad.TitleScreen:
                scene = "StartScreen";
                break;
            
        }
        if (other.CompareTag("Player"))
        {
            if (SceneManager.GetActiveScene().name != "Tutorial")
            {
                BlackoutManager.Instance.RaiseOpacity();
            }
            
            gameObject.GetComponent<BoxCollider>().enabled = false;
            StartCoroutine(LoadNextScene(scene));
        }
    }
    
     IEnumerator LoadNextScene(string scene)
    {
        yield return new WaitForSecondsRealtime(3f);
        SceneManager.LoadScene(scene);
    }
}
