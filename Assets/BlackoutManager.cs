using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    private float _failSafeTimer;

    private void OnEnable()
    {
        _failSafeTimer = 10;
        StartCoroutine(Failsafe());
    }

    private void OnDisable()
    {
        StopCoroutine(Failsafe());
        _failSafeTimer = 10;
    }

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
        gameObject.SetActive(true);
        _lerpTime = 0;
       _lerpDirection = LerpDirection.FadeOut;
    }

    public void RaiseOpacity()
    {
        gameObject.SetActive(true);
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

        switch (_lerpDirection)
        {
            case LerpDirection.Neither:
                blackoutImage.color = Color.black;
                break;
            case LerpDirection.FadeOut:
                blackoutImage.color = Color.Lerp(blackoutColor, transparentColor, _lerpTime);
                if (blackoutImage.color.a <= 0)
                {
                    blackoutImage.gameObject.SetActive(false);
                }
                break;  
            case LerpDirection.FadeIn:
                blackoutImage.color = Color.Lerp(transparentColor, blackoutColor, _lerpTime);
                break;
        }
        
        _lerpTime += .002f; 
        
    }

    IEnumerator Failsafe()
    {
        yield return new WaitForSecondsRealtime(1);
        _failSafeTimer--;
        if (_failSafeTimer <= 0)
        {
            Debug.LogError("Failsafe timer has expired, reloading scene to fix errors.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
        
    }
