using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

		if (dataHolder.currentControl == ControlsManager.ControlScheme.Keyboard)
		{
			var interactable = GetInteractable();
			if (interactable != null)
			{
				SwitchSelected(interactable);
			}
		}

		if (dataHolder.isGamepad == false)
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
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
