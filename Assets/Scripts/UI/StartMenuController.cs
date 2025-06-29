using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuController : MonoBehaviour
{
	[SerializeField] private EventSystem eventSystem;

	[SerializeField] private GameObject playBtn,
		demoPlayButton,
		defaultPlayButton,
		controlsBtn,
		settingsBtn,
		quitBtn,
		settingBck,
		settingTitle;

	[SerializeField] private TextMeshProUGUI newGameText, notifyText;

	[SerializeField] private GameObject controlGui, settingGui, menuGui, blackout, load, lockedBck, lockedImg;
	[SerializeField] private Image blackoutImg, loadingImg1, loadingImg2, vignette, newIconImg;
	[SerializeField] private DataHolder dataHolder;
	[SerializeField] private GameObject newGamePopupBtn, newGameBck, newGamePopup, newGameBtn;
	[SerializeField] private GameObject hardModePopupBtn, hardModeBck, hardModePopup, hardModeBtn;
	private string _sceneToLoad;

	private ControlsManager _controlsManager;
	[SerializeField] private bool isCredits;
	public bool creditsFinished;
	[SerializeField] private Sprite playSprite;
	[SerializeField] private CanvasGroup loadingGroup;
	private Tween _loadTween;
	private Tween _shakeTween;
	private Sequence _notifySequence;

	private void Start()
	{
		if (dataHolder.demoMode && SceneManager.GetActiveScene().name == "StartScreen")
		{
			playBtn = demoPlayButton;
			defaultPlayButton.SetActive(false);
			quitBtn.SetActive(false);
			newIconImg.sprite = playSprite;
			newGameText.text = "Play";
			lockedBck.SetActive(false);
			lockedImg.SetActive(false);
		}
		else if (dataHolder.demoMode && SceneManager.GetActiveScene().name == "creditsScene")
		{
			quitBtn.SetActive(false);
		}
		else
		{
			SaveData.Instance.LoadSave();
			Debug.Log(SaveData.Instance.CheckIfSaveFileExists());
			if (dataHolder.hasPlayedThisSession || SaveData.Instance.CheckIfSaveFileExists())
			{
				demoPlayButton.SetActive(true);
			}
			else
			{
				demoPlayButton.SetActive(false);
			}

			playBtn = defaultPlayButton;
		}

		if (dataHolder.hasBeatenBaseGame)
		{
			lockedBck.SetActive(false);
			lockedImg.SetActive(false);
		}

		SwitchSelected(playBtn);
		_controlsManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<ControlsManager>();

		if (isCredits && dataHolder.demoMode)
		{
			StartCoroutine(ResetAfterDelay());
		}
	}

	private void Update()
	{
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

	private IEnumerator ResetAfterDelay()
	{
		yield return new WaitUntil(() => creditsFinished);
		yield return new WaitForSeconds(5f);

		SceneManager.LoadScene("StartScreen", LoadSceneMode.Single);
	}

	private void FixedUpdate()
	{
		if (SceneManager.GetActiveScene().name == "EndScreen") return;

		_controlsManager.CheckControl();
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus) return;
		SwitchSelected(playBtn);
	}

	public void ToggleHardcoreMode(bool isOn)
	{
		dataHolder.hardcoreMode = isOn;
	}

	public void QuitGame()
	{
		Application.Quit();
		ButtonHandler.Instance.PlayBackSound();
	}

	public void SwitchSelected(GameObject g)
	{
		eventSystem.SetSelectedGameObject(null);
		eventSystem.SetSelectedGameObject(g);
	}

	public void LoadScene(string sceneName)
	{
		if (SceneManager.GetActiveScene().name == "StartScreen")
		{
			_sceneToLoad = sceneName;
			return;
		}

		ButtonHandler.Instance.PlayConfirmSound();
		SceneManager.LoadScene(sceneName);
	}

	public void LoadHardMode(bool noPopup)
	{
		if (noPopup || dataHolder.demoMode)
		{
			if (!dataHolder.demoMode)
			{
				PopupClose(hardModePopup);
			}

			FadeInLoadingScreen();
			ToggleHardcoreMode(true);
			
			SaveData.Instance.EraseData(!dataHolder.demoMode, true);

			LoadScene("Intermission");
		}
		else if (dataHolder.hasBeatenBaseGame || dataHolder.demoMode)
		{
			SwitchSelected(hardModePopupBtn);
			
			hardModePopup.SetActive(true);
			hardModeBck.transform.localScale = new Vector3(0, 0.1f, 1);
		
			foreach (var t in hardModeBck.GetComponentsInChildren<Transform>())
			{
				if (t.CompareTag("Animate"))
				{
					t.localScale = new Vector3(1, 0, 1);
				}
			}
		
			var quitSeq = DOTween.Sequence().SetUpdate(true);
			quitSeq.Append(hardModeBck.transform.DOScale(new Vector3(1, 0.1f, 1), 0.15f).SetEase(Ease.OutBack).SetUpdate(true));
			quitSeq.Append(hardModeBck.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));

			quitSeq.OnComplete(() =>
			{
				foreach (var t in hardModeBck.GetComponentsInChildren<Transform>())
				{
					if (t.CompareTag("Animate"))
					{
						t.DOScale(Vector3.one, 0.1f).SetUpdate(true);
					}
				}
			});
		}
		else
		{
			notifyText.text = "unlocked after completing the base game";
			_shakeTween?.Kill(true);
			_shakeTween = lockedImg.transform.DOShakePosition(0.3f, new Vector3(10, 0, 0), 20, 0, false, true, ShakeRandomnessMode.Harmonic);

			_notifySequence?.Kill();
			_notifySequence = DOTween.Sequence().SetUpdate(true);
			_notifySequence.Append(notifyText.DOFade(1f, 0.25f));
			_notifySequence.Append(notifyText.DOFade(0f, 0.25f).SetDelay(1f));
		}
	}

	public void Back(InputAction.CallbackContext context)
	{
		if (!context.performed) return;

		if (settingGui.activeSelf)
		{
			var settingsSequence = DOTween.Sequence().SetUpdate(true);

			foreach (var t in settingBck.GetComponentInChildren<GridLayoutGroup>().GetComponentsInChildren<Transform>())
			{
				if (t.CompareTag("Animate"))
				{
					settingsSequence.Join(t.DOScale(new Vector3(1, 0, 1), 0.1f));
				}
			}

			settingsSequence.OnComplete(() =>
			{
				var settingCloseSeq = DOTween.Sequence().SetUpdate(true);
				settingCloseSeq.Append(settingTitle.transform.DOScale(new Vector3(1, 0, 1), 0.1f));
				settingCloseSeq.Append(settingBck.transform.DOScale(new Vector3(1, 0, 1), 0.2f).SetEase(Ease.InBack));

				settingCloseSeq.OnComplete(() =>
				{
					ButtonHandler.Instance.PlayBackSound();
					settingGui.SetActive(false);
					menuGui.SetActive(true);
					SwitchSelected(settingsBtn);
				});
			});
		}
		else if (controlGui.activeSelf)
		{
			controlGui.GetComponent<CheckControls>().CloseControls(null, this);
		}
		else if (newGamePopup.activeSelf)
		{
			PopupClose(newGamePopup);
		}
		else if (hardModePopup.activeSelf)
		{
			PopupClose(hardModePopup);
		}
	}

	public void OnControlsClosed()
	{
		ButtonHandler.Instance.PlayBackSound();
		controlGui.SetActive(false);
		menuGui.SetActive(true);
		SwitchSelected(controlsBtn);
	}

	public void OpenSettings()
	{
		settingGui.SetActive(true);
		settingBck.transform.localScale = new Vector3(0, 0.1f, 1);

		foreach (var t in settingBck.GetComponentInChildren<GridLayoutGroup>().GetComponentsInChildren<Transform>())
		{
			if (t.CompareTag("Animate"))
			{
				t.localScale = new Vector3(1, 0, 1);
			}
		}

		var settingSequence = DOTween.Sequence().SetUpdate(true);
		settingSequence.Append(settingBck.transform.DOScale(new Vector3(1, 0.1f, 1), 0.15f).SetEase(Ease.OutBack)
			.SetUpdate(true));
		settingSequence.Append(settingBck.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
		settingTitle.transform.localScale = new Vector3(1, 0, 1);

		settingSequence.OnComplete(() =>
		{
			settingTitle.transform.DOScaleY(1f, 0.1f).SetUpdate(true);

			foreach (var t in settingBck.GetComponentInChildren<GridLayoutGroup>().GetComponentsInChildren<Transform>())
			{
				if (t.CompareTag("Animate"))
				{
					t.DOScale(Vector3.one, 0.1f).SetUpdate(true);
				}
			}
		});
	}

	public void NewGame(bool noPopup)
	{
		if (dataHolder.demoMode || noPopup)
		{
			if (!dataHolder.demoMode)
			{
				PopupClose(newGamePopup);
			}
			else
			{
				HorseFacts.Instance.ResetReadHorseFacts();
			}

			FadeInLoadingScreen();
			ToggleHardcoreMode(false);
			SaveData.Instance.EraseData(!dataHolder.demoMode, true);
			dataHolder.eraseViewedLore = true;
			LoadScene("Intermission");
		}
		else
		{
			SwitchSelected(newGamePopupBtn);
			
			newGamePopup.SetActive(true);
			newGameBck.transform.localScale = new Vector3(0, 0.1f, 1);
		
			foreach (var t in newGameBck.GetComponentsInChildren<Transform>())
			{
				if (t.CompareTag("Animate"))
				{
					t.localScale = new Vector3(1, 0, 1);
				}
			}
		
			var quitSeq = DOTween.Sequence().SetUpdate(true);
			quitSeq.Append(newGameBck.transform.DOScale(new Vector3(1, 0.1f, 1), 0.15f).SetEase(Ease.OutBack).SetUpdate(true));
			quitSeq.Append(newGameBck.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));

			quitSeq.OnComplete(() =>
			{
				foreach (var t in newGameBck.GetComponentsInChildren<Transform>())
				{
					if (t.CompareTag("Animate"))
					{
						t.DOScale(Vector3.one, 0.1f).SetUpdate(true);
					}
				}
			});
		}
	}

	public void PopupClose(GameObject bck)
	{
		var quitCloseSeq = DOTween.Sequence().SetUpdate(true);
		GameObject popupBck = null;

		foreach (var rect in bck.GetComponentsInChildren<RectTransform>())
		{
			if (rect.name == "Popup")
			{
				popupBck = rect.gameObject;
			}
		}
			
		foreach (var t in bck.GetComponentsInChildren<Transform>())
		{
			if (t.CompareTag("Animate"))
			{
				quitCloseSeq.Join(t.DOScale(new Vector3(1, 0, 1), 0.1f));
			}
		}

		quitCloseSeq.OnComplete(() =>
		{
			var quitSeq = DOTween.Sequence().SetUpdate(true);
			quitSeq.Append(popupBck.transform.DOScale(new Vector3(1, 0, 1), 0.2f).SetEase(Ease.InBack));

			quitSeq.OnComplete(() =>
			{
				ButtonHandler.Instance.PlayBackSound();
				bck.GetComponentInParent<RectTransform>().gameObject.SetActive(false);
				if (bck == hardModeBck)
				{
					SwitchSelected(hardModeBtn);
				}
				else if (bck == newGameBck)
				{
					SwitchSelected(newGameBtn);
				}
				else
				{
					SwitchSelected(playBtn);
				}
			});
		});
	}

	public void FadeInLoadingScreen()
	{
		if (!blackout || !load || !loadingGroup) return;

		blackout.SetActive(true);
		load.SetActive(true);

		loadingGroup.alpha = 0f;
		loadingGroup.gameObject.SetActive(true);

		_loadTween?.Kill();
		_loadTween = loadingGroup.DOFade(1f, 1.5f).OnComplete(() =>
		{
			load.SetActive(false);
			ButtonHandler.Instance.PlayConfirmSound();
			SceneManager.LoadScene(_sceneToLoad);
		});
	}
}
