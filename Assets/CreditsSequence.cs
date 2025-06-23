using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using FMOD.Studio;
using UnityEngine.SceneManagement;

public class CreditsSequence : MonoBehaviour
{
    [SerializeField] private List<CanvasGroup> slides;
    [SerializeField] private float totalCreditDuration;
    [SerializeField] private float fadeInDuration;
    [SerializeField] private float fadeOutDuration;

    private void Start()
    {
        InitialiseCreditsSequence();
    }

    private void InitialiseCreditsSequence()
    {
        var totalDur = totalCreditDuration - (fadeOutDuration * slides.Count) - (fadeInDuration * slides.Count);
        var betweenDuration = totalDur / slides.Count;
        var creditsSeq = DOTween.Sequence().SetUpdate(true);

        foreach (var cg in slides)
        {
            creditsSeq.Append(cg.DOFade(1f, fadeInDuration));
            creditsSeq.Append(cg.DOFade(0f, fadeOutDuration).SetDelay(betweenDuration));
        }

        creditsSeq.OnComplete(() =>
        { 
            Debug.Log("Credits Sequence Complete");
        });
    }


    private void Update()
    {
        AudioManager.Instance.MusicEventInstance.getPlaybackState(out var playbackState);
        if (playbackState is PLAYBACK_STATE.STOPPING or PLAYBACK_STATE.STOPPED)
        {
            Debug.Log("Music Ended");
            SceneManager.LoadScene("StartScreen", LoadSceneMode.Single);
        }
    }
}
