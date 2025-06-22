using System;
using UnityEngine;
using DG.Tweening;

public class SetTimeOnDeath : MonoBehaviour
{
    private static readonly int StaticLaugh = Animator.StringToHash("StaticLaugh");
    [SerializeField] private CanvasGroup hudCanvasGroup;
    [SerializeField] private Animator animator;

    public void SetTimeScale()
    {
        Time.timeScale = 0f;
    }

    public void FadeHudOut()
    {
        hudCanvasGroup.DOFade(0f, 0.5f);
    }

    public void StartLaugh()
    {
        animator.SetBool(StaticLaugh, true);
    }
}
