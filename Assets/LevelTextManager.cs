using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelTextManager : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text subtitleText;
    private bool _doneWaiting;

    void Start()
    {
        if (titleText == null)
        {
            titleText = transform.Find("Title").GetComponent<TMP_Text>();
        }

        switch (LevelBuilder.Instance.currentFloor)
        {
            case LevelBuilder.LevelMode.TEST:
                titleText.text = ("TEST FLOOR");
                break;
            case LevelBuilder.LevelMode.Floor1:
                titleText.text = ("FLOOR 1");
                break;
            case LevelBuilder.LevelMode.Floor2:
                titleText.text = ("FLOOR 2");
                break;
            case LevelBuilder.LevelMode.Floor3:
                titleText.text = ("FLOOR 3");
                break;
        }

        if (subtitleText == null)
        {
            subtitleText = transform.Find("Subtitle").GetComponent<TMP_Text>();
        }

        switch (LevelBuilder.Instance.currentFloor)
        {
            case LevelBuilder.LevelMode.TEST:
                subtitleText.text = ("You probably shouldn't be here...");
                break;
            case LevelBuilder.LevelMode.Floor1:
                subtitleText.text = ("And so it begins...");
                break;
            case LevelBuilder.LevelMode.Floor2:
                subtitleText.text = ("Welcome to the midpoint.");
                break;
            case LevelBuilder.LevelMode.Floor3:
                subtitleText.text = ("...");
                break;
        }

        StartCoroutine(LowerTextOpacity());

    }

    IEnumerator LowerTextOpacity()
    {
        while (true)
        {
            if (!_doneWaiting)
            {
                yield return new WaitForSecondsRealtime(2f);
            }

            _doneWaiting = true;
            var subtitleTextColor = subtitleText.color;
            subtitleTextColor.a -= .1f;
            subtitleText.color = subtitleTextColor;
            titleText.color = subtitleText.color;
            yield return new WaitForSeconds(0.1f);
        }
}

// Update is called once per frame
    void Update()
    {
        if (titleText.color.a <= 0)
        {
            titleText.gameObject.SetActive(false);
            StopCoroutine(LowerTextOpacity());
            
        }  
        if (subtitleText.color.a <= 0)
        {
            subtitleText.gameObject.SetActive(false);
        }
    }
}
