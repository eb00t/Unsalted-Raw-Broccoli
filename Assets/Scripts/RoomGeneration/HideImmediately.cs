using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HideImmediately : MonoBehaviour
{
    int _disableCount;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(HideMe());
        _disableCount = 0;
    }

    IEnumerator HideMe()
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (_disableCount >= 1 && LevelBuilder.Instance.currentFloor == LevelBuilder.LevelMode.EndScreen)
        {
            SceneManager.LoadScene("creditsScene");
        }
        _disableCount++;
        
    }
}
