using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BlackoutManager : MonoBehaviour
{
    public static BlackoutManager Instance { get; private set; }
    
    public Image blackoutImage;
    public Color blackoutColor;
    public Color transparentColor;
    private float _lerpTime;
    private bool _fadedOut;
    private enum LerpDirection
    {
        Neither,
        FadeOut,
        FadeIn,
    }
    private LerpDirection _lerpDirection;
    void Awake()
    {
        _lerpDirection = LerpDirection.Neither;
        transparentColor.a = 0;
        if (blackoutImage == null)
        {
            blackoutImage = transform.Find("Blackout").gameObject.GetComponent<Image>();
        }
        gameObject.SetActive(true);
        
        if (Instance != null)
        {
            Debug.LogError("More than one Blackout Manager script in the scene.");
        }

        Instance = this;
    }

    public void LowerOpacity()
    {
        _lerpTime = 0;
       _lerpDirection = LerpDirection.FadeOut;
    }

    public void RaiseOpacity()
    {
        _lerpTime = 0;
        _lerpDirection = LerpDirection.FadeIn;
    }

    private void Update()
    {
        if (LevelBuilder.Instance.bossRoomGeneratingFinished && _fadedOut == false)
        {
            _fadedOut = true;
            LowerOpacity();
        }
        if (blackoutImage.color.a <= 0 && _lerpDirection == LerpDirection.FadeOut)
        {
            blackoutImage.gameObject.SetActive(false);
            _lerpDirection = LerpDirection.Neither;
        }

        switch (_lerpDirection)
        {
            case LerpDirection.Neither:
                blackoutImage.color = Color.black;
                break;
            case LerpDirection.FadeOut:
                blackoutImage.color = Color.Lerp(blackoutColor, transparentColor, _lerpTime);
                break;  
            case LerpDirection.FadeIn:
                blackoutImage.color = Color.Lerp(transparentColor, blackoutColor, _lerpTime);
                break;
        }
        
        _lerpTime += .002f; 
        
    }
}
