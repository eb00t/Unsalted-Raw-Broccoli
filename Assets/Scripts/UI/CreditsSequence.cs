using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using FMOD.Studio;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CreditsSequence : MonoBehaviour
{
    [SerializeField] private List<CanvasGroup> slides;
    [SerializeField] private CanvasGroup buttonsGroup, thanksGroup;
    [SerializeField] private TitleType titleType;
    [SerializeField] private float totalCreditDuration;
    [SerializeField] private float fadeInDuration;
    [SerializeField] private float fadeOutDuration;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private ControlsManager controlsManager;
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private GameObject menuBtn, quitBtn;

    private void Start()
    {
        if (dataHolder.demoMode)
        {
            quitBtn.SetActive(false);
            
            if (dataHolder.fastestClearTime == 0 || (dataHolder.playerTimeToClear < dataHolder.fastestClearTime))
            {
                dataHolder.fastestClearTime = dataHolder.playerTimeToClear;
            }
            
            slides[0].GetComponent<TextMeshProUGUI>().text = 
                "RUN TIME\n<color=#00A2FF>" + TimeSpan.FromSeconds(dataHolder.playerTimeToClear).ToString(@"hh\:mm\:ss") + "</color>"
                             + "\n\nFASTEST RUN TIME\n<color=#00A2FF>" + TimeSpan.FromSeconds(dataHolder.fastestClearTime).ToString(@"hh\:mm\:ss") + "</color>";
        }
        else
        {
            if (dataHolder.playerPersonalBestTime == 0 || (dataHolder.playerTimeToClear < dataHolder.playerPersonalBestTime))
            {
                dataHolder.playerPersonalBestTime = dataHolder.playerTimeToClear;
            }
            
            slides[0].GetComponent<TextMeshProUGUI>().text =
                "RUN TIME\n<color=#00A2FF>" + TimeSpan.FromSeconds(dataHolder.playerTimeToClear).ToString(@"hh\:mm\:ss") + "</color>"
                             + "\n\nPERSONAL FASTEST RUN TIME\n<color=#00A2FF>" + TimeSpan.FromSeconds(dataHolder.playerPersonalBestTime).ToString(@"hh\:mm\:ss") + "</color>";
        }

        dataHolder.playerTimeToClear = 0f;

        InitialiseCreditsSequence();

        dataHolder.hasBeatenBaseGame = true;
        SaveData.Instance.UpdateSave();
    }

    private void InitialiseCreditsSequence()
    {
        var totalDur = totalCreditDuration - (fadeOutDuration * slides.Count) - (fadeInDuration * slides.Count);
        var betweenDuration = totalDur / slides.Count;
        var creditsSeq = DOTween.Sequence().SetUpdate(true);

        for (var i = 0; i < slides.Count; i++)
        {
            if (i == slides.Count - 1)
            {
                var showTitle = slides[i].DOFade(1f, 0f).OnComplete(() =>
                {
                    titleType.enabled = true;
                });
                
                var buttonFade = buttonsGroup.DOFade(1f, .5f).OnComplete(() =>
                {
                    menuBtn.GetComponent<Button>().interactable = true;
                    quitBtn.GetComponent<Button>().interactable = true;
                    eventSystem.SetSelectedGameObject(menuBtn);
                });

                var slideFade = slides[i].GetComponent<RectTransform>().DOAnchorPosY(50f, 1f).SetEase(Ease.OutBounce).SetDelay(1.7f);
                
                creditsSeq.Append(showTitle);
                creditsSeq.Append(slideFade);
                creditsSeq.Append(thanksGroup.DOFade(1f, .5f));
                creditsSeq.Append(buttonFade);
            }
            else
            {
                creditsSeq.Append(slides[i].DOFade(1f, fadeInDuration));
                creditsSeq.Append(slides[i].DOFade(0f, fadeOutDuration).SetDelay(betweenDuration));
            }
        }

        creditsSeq.OnComplete(() =>
        { 
            StartCoroutine(MusicFinishedLoadMainMenu());
        });
    }
    
    private void Update()
    {
        controlsManager.CheckControl();
        
        if (!dataHolder.isGamepad)
        {
            var interactable = GetInteractable();
            if (interactable != null)
            {
                SwitchSelected(interactable);
            }

            if (!Cursor.visible || Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
        else if (dataHolder.isGamepad && Cursor.visible || Cursor.lockState == CursorLockMode.None)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    
    private static GameObject GetInteractable()
    {
        var pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };

        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            var go = result.gameObject;
			
            if (go.TryGetComponent<Selectable>(out var selectable) && selectable.interactable)
            {
                return selectable.gameObject;
            }
        }

        return null;
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) return;
        SwitchSelected(menuBtn);
    }
    
    public void SwitchSelected(GameObject g)
    {
        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(g);
    }

    private IEnumerator MusicFinishedLoadMainMenu()
    {
        yield return new WaitUntil(() => GetPlaybackState() is PLAYBACK_STATE.STOPPING or PLAYBACK_STATE.STOPPED);
        SceneManager.LoadScene("StartScreen", LoadSceneMode.Single);
    }

    private PLAYBACK_STATE GetPlaybackState()
    {
        AudioManager.Instance.MusicEventInstance.getPlaybackState(out var playbackState);
        return playbackState;
    }

    public void ClearSave()
    {
        SaveData.Instance.EraseData(!dataHolder.demoMode, true);
    }

    public void QuitGame()
    {
        Application.Quit();
        ButtonHandler.Instance.PlayBackSound();
    }
    
    public void LoadScene(string sceneName)
    {
        ButtonHandler.Instance.PlayConfirmSound();
        SceneManager.LoadScene(sceneName);
    }
}
