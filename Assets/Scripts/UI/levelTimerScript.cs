using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class levelTimerScript : MonoBehaviour
{
    //Timer
    public float timer;
    public TMP_Text timerText;
   // public bool timerOn = true;
    public GameObject timerDisplay, timerObj;

    // Update is called once per frame
    void Update()
    {
        timerStart();

        if(timerDisplay.activeInHierarchy == true)
        {
            timerObj.SetActive(false);
        }
    }

    //Timer Code
    public void timerStart()
    {
        timer += Time.deltaTime;
        updateTimer(timer);
    }

    void updateTimer(float currentTime)
    {
        currentTime += 1;

        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = string.Format("Time Spent on Level : {0:00} : {1:00}", minutes, seconds);
    }
}
