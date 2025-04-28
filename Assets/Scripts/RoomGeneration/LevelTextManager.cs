using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    //private float _timer = 2;
    
    public Color loadBckColor, loadSqrColor;
    [SerializeField] private Image loadBck, loadSqr;
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
            case LevelBuilder.LevelMode.Floor4:
                titleText.text = ("FLOOR 4");
                break;
            case LevelBuilder.LevelMode.FinalBoss:
                titleText.text = ("FLOOR X");
                break;
            case LevelBuilder.LevelMode.Intermission:
                titleText.text = ("INTERMISSION");
                break;
            case LevelBuilder.LevelMode.Tutorial:
                titleText.text = ("TUTORIAL");
                break;
            case LevelBuilder.LevelMode.TitleScreen:
                titleText.text = ("");
                break;
            default:
                titleText.text = ("what are you doing here, man?");
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
                subtitleText.text = ("Your journey continues...");
                break;
            case LevelBuilder.LevelMode.Floor3:
                subtitleText.text = ("Welcome to the midpoint.");
                break;
            case LevelBuilder.LevelMode.Floor4:
                subtitleText.text = ("...");
                break;
            case LevelBuilder.LevelMode.FinalBoss:
                subtitleText.text = ("The 'final' showdown...");
                break;
            case LevelBuilder.LevelMode.Intermission:
                subtitleText.text = ("A brief respite...");
                break;
            case LevelBuilder.LevelMode.Tutorial:
                subtitleText.text = ("");
                break;
            case LevelBuilder.LevelMode.TitleScreen:
                subtitleText.text = ("");
                break;
            default:
                subtitleText.text = ("no, seriously. something went wrong.");
                break;
        }
    }

    IEnumerator WaitToLowerTextOpacity()
    {
        if (LevelBuilder.Instance.currentFloor is (LevelBuilder.LevelMode.Tutorial
            or LevelBuilder.LevelMode.Intermission or LevelBuilder.LevelMode.TitleScreen))
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
    
    private void Update()
    {
        if (LevelBuilder.Instance.bossRoomGeneratingFinished && _fadedOut == false && LevelBuilder.Instance.currentFloor is not (LevelBuilder.LevelMode.Intermission or LevelBuilder.LevelMode.Tutorial))
        {
            _fadedOut = true;
            StartCoroutine(WaitToLowerTextOpacity());
        }
        else if (LevelBuilder.Instance.currentFloor is (LevelBuilder.LevelMode.Intermission or LevelBuilder.LevelMode.Tutorial or LevelBuilder.LevelMode.TitleScreen) && _fadedOut == false)
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
                loadBck.color = Color.Lerp(transparentColor, loadBckColor, _lerpTime);
                loadSqr.color = Color.Lerp(transparentColor, loadSqrColor, _lerpTime);
                subtitleText.color = titleText.color;
                break;
        }
        
        _lerpTime += .01f; 
        
    }
}
    
