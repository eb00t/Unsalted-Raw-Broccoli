using System;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class CheckControls : MonoBehaviour
{
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private GameObject xboximg, psimg, keyimg, keyTitle;
    
    private GameObject _startMenu;
    private bool _isGamepad;
    private GameObject _uiManager;
    private ControlsManager _controlsManager;
    private CanvasGroup _xboxCanvasGroup;
    private CanvasGroup _psCanvasGroup;

    private void Start()
    {
        _psCanvasGroup = psimg.GetComponent<CanvasGroup>();
        _xboxCanvasGroup = xboximg.GetComponent<CanvasGroup>();
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _controlsManager = _uiManager.GetComponent<ControlsManager>();
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;
        UpdateImg();
    }

    private void UpdateImg()
    {
        _controlsManager.CheckControl();
        switch (dataHolder.currentControl)
        {
            case ControlsManager.ControlScheme.Xbox:
                if (xboximg.activeSelf) return;
                xboximg.SetActive(true);
                psimg.SetActive(false);
                keyimg.SetActive(false);
                
                foreach (var t in xboximg.GetComponentsInChildren<Transform>())
                {
                    if (t.CompareTag("Animate"))
                    {
                        t.DOScale(new Vector3(1, 0, 1), 0.1f);
                    }
                }
                
                _xboxCanvasGroup.DOFade(1f, 0.1f).SetUpdate(true).OnComplete(() =>
                {
                    var xboxOpenSequence = DOTween.Sequence().SetUpdate(true);
                    foreach (var t in xboximg.GetComponentsInChildren<Transform>())
                    {
                        if (t.CompareTag("Animate"))
                        {
                            xboxOpenSequence.Join(t.DOScale(new Vector3(1, 1, 1), 0.1f));
                        }
                    }
                });
                break;
            case ControlsManager.ControlScheme.Playstation:
                if (psimg.activeSelf) return;
                
                xboximg.SetActive(false);
                psimg.SetActive(true);
                keyimg.SetActive(false);
                
                foreach (var t in psimg.GetComponentsInChildren<Transform>())
                {
                    if (t.CompareTag("Animate"))
                    {
                        t.DOScale(new Vector3(1, 0, 1), 0.1f);
                    }
                }
                
                _psCanvasGroup.DOFade(1f, 0.1f).SetUpdate(true).OnComplete(() =>
                {
                    var psOpenSequence = DOTween.Sequence().SetUpdate(true);
                    foreach (var t in psimg.GetComponentsInChildren<Transform>())
                    {
                        if (t.CompareTag("Animate"))
                        {
                            psOpenSequence.Join(t.DOScale(new Vector3(1, 1, 1), 0.1f));
                        }
                    }
                });
                break;
            case ControlsManager.ControlScheme.Keyboard:
                if (keyimg.activeSelf) return;
                xboximg.SetActive(false);
                psimg.SetActive(false);
                keyimg.SetActive(true);
                OpenKeyboardControls();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void CloseControls([CanBeNull] MenuHandler handler, [CanBeNull] StartMenuController startMenuController)
    {
        switch (dataHolder.currentControl)
        {
            case ControlsManager.ControlScheme.Xbox:
                var xboxCloseSeq = DOTween.Sequence().SetUpdate(true);
                
                foreach (var t in xboximg.GetComponentsInChildren<Transform>())
                {
                    if (t.CompareTag("Animate"))
                    {
                        xboxCloseSeq.Join(t.DOScale(new Vector3(1, 0, 1), 0.1f));
                    }
                }
                
                xboxCloseSeq.OnComplete(() =>
                {
                    _xboxCanvasGroup.DOFade(0f, 0.1f).SetUpdate(true).OnComplete(() =>
                    {
                        if (handler != null)
                        {
                            handler.OnControlsClosed();
                        }
                        else if (startMenuController != null)
                        {
                            startMenuController.OnControlsClosed();
                        }
                    });
                });
                break;
            case ControlsManager.ControlScheme.Playstation:
                var psCloseSeq = DOTween.Sequence().SetUpdate(true);
                
                foreach (var t in psimg.GetComponentsInChildren<Transform>())
                {
                    if (t.CompareTag("Animate"))
                    {
                        psCloseSeq.Join(t.DOScale(new Vector3(1, 0, 1), 0.1f));
                    }
                }
                
                psCloseSeq.OnComplete(() =>
                {
                    _psCanvasGroup.DOFade(0f, 0.1f).SetUpdate(true).OnComplete(() =>
                    {
                        if (handler != null)
                        {
                            handler.OnControlsClosed();
                        }
                        else if (startMenuController != null)
                        {
                            startMenuController.OnControlsClosed();
                        }
                    });
                });
                break;
            case ControlsManager.ControlScheme.Keyboard:
                var keyCloseSeq = DOTween.Sequence().SetUpdate(true);

                foreach (var t in keyimg.GetComponentInChildren<GridLayoutGroup>().GetComponentsInChildren<Transform>())
                {
                    if (t.CompareTag("Animate"))
                    {
                        keyCloseSeq.Join(t.DOScale(new Vector3(1, 0, 1), 0.1f));
                    }
                }
			
                keyCloseSeq.OnComplete(() =>
                {
                    var settingCloseSeq = DOTween.Sequence().SetUpdate(true);
                    settingCloseSeq.Append(keyTitle.transform.DOScale(new Vector3(1, 0, 1), 0.1f));
                    settingCloseSeq.Append(keyimg.transform.DOScale(new Vector3(1, 0, 1), 0.2f).SetEase(Ease.InBack));
				
                    settingCloseSeq.OnComplete(() =>
                    {
                        if (handler != null)
                        {
                            handler.OnControlsClosed();
                        }
                        else if (startMenuController != null)
                        {
                            startMenuController.OnControlsClosed();
                        }
                    });
                });
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OpenKeyboardControls()
    {
        keyimg.SetActive(true);
        keyimg.transform.localScale = new Vector3(0, 0.1f, 1);

        foreach (var t in keyimg.GetComponentInChildren<GridLayoutGroup>().GetComponentsInChildren<Transform>())
        {
            if (t.CompareTag("Animate"))
            {
                t.localScale = new Vector3(1, 0, 1);
            }
        }

        var keySeq = DOTween.Sequence().SetUpdate(true);
        keySeq.Append(keyimg.transform.DOScale(new Vector3(1, 0.1f, 1), 0.15f).SetEase(Ease.OutBack).SetUpdate(true));
        keySeq.Append(keyimg.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
        keyTitle.transform.localScale = new Vector3(1, 0, 1);
		
        keySeq.OnComplete(() =>
        {
            keyTitle.transform.DOScaleY(1f, 0.1f).SetUpdate(true);
			
            foreach (var t in keyimg.GetComponentInChildren<GridLayoutGroup>().GetComponentsInChildren<Transform>())
            {
                if (t.CompareTag("Animate"))
                {
                    t.DOScale(Vector3.one, 0.1f).SetUpdate(true);
                }
            }
        });
    }

    private void OnDisable()
    {
        keyimg.SetActive(false);
        xboximg.SetActive(false);
        psimg.SetActive(false);
    }
}
