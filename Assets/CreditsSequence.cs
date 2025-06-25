using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using FMOD.Studio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CreditsSequence : MonoBehaviour
{
    [SerializeField] private List<CanvasGroup> slides;
    [SerializeField] private CanvasGroup buttonsGroup;
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
        }

        InitialiseCreditsSequence();

        dataHolder.hasBeatenBaseGame = true;
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

                var slideFade = slides[i].GetComponent<RectTransform>().DOAnchorPosY(0f, 1f).SetEase(Ease.OutBounce).SetDelay(1.7f);
                
                creditsSeq.Append(showTitle);
                creditsSeq.Append(slideFade);
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
