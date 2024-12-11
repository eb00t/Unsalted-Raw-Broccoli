using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelTrigger : MonoBehaviour
{
//TODO: THIS CODE IS ABSOLUTE DOGSHIT, AND DOES NOT DO WHAT IT IS SUPPOSED TO. PLEASE FIX WHEN POSSIBLE
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            BlackoutManager.Instance.RaiseOpacity();
            gameObject.GetComponent<BoxCollider>().enabled = false;
            StartCoroutine(LoadNextScene());
        }
    }
    
     IEnumerator LoadNextScene()
    {
        yield return new WaitForSecondsRealtime(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
