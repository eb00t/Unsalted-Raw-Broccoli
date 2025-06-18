using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LevelTextManager : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text subtitleText;
    private bool _doneWaiting;
    private float _lerpTime;
    private bool _fadedOut;
    [SerializeField] private Image loadBck, loadSqr;

    [SerializeField] private CanvasGroup textGroup;
    private Tween _textTween;

    private void Awake()
    {
        RaiseTextOpacity();
    }

    private void Start()
    {
        if (subtitleText == null)
        {
            subtitleText = transform.Find("Subtitle").GetComponent<TMP_Text>();
        }
        
        switch (LevelBuilder.Instance.currentFloor)
        {
            case LevelBuilder.LevelMode.TEST:
                titleText.text = ("TEST FLOOR");
                subtitleText.text = ("You probably shouldn't be here...");
                break;
            case LevelBuilder.LevelMode.Floor1:
                titleText.text = ("FLOOR 1");
                subtitleText.text = ("And so it begins...");
                break;
            case LevelBuilder.LevelMode.Floor2:
                titleText.text = ("FLOOR 2");
                subtitleText.text = ("Your journey continues...");
                break;
            case LevelBuilder.LevelMode.Floor3:
                titleText.text = ("FLOOR 3");
                subtitleText.text = ("Welcome to the midpoint.");
                break;
            case LevelBuilder.LevelMode.Floor4:
                titleText.text = ("FLOOR 4");
                subtitleText.text = ("...");
                break;
            case LevelBuilder.LevelMode.FinalBoss:
                titleText.text = ("FLOOR X");
                subtitleText.text = ("The 'final' showdown...");
                break;
            case LevelBuilder.LevelMode.Intermission:
                titleText.text = ("CONSTRUCT LOBBY");
                subtitleText.text = ("A place of beginnings and endings...");
                break;
            case LevelBuilder.LevelMode.Tutorial:
                titleText.text = ("TUTORIAL");
                subtitleText.text = ("Blank Class-struct...");
                break;
            case LevelBuilder.LevelMode.TitleScreen:
                titleText.text = ("");
                subtitleText.text = ("");
                break;
            case LevelBuilder.LevelMode.EndScreen:
                titleText.text = ("...");
                subtitleText.text = ("...");
                break;
            default:
                titleText.text = ("what are you doing here, man?");
                subtitleText.text = ("no, seriously. something went wrong.");
                break;
        }
    }

    private IEnumerator WaitToLowerTextOpacity()
    {
        if (LevelBuilder.Instance.currentFloor is (LevelBuilder.LevelMode.Tutorial
            or LevelBuilder.LevelMode.Intermission or LevelBuilder.LevelMode.TitleScreen or LevelBuilder.LevelMode.EndScreen))
        {
            yield return new WaitForSecondsRealtime(3f);
        }
        else
        {
            yield return new WaitForSecondsRealtime(2f);
        }
        LowerTextOpacity();
    }

    private void LowerTextOpacity()
    {
        _textTween?.Kill();
        _textTween = textGroup.DOFade(0f, 2f).SetUpdate(true);
    }

    private void RaiseTextOpacity()
    {
        titleText.gameObject.SetActive(true);
        subtitleText.gameObject.SetActive(true);

        _textTween = textGroup.DOFade(1, 1f).SetUpdate(true);
    }
    
    private void Update()
    {
        var level = LevelBuilder.Instance.currentFloor;

        if (_fadedOut) return;

        if (level is LevelBuilder.LevelMode.Intermission or LevelBuilder.LevelMode.Tutorial or LevelBuilder.LevelMode.TitleScreen or LevelBuilder.LevelMode.EndScreen)
        {
            _fadedOut = true;
            StartCoroutine(WaitToLowerTextOpacity());
        }
        else if (LevelBuilder.Instance.bossRoomGeneratingFinished)
        {
            _fadedOut = true;
            StartCoroutine(WaitToLowerTextOpacity());
        }
    }
}
    
