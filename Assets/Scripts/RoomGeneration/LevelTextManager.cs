using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelTextManager : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text subtitleText;
    private bool _doneWaiting;
    public Color textColor;
    public Color transparentColor;
    private float _lerpTime;
    private bool _fadedOut;
    private Color _startColor;
    private float _timer = 2;
    private enum LerpDirection
    {
        Neither,
        FadeOut,
        FadeIn,
    }
    private LerpDirection _lerpDirection;

    private void Awake()
    {
        _startColor = titleText.color;
        RaiseTextOpacity();
    }

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
            case LevelBuilder.LevelMode.Intermission:
                titleText.text = ("");
                break;
            case LevelBuilder.LevelMode.Tutorial:
                titleText.text = ("TUTORIAL");
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
            case LevelBuilder.LevelMode.Intermission:
                subtitleText.text = ("");
                break;
            case LevelBuilder.LevelMode.Tutorial:
                subtitleText.text = ("");
                break;
        }
    }

    IEnumerator WaitToLowerTextOpacity()
    {
        if (LevelBuilder.Instance.currentFloor is (LevelBuilder.LevelMode.Tutorial
            or LevelBuilder.LevelMode.Intermission))
        {
            yield return new WaitForSecondsRealtime(3f);
        }
        else
        {
            yield return new WaitForSecondsRealtime(2f);
        }
        LowerTextOpacity();
    }

    void LowerTextOpacity()
    {
        _lerpTime = 0;
        _lerpDirection = LerpDirection.FadeOut;
    }

    void RaiseTextOpacity()
    {
        _lerpTime = 0;
        _lerpDirection = LerpDirection.FadeIn;
    }
    
    private void FixedUpdate()
    {
        if (LevelBuilder.Instance.bossRoomGeneratingFinished && _fadedOut == false && LevelBuilder.Instance.currentFloor is not (LevelBuilder.LevelMode.Intermission or LevelBuilder.LevelMode.Tutorial))
        {
            _fadedOut = true;
            StartCoroutine(WaitToLowerTextOpacity());
        }
        else if (LevelBuilder.Instance.currentFloor is (LevelBuilder.LevelMode.Intermission or LevelBuilder.LevelMode.Tutorial) && _fadedOut == false)
        {
            _fadedOut = true;
            StartCoroutine(WaitToLowerTextOpacity());
        }
        if (titleText.color.a <= 0 && _lerpDirection == LerpDirection.FadeOut)
        {
            titleText.gameObject.SetActive(false);
            subtitleText.gameObject.SetActive(false);
        }

        switch (_lerpDirection)
        {
            case LerpDirection.Neither:
                textColor = _startColor;
                titleText.color = textColor;
                subtitleText.color = textColor;
                break;
            case LerpDirection.FadeOut:
                _lerpTime += .002f; 
                titleText.color = Color.Lerp(textColor, transparentColor, _lerpTime);
                subtitleText.color = titleText.color;
                if (titleText.color.a <= 0)
                {
                    titleText.gameObject.SetActive(false);
                    subtitleText.gameObject.SetActive(false);
                }
                break;  
            case LerpDirection.FadeIn:
                _lerpTime += .006f; 
                titleText.color = Color.Lerp(transparentColor, textColor, _lerpTime);
                subtitleText.color = titleText.color;
                break;
        }
        
        _lerpTime += .01f; 
        
    }
}
    
