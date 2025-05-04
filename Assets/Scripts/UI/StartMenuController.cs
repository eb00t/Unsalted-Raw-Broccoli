using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
	[SerializeField] private EventSystem eventSystem;
	[SerializeField] private GameObject playBtn, controlsBtn, settingsBtn, quitBtn;
	[SerializeField] private GameObject controlGui, settingGui, menuGui;
	[SerializeField] private DataHolder dataHolder;
	
	private ControlsManager _controlsManager;

	private void Start()
	{
		SwitchSelected(playBtn);
		_controlsManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<ControlsManager>();

		if (dataHolder.isGamepad == false)
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}
	}

	private void FixedUpdate()
	{
		if (SceneManager.GetActiveScene().name == "EndScreen" || SceneManager.GetActiveScene().name == "creditsScene") return;
		
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
