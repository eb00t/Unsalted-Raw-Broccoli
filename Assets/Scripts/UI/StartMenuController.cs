using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuController : MonoBehaviour
{
	[SerializeField] private EventSystem eventSystem;
	[SerializeField] private GameObject playBtn, demoPlayButton, defaultPlayButton, controlsBtn, settingsBtn, quitBtn;

	[SerializeField] private GameObject controlGui, settingGui, menuGui, blackout, load;
	[SerializeField] private Image blackoutImg, loadingImg1, loadingImg2, vignette;
	[SerializeField] private DataHolder dataHolder;

	public Color blackoutColor, loadImgColor, loadImg2Color;
	public Color vignetteColor;
	public Color transparentColor;
	
	private ControlsManager _controlsManager;
	[SerializeField] private bool isCredits;
	public bool creditsFinished;
	private float _lerpTime = 0f;

	private void Start()
	{
		if (dataHolder.demoMode)
		{
			playBtn = demoPlayButton;
		}
		else
		{
			playBtn = defaultPlayButton;
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

		if (blackout.activeSelf)
		{
			_lerpTime += Time.deltaTime;
			blackoutImg.color = Color.Lerp(transparentColor, blackoutColor, _lerpTime);
			vignette.color = Color.Lerp(transparentColor, vignetteColor, _lerpTime);
			loadingImg1.color = Color.Lerp(transparentColor, loadImgColor, _lerpTime);
			loadingImg2.color = Color.Lerp(transparentColor, loadImg2Color, _lerpTime);
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
			StartCoroutine(WaitToLoad(sceneName));
			return;
		}

		ButtonHandler.Instance.PlayConfirmSound();
		SceneManager.LoadScene(sceneName);
	}

	private IEnumerator WaitToLoad(string sceneName)
	{
		yield return new WaitForSeconds(3f);
		ButtonHandler.Instance.PlayConfirmSound();
		SceneManager.LoadScene(sceneName);
	}

	public void Back(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		
		if (settingGui.activeSelf)
		{
			settingGui.SetActive(false);
			menuGui.SetActive(true);
			SwitchSelected(settingsBtn);
		}
		else if (controlGui.activeSelf)
		{
			controlGui.SetActive(false);
			menuGui.SetActive(true);
			SwitchSelected(controlsBtn);
		}
	}

	public void FadeInLoadingScreen()
	{
		blackout.SetActive(true);
		load.SetActive(true);
	}

	public void WipeData()
	{
		dataHolder.currencyHeld = 0;
		dataHolder.currentLevel = LevelBuilder.LevelMode.Floor1;
		dataHolder.highestFloorCleared = 0;
		dataHolder.savedItems.Clear();
		dataHolder.savedItemCounts.Clear();
		dataHolder.equippedConsumables = new int[5];
		dataHolder.currencyHeld = 0;
		dataHolder.permanentPassiveItems = new int[4];
	}
}
