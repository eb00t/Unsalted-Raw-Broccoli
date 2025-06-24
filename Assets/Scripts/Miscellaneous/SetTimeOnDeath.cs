using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class SetTimeOnDeath : MonoBehaviour
{
    private static readonly int StaticLaugh = Animator.StringToHash("StaticLaugh");
    [SerializeField] private CanvasGroup hudCanvasGroup, youDiedGroup, pressBtnGroup, vignetteGroup, bckGroup;
    [SerializeField] private Image parentImage;
    [SerializeField] private Animator animator;
    [SerializeField] private Material circleFade;

    private void OnEnable()
    {
        circleFade.SetColor("_Color", new Color(0f, 0f, 0f, 1f));
        circleFade.SetFloat("_HoleRadius", 1f);
        vignetteGroup.alpha = 0f;
        bckGroup.alpha = 0f;
        var color = parentImage.color;
        color.a = 0f;
        parentImage.color = color;
        youDiedGroup.alpha = 0f;
        pressBtnGroup.alpha = 0f;
        var deathSequence = DOTween.Sequence().SetUpdate(true);

        hudCanvasGroup.DOFade(0f, .5f).SetUpdate(true);
        deathSequence.Append(circleFade.DOFloat(0f, "_HoleRadius", .5f));
        //deathSequence.Append(circleFade.DOFloat(1f, "_HoleRadius", 0.75f).SetEase(Ease.OutCubic));
        deathSequence.Join(vignetteGroup.DOFade(1f, .5f));
        deathSequence.Join(bckGroup.DOFade(1f, .5f));
        deathSequence.Join(parentImage.DOFade(0.41f, .5f));
        deathSequence.Append(circleFade.DOColor(new Color(0,0,0,0), "_Color", .5f));
        deathSequence.Append(youDiedGroup.DOFade(1f, .5f));
        deathSequence.Append(pressBtnGroup.DOFade(1f, .5f));

        deathSequence.OnComplete(() =>
        {
            Time.timeScale = 0f;
        });
    }

    public void SetTimeScale()
    {
        Time.timeScale = 0f;
    }

    public void FadeHudOut()
    {
        hudCanvasGroup.DOFade(0f, 0.5f).SetUpdate(true);
    }

    public void StartLaugh()
    {
        animator.SetBool(StaticLaugh, true);
    }
}
