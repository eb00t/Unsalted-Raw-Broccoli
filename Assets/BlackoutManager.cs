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
    public bool blackoutComplete;
    private float _lerpTime;
    private bool _fadedOut;
    private float _failSafeTimer = 15;
    private bool _loading = true;
    private float _timer = 2;
    
    private void Start()
    {
        _failSafeTimer = LevelBuilder.Instance.howManyRoomsToSpawn + 6;
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
        _loading = false;
        blackoutImage.gameObject.SetActive(true);
        _lerpTime = 0;
        _lerpDirection = LerpDirection.FadeOut;
        _timer = 2;
    }

    public void RaiseOpacity()
    {
        blackoutComplete = false;
        blackoutImage.gameObject.SetActive(true);
        _lerpTime = 0;
        _lerpDirection = LerpDirection.FadeIn;
    }

    private void Update()
    {
        if (LevelBuilder.Instance.bossRoomGeneratingFinished && _fadedOut == false && LevelBuilder.Instance.currentFloor != LevelBuilder.LevelMode.Intermission)
        {
            _fadedOut = true;
            LowerOpacity();
        } 
        else if (LevelBuilder.Instance.currentFloor == LevelBuilder.LevelMode.Intermission && _fadedOut == false)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                _fadedOut = true;
                LowerOpacity();
            }
        }

        switch (_lerpDirection)
        {
            case LerpDirection.Neither:
                blackoutImage.color = new Color(0.02745098f, 0.1019608f, 0.2235294f);
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
        
        if (_loading)
        {
            //Debug.Log(_failSafeTimer);
            _failSafeTimer -= Time.deltaTime;
            if (_failSafeTimer <= 0)
            {
                Debug.LogError("Failsafe timer has expired, reloading scene to fix errors.");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        
        if (blackoutImage.color.a <= 0)
        {
            blackoutComplete = true;
        }
    }
}
    


