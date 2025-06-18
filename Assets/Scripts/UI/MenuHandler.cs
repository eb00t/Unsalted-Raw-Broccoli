using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class MenuHandler : MonoBehaviour
{
	[Header("Code References")]
	[SerializeField] private CharacterMovement characterMovement;
	CharacterAttack _characterAttack;
	private ItemPickupHandler _itemPickupHandler;
	private InventoryStore _inventoryStore;
	private ToolbarHandler _toolbarHandler;
	private GameObject _player;
	private BlackoutManager _blackoutManager;
	private IDisposable _mEventListener;
	private DialogueHandler _dialogueHandler;
	
	[Header("UI References")]
	[SerializeField] private GameObject grid;
	[SerializeField] private GameObject invGui, invBck, invTitleText, invContent;
	[SerializeField] private GameObject toolbarGui;
	[SerializeField] private GameObject menuGui;
	[SerializeField] private GameObject quitPopupGui, quitBck;
	[SerializeField] private GameObject statsGui;
	[SerializeField] private GameObject infoGui;
	[SerializeField] private GameObject settingGui, settingBck, settingTitleText;
	[SerializeField] private GameObject controlGui;
	[SerializeField] private GameObject diedScreen;
	[SerializeField] private GameObject infoPopup;
	public GameObject dialogueGUI, dialogueBck, dialogueTitleText;
	[SerializeField] private GameObject settingsBtn, controlsBtn, quitBtn;
	[SerializeField] private GameObject slotsTooltip, inventoryTooltip;

	public GameObject mapCamera;
	
	[Header("Navigation")]
	[SerializeField] private EventSystem eventSystem;
	[SerializeField] private GameObject selectedMenu, selectedEquip, slots;
	private GameObject _lastSelected;

	public GameObject shopGUI, shopBck, shopInfo, shopTitleText, nextLevelTrigger, shopContent;
	[SerializeField] private DataHolder dataHolder;
	public ReadLore nearestLore;
	public NextLevelTrigger nearestLevelTrigger;
	public RechargeStationHandler rechargeStationHandler;
	[NonSerialized] public dialogueControllerScript dialogueController;
	private bool _distanceBasedDialogue;
	private CurrencyManager _currencyManager;
	[SerializeField] private GameObject hardcoreIndicator;

	[SerializeField] private float idleResetTime;
	private float _idleTimer;
	private CanvasGroup _hudCanvasGroup;

	private void Start()
	{
		_inventoryStore = GetComponent<InventoryStore>();
		_toolbarHandler = GetComponent<ToolbarHandler>();
		_currencyManager = GetComponent<CurrencyManager>();
		_player = GameObject.FindGameObjectWithTag("Player");
		_itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
		_characterAttack = _player.GetComponentInChildren<CharacterAttack>();
		_blackoutManager = GameObject.Find("Game Manager").GetComponentInChildren<BlackoutManager>();
		_dialogueHandler = GameObject.Find("Game Manager").GetComponent<DialogueHandler>();
		hardcoreIndicator.SetActive(dataHolder.hardcoreMode);
		_idleTimer = idleResetTime;
		_hudCanvasGroup = GetComponentInParent<CanvasGroup>();

		foreach (var t in dialogueGUI.GetComponentsInChildren<Transform>())
		{
			switch (t.name)
			{
				case "Text box":
					dialogueBck = t.gameObject;
					break;
				case "SpeakerHolder":
					dialogueTitleText = t.gameObject;
					break;
			}
		}
	}

	private void Update()
	{
		// resets to main menu if no inputs are made during idle reset time
		if (dataHolder.demoMode)
		{
			_idleTimer -= Time.unscaledDeltaTime;
			
			if (_idleTimer <= 0)
			{
				SceneManager.LoadScene("StartScreen", LoadSceneMode.Single);
			}
			
			// quick reset keybind for events
			if (Input.GetKey(KeyCode.F) && Input.GetKey(KeyCode.L) && Input.GetKeyDown(KeyCode.Alpha1))
			{
				SceneManager.LoadScene("StartScreen", LoadSceneMode.Single);
			}
		}
		
		// update if ui is open or not in player movement script
		var pauseGuisOpen = invGui.activeSelf ||
		                    menuGui.activeSelf ||
		                    quitPopupGui.activeSelf ||
		                    settingGui.activeSelf ||
		                    controlGui.activeSelf ||
		                    diedScreen.activeSelf ||
		                    infoPopup.activeSelf;
		var noPauseGuisOpen = (shopGUI != null && shopGUI.activeSelf) || (dialogueGUI != null && dialogueGUI.activeSelf) ||  (mapCamera != null && mapCamera.activeSelf);

		if (!_characterAttack.isDead)
		{
			characterMovement.uiOpen = pauseGuisOpen || noPauseGuisOpen;
			Time.timeScale = pauseGuisOpen ? 0 : 1;
		}
		else
		{
			characterMovement.uiOpen = true;
		}

		if (Time.timeScale == 0 && !_characterAttack.isDead)
		{
			AudioManager.Instance.SetGlobalEventParameter("NoMusicUIVolume", 0.1f);
			AudioManager.Instance.SetGlobalEventParameter("NotUI", 0.5f);
		}
		else if (Time.timeScale != 0 && !_characterAttack.isDead)
		{
			AudioManager.Instance.SetGlobalEventParameter("NoMusicUIVolume", 1f);
			AudioManager.Instance.SetGlobalEventParameter("NotUI", 1f);
		}

		if (dataHolder.currentControl == ControlsManager.ControlScheme.Keyboard)
		{
			var interactable = GetInteractable();
			if (interactable != null)
			{
				SwitchSelected(interactable);
			}

			if ((!Cursor.visible || Cursor.lockState == CursorLockMode.Locked) && characterMovement.uiOpen)
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}
			else if ((Cursor.visible || Cursor.lockState == CursorLockMode.None) && !characterMovement.uiOpen)
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
		else if (dataHolder.isGamepad && Cursor.visible || Cursor.lockState == CursorLockMode.None)
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}
		
		if (eventSystem.currentSelectedGameObject != _lastSelected)
		{
			if (dataHolder.currentControl == ControlsManager.ControlScheme.Keyboard) return;
			ButtonHandler.Instance.PlayNavigateSound();
			_lastSelected = eventSystem.currentSelectedGameObject;
		}
	}
	
	// checks if there is an interactable ui element below the mouse with a raycast
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

	// opens equip menu (with inventory), hides other menus and resets interaction bool to prevent unwanted ui navigation
	public void ToggleEquip()
	{
		if (!_blackoutManager.blackoutComplete) return;
		
		ButtonHandler.Instance.PlayConfirmSound();
		foreach (var s in slots.GetComponentsInChildren<Button>())
		{
			s.interactable = true;
		}
		
		foreach (var b in grid.GetComponentsInChildren<Button>())
		{
			b.interactable = false;
		}
		
		slotsTooltip.SetActive(true);
		inventoryTooltip.SetActive(false);
		invGui.SetActive(true);

		if (!infoGui.activeSelf)
		{
			invBck.transform.localScale = new Vector3(0, 0.1f, 1);

			var invOpenSeq = DOTween.Sequence().SetUpdate(true);
			invOpenSeq.Append(invBck.transform.DOScale(new Vector3(1, 0.1f, 1), 0.15f).SetEase(Ease.OutBack));
			invOpenSeq.Append(invBck.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
			invTitleText.transform.localScale = new Vector3(1, 0, 1);
			slots.transform.localScale  = new Vector3(0, 1, 1);
			
			foreach (var t in invContent.GetComponentsInChildren<Transform>())
			{
				if (t.CompareTag("Animate"))
				{
					t.localScale = new Vector3(1, 0, 1);
				}
			}

			invOpenSeq.OnComplete(() =>
			{
				invTitleText.transform.DOScaleY(1f, 0.1f).SetUpdate(true);
				slots.transform.DOScale(Vector3.one, 0.1f).SetUpdate(true);
				
				foreach (var t in invContent.GetComponentsInChildren<Transform>())
				{
					if (t.CompareTag("Animate"))
					{
						t.DOScale(Vector3.one, 0.1f).SetUpdate(true);
					}
				}
			});
		}

		infoGui.SetActive(false);
		_toolbarHandler.isInfoOpen = false;
		menuGui.SetActive(false);
		SwitchSelected(_toolbarHandler.slots[_toolbarHandler.slotNo]);
	}

	public void ToggleShop()
	{
		if (shopGUI == null) return;
		var shopHandler = shopGUI.GetComponentInParent<ShopHandler>();
		//if (!_player.GetComponent<ItemPickupHandler>().isPlrNearShop) return;
		
		ButtonHandler.Instance.PlayConfirmSound();

		_currencyManager.canvasGroup.DOFade(1, 0.5f);
		
		shopGUI.SetActive(true);
		
		shopBck.transform.localScale = new Vector3(0, 0.1f, 1);
		
		var shopOpenSeq = DOTween.Sequence().SetUpdate(true);
		shopOpenSeq.Append(shopBck.transform.DOScale(new Vector3(1, 0.1f, 1), 0.15f).SetEase(Ease.OutBack));
		shopOpenSeq.Append(shopBck.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
		shopTitleText.transform.localScale = new Vector3(1, 0, 1);
		shopInfo.transform.localScale = new Vector3(1, 0, 1);
		
		foreach (var t in shopContent.GetComponentsInChildren<Transform>())
		{
			if (t.CompareTag("Animate"))
			{
				t.localScale = new Vector3(1, 0, 1);
			}
		}
		
		shopOpenSeq.OnComplete(() =>
		{
			shopTitleText.transform.DOScaleY(1f, 0.1f).SetUpdate(true);
			shopInfo.transform.DOScale(Vector3.one, 0.1f).SetUpdate(true);
			
			foreach (var t in shopContent.GetComponentsInChildren<Transform>())
			{
				if (t.CompareTag("Animate"))
				{
					t.DOScale(Vector3.one, 0.1f).SetUpdate(true);
				}
			}
			
			StartCoroutine(DelayShopSwitch(shopHandler));
		});
	}
	
	private IEnumerator DelayShopSwitch(ShopHandler shopHandler)
	{
		yield return new WaitForSecondsRealtime(.35f);
		SwitchSelected(shopHandler.grid.GetComponentInChildren<Button>().gameObject);
	}

	public void NextLevelLoad(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		if (characterMovement.uiOpen) return;
		if (!_player.gameObject.GetComponent<ItemPickupHandler>().isPlrNearEnd) return;
		if (nearestLevelTrigger == null) return;
		
		nearestLevelTrigger.GetComponent<NextLevelTrigger>().LoadNextLevel();
	}
	
	private void OnEnable()
	{
		_mEventListener = InputSystem.onAnyButtonPress.Call(ResetIdleTimer);
	}

	private void ResetIdleTimer(InputControl button)
	{
		ResetIdleTime();
	}

	public void ResetIdleTime()
	{
		_idleTimer = idleResetTime;
	}
	
	private void OnDisable()
	{
		_mEventListener.Dispose();
	}

	public void CancelDialogue(InputAction.CallbackContext context)
	{
		if (!context.performed || !dialogueGUI.activeSelf) return;

		_dialogueHandler.StopAllCoroutines();
		_dialogueHandler.index = 0;
		_dialogueHandler.loadedBodyText.Clear();
		_dialogueHandler.loadedSpeakerText.Clear();
		_dialogueHandler._speakerText.text = "";
		_dialogueHandler._dialogueText.text = "";
		if (_dialogueHandler.flipped)
		{
			_dialogueHandler.flipped = false;
			_player.GetComponentInChildren<SpriteRenderer>().flipX = false;
		}
		
		SetDialogueActive(false);
	}

	// when Button East/Esc is pressed close current menu and open previous menus
	public void Back(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		if (!characterMovement.uiOpen) return;

		if (invGui.activeSelf && !_toolbarHandler.isInfoOpen)
		{
			
			var invContentSeq = DOTween.Sequence().SetUpdate(true);
			
			foreach (var t in invContent.GetComponentsInChildren<Transform>())
			{
				if (t.CompareTag("Animate"))
				{
					invContentSeq.Join(t.DOScale(new Vector3(1, 0, 1), 0.1f));
				}
			}

			invContentSeq.OnComplete(() =>
			{
				var invCloseSeq = DOTween.Sequence().SetUpdate(true);
			
				invCloseSeq.Append(slots.transform.DOScale(new Vector3(0, 1, 1), 0.1f));
				invCloseSeq.Append(invTitleText.transform.DOScale(new Vector3(1, 0, 1), 0.1f));
				invCloseSeq.Append(invBck.transform.DOScale(new Vector3(1, 0, 1), 0.2f).SetEase(Ease.InBack));
			
				invCloseSeq.OnComplete(() =>
				{
					ButtonHandler.Instance.PlayBackSound();
					foreach (var b in grid.GetComponentsInChildren<Button>())
					{
						b.interactable = false;
					}

					foreach (var s in slots.GetComponentsInChildren<Button>())
					{
						s.interactable = true;
					}

					invGui.SetActive(false);
					menuGui.SetActive(true);
					SwitchSelected(selectedMenu);
				});
			});
		}
		else if (invGui.activeSelf && _toolbarHandler.isInfoOpen)
		{
			infoGui.transform.DOScale(new Vector3(1, 0, 1), 0.1f).SetUpdate(true).OnComplete(() =>
			{
				ButtonHandler.Instance.PlayBackSound();
				foreach (var b in grid.GetComponentsInChildren<Button>())
				{
					b.interactable = false;
				}
				
				foreach (var s in slots.GetComponentsInChildren<Button>())
				{
					s.interactable = true;
				}
				
				slotsTooltip.SetActive(true);
				inventoryTooltip.SetActive(false);
				SwitchSelected(_toolbarHandler.slots[_toolbarHandler.slotNo]);
				infoGui.SetActive(false);
				_toolbarHandler.isInfoOpen = false;
			});
		}
		else if (menuGui.activeSelf)
		{
			ButtonHandler.Instance.PlayBackSound();
			menuGui.SetActive(false);
			toolbarGui.SetActive(true);
			statsGui.SetActive(true);
			SwitchSelected(null);
		}
		else if (shopGUI != null  && shopGUI.activeSelf)
		{
			var shopContentSeq = DOTween.Sequence().SetUpdate(true);
			
			foreach (var t in shopContent.GetComponentsInChildren<Transform>())
			{
				if (t.CompareTag("Animate"))
				{
					shopContentSeq.Join(t.DOScale(new Vector3(1, 0, 1), 0.1f));
				}
			}

			shopContentSeq.OnComplete(() =>
			{
				var shopCloseSeq = DOTween.Sequence().SetUpdate(true);
				shopCloseSeq.Append(shopTitleText.transform.DOScale(new Vector3(1, 0, 1), 0.1f));
				shopCloseSeq.Append(shopInfo.transform.DOScale(new Vector3(1, 0, 1), 0.1f));
				shopCloseSeq.Append(shopBck.transform.DOScale(new Vector3(1, 0, 1), 0.2f).SetEase(Ease.InBack));
				
				shopCloseSeq.OnComplete(() =>
				{
					shopGUI.SetActive(false);
					_currencyManager.canvasGroup.DOFade(0, 0.5f);
					SetDialogueActive(true);
					dialogueController.isEndText = true;
					dialogueController.LoadDialogue(dialogueController.dialogueToLoad);
				});
			});
		}
		else if (settingGui.activeSelf)
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
				settingCloseSeq.Append(settingTitleText.transform.DOScale(new Vector3(1, 0, 1), 0.1f));
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
		else if (quitPopupGui.activeSelf)
		{
			var quitCloseSeq = DOTween.Sequence().SetUpdate(true);
			
			foreach (var t in quitBck.GetComponentsInChildren<Transform>())
			{
				if (t.CompareTag("Animate"))
				{
					quitCloseSeq.Join(t.DOScale(new Vector3(1, 0, 1), 0.1f));
				}
			}

			quitCloseSeq.OnComplete(() =>
			{
				var quitSeq = DOTween.Sequence().SetUpdate(true);
				quitSeq.Append(quitBck.transform.DOScale(new Vector3(1, 0, 1), 0.2f).SetEase(Ease.InBack));

				quitSeq.OnComplete(() =>
				{
					ButtonHandler.Instance.PlayBackSound();
					quitPopupGui.SetActive(false);
					menuGui.SetActive(true);
					SwitchSelected(quitBtn);
				});
			});
		}
		else if (controlGui.activeSelf)
		{
			ButtonHandler.Instance.PlayBackSound();
			controlGui.SetActive(false);
			menuGui.SetActive(true);
			SwitchSelected(controlsBtn);
		}
		else if (infoPopup.activeSelf)
		{
			var infoSeq = DOTween.Sequence().SetUpdate(true);
			
			foreach (var t in infoPopup.GetComponentsInChildren<Transform>())
			{
				if (t.CompareTag("Animate"))
				{
					infoSeq.Join(t.DOScale(new Vector3(1, 0, 1), 0.1f));
				}
			}

			infoSeq.OnComplete(() =>
			{
				var infoCloseSeq = DOTween.Sequence().SetUpdate(true);
				infoCloseSeq.Append(infoPopup.transform.DOScale(new Vector3(1, 0, 1), 0.2f).SetEase(Ease.InBack));

				infoCloseSeq.OnComplete(() =>
				{
					infoPopup.SetActive(false);
					_itemPickupHandler.TogglePrompt("", false, ControlsManager.ButtonType.Back, "", null, false);
				});
			});
		}
	}

	public void DeathReload(InputAction.CallbackContext context)
	{
		if (!context.performed) return;

		if (diedScreen.activeSelf)
		{
			if (dataHolder.hardcoreMode)
			{
				SaveData.Instance.EraseData();
				SaveData.Instance.LoadSave();
			}

			SceneReload();
		}
	}
	
	public void EnableDialogueBox(InputAction.CallbackContext context)
	{
		if (!context.performed || characterMovement.uiOpen || (mapCamera != null && mapCamera.activeSelf)) return;
		TriggerDialogue(true, dialogueController);
	}

	public void TriggerDialogue(bool isDistanceBased, dialogueControllerScript controller)
	{
		if (characterMovement.uiOpen || (mapCamera != null && mapCamera.activeSelf)) return;
		if (_itemPickupHandler.isPlrNearDialogue && isDistanceBased)
		{
			SetDialogueActive(true);
			controller.LoadDialogue(controller.dialogueToLoad);
		}
		else if (!isDistanceBased)
		{
			SetDialogueActive(true);
			controller.LoadDialogue(controller.dialogueToLoad);
		}
	}

	public void ShowLore(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		
		if (_itemPickupHandler.isPlrNearLore && nearestLore.gameObject.activeSelf && !nearestLore.hasBeenRead)
		{
			SetDialogueActive(true);
			dialogueController.LoadLore(nearestLore.whatLore);
			Debug.Log(nearestLore.loreType);
		}
	}

	public void SetDialogueActive(bool isActive)
	{
		if (isActive)
		{
			dialogueGUI.SetActive(true);
			dialogueBck.transform.localScale = new Vector3(0, 0.1f, 1);
			dialogueTitleText.transform.localScale = new Vector3(1, 0, 1);
			
			var dialogueSeq = DOTween.Sequence().SetUpdate(true);
			dialogueSeq.Append(dialogueBck.transform.DOScale(new Vector3(1, 0.1f, 1), 0.15f).SetEase(Ease.OutBack));
			dialogueSeq.Append(dialogueBck.transform.DOScale(new Vector3(1, 1, 1), 0.25f).SetEase(Ease.OutBack));

			dialogueSeq.OnComplete(() =>
			{
				dialogueTitleText.transform.DOScaleY(1f, 0.1f).SetUpdate(true);
			});
		}
		else
		{
			var dialogueCloseSeq = DOTween.Sequence().SetUpdate(true);
			dialogueCloseSeq.Append(dialogueTitleText.transform.DOScale(new Vector3(1, 0, 1), 0.1f));
			dialogueCloseSeq.Append(dialogueBck.transform.DOScale(new Vector3(1, 0, 1), 0.2f).SetEase(Ease.InBack));
				
			dialogueCloseSeq.OnComplete(() =>
			{
				dialogueGUI.SetActive(false);
			});
		}
	}

	// toggle pause menu
	public void Pause(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		if (characterMovement.uiOpen && !menuGui.activeSelf) return;
		if (!_blackoutManager.blackoutComplete) return;
		
		menuGui.SetActive(!menuGui.activeSelf);
		
		if (menuGui.activeSelf)
		{
			SwitchSelected(selectedMenu);
			statsGui.SetActive(false);
			toolbarGui.SetActive(false);
		}
		else
		{
			SwitchSelected(null);
			statsGui.SetActive(true);
			toolbarGui.SetActive(true);
		}
	}

	// switches controller navigation to a new object
	public void SwitchSelected(GameObject g)
	{
		eventSystem.SetSelectedGameObject(null);
		eventSystem.SetSelectedGameObject(g);
		_lastSelected = g;
	}
	
	private void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus) return;
		SwitchSelected(_lastSelected);
	}

	// If player has items, switches navigation to inventory to add item to selected toolbar index when toolbar button is pressed
	public void SlotSelected(int slot)
	{
		_toolbarHandler.slotNo = slot;
		
		if (dataHolder.savedItems.Count > 0)
		{
			SwitchSelected(grid.GetComponentInChildren<Button>().gameObject);
			
			foreach (var b in grid.GetComponentsInChildren<Button>())
			{
				b.interactable = true;
			}
			
			foreach (var s in slots.GetComponentsInChildren<Button>())
			{
				s.interactable = false;
			}
			
			slotsTooltip.SetActive(false);
			inventoryTooltip.SetActive(true);
			infoGui.SetActive(true);
			infoGui.transform.localScale = new Vector3(1, 0, 1);
			infoGui.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
			_toolbarHandler.isInfoOpen = true;
		}
		else
		{
			// no items held popup
			_inventoryStore.TriggerNotification(null, "No items held in inventory", false);
		}
	}

	public void EnergyPurchased(InputAction.CallbackContext context)
	{
		if (!context.performed || characterMovement.uiOpen) return;
		if (!_player.gameObject.GetComponent<ItemPickupHandler>().isPlayerNearRecharge) return;
		if (rechargeStationHandler == null) return;
		if (rechargeStationHandler.hasBeenPurchased) return;
		
		if (dataHolder.currencyHeld - rechargeStationHandler.cost >= 0)
		{
			_currencyManager.UpdateCurrency(-rechargeStationHandler.cost);
			rechargeStationHandler.InstantiateEnergy();
		}
		else
		{
			_inventoryStore.TriggerNotification(null, "Not enough robot coils held.", false);
		}
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
		settingSequence.Append(settingBck.transform.DOScale(new Vector3(1, 0.1f, 1), 0.15f).SetEase(Ease.OutBack).SetUpdate(true));
		settingSequence.Append(settingBck.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
		settingTitleText.transform.localScale = new Vector3(1, 0, 1);
		
		settingSequence.OnComplete(() =>
		{
			settingTitleText.transform.DOScaleY(1f, 0.1f).SetUpdate(true);
			
			foreach (var t in settingBck.GetComponentInChildren<GridLayoutGroup>().GetComponentsInChildren<Transform>())
			{
				if (t.CompareTag("Animate"))
				{
					t.DOScale(Vector3.one, 0.1f).SetUpdate(true);
				}
			}
		});
	}

	public void OpenQuit()
	{
		quitPopupGui.SetActive(true);
		quitBck.transform.localScale = new Vector3(0, 0.1f, 1);
		
		foreach (var t in quitBck.GetComponentsInChildren<Transform>())
		{
			if (t.CompareTag("Animate"))
			{
				t.localScale = new Vector3(1, 0, 1);
			}
		}
		
		var quitSeq = DOTween.Sequence().SetUpdate(true);
		quitSeq.Append(quitBck.transform.DOScale(new Vector3(1, 0.1f, 1), 0.15f).SetEase(Ease.OutBack).SetUpdate(true));
		quitSeq.Append(quitBck.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));

		quitSeq.OnComplete(() =>
		{
			foreach (var t in quitBck.GetComponentsInChildren<Transform>())
			{
				if (t.CompareTag("Animate"))
				{
					t.DOScale(Vector3.one, 0.1f).SetUpdate(true);
				}
			}
		});
	}

	public void HoldOpenMap(InputAction.CallbackContext context)
	{
		if (characterMovement.uiOpen && !mapCamera.activeSelf) return;
		
		if (context.started) // input started (i.e when the button is held)
		{
			mapCamera.SetActive(true);
			_hudCanvasGroup.DOFade(0, 0.2f);
		}
		
		if (context.canceled) // when input ends (i.e. when the button is let go)
		{
			mapCamera.SetActive(false);
			_hudCanvasGroup.DOFade(1, 0.2f);
		}
	}

	public void SceneReload() // reloads scene
	{
		ButtonHandler.Instance.PlayBackSound();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void LoadScene(string scene)
	{
		SceneManager.LoadScene(scene);
	}

	public void Quit() // quits game
	{
		ButtonHandler.Instance.PlayBackSound();
		Application.Quit();
	}
}
