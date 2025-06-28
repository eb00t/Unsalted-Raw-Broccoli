using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Serialization;

public class MenuHandler : MonoBehaviour
{
	public static MenuHandler Instance { get; private set; }
	
	[Header("Code References")]
	[SerializeField] private CharacterMovement characterMovement;
	private CharacterAttack _characterAttack;
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
	[SerializeField] private GameObject menuGui, pauseTitle;
	[SerializeField] private GameObject quitPopupGui, quitBck;
	[SerializeField] private GameObject statsGui;
	[SerializeField] private GameObject infoGui;
	[SerializeField] private GameObject settingGui, settingBck, settingTitleText;
	[SerializeField] private GameObject controlGui;
	[SerializeField] private GameObject diedScreen;
	[SerializeField] private GameObject infoPopup;
	public GameObject dialogueGUI, dialogueBck, dialogueTitleText, dialogueBodyText;
	[SerializeField] private GameObject settingsBtn, controlsBtn, quitBtn;
	[SerializeField] private GameObject slotsTooltip, inventoryTooltip;
	[SerializeField] private CanvasGroup mapTxtGroup;
	private TextMeshProUGUI _mapTxt;
	[SerializeField] private GameObject loreCanvas;
	private LoreGUIManager _loreGUIManager;

	public GameObject mapCamera;
	
	[Header("Navigation")]
	public EventSystem eventSystem;
	[SerializeField] private GameObject selectedMenu, selectedEquip, slots;
	private GameObject _lastSelected;

	public GameObject shopGUI, shopBck, shopInfo, shopTitleText, nextLevelTrigger, shopContent;
	[SerializeField] private DataHolder dataHolder;
	public ReadLore nearestLore;
	public NextLevelTrigger nearestLevelTrigger;
	public RechargeStationHandler rechargeStationHandler;
	[NonSerialized] public DialogueControllerScript dialogueController;
	private bool _distanceBasedDialogue;
	private CurrencyManager _currencyManager;
	[SerializeField] private GameObject hardcoreIndicator;

	[SerializeField] private float idleResetTime;
	private float _idleTimer;
	private CanvasGroup _hudCanvasGroup;
	public CanvasGroup currencyCanvasGroup;

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one MenuHandler script in the scene.");
		}

		Instance = this;
	}

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
		_mapTxt = mapTxtGroup.GetComponent<TextMeshProUGUI>();
		if (loreCanvas != null) _loreGUIManager = loreCanvas.GetComponentInParent<LoreGUIManager>(); 

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
				case "Normal Text":
					dialogueBodyText = t.gameObject;
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
		var noPauseGuisOpen = (shopGUI != null && shopGUI.activeSelf) ||
		                      (dialogueGUI != null && dialogueGUI.activeSelf) ||
		                      (mapCamera != null && mapCamera.activeSelf) ||
		                      _blackoutManager != null && !_blackoutManager.blackoutComplete ||
			                      (loreCanvas != null && loreCanvas.activeSelf);
		                      

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
				currencyCanvasGroup.DOFade(1f, 0.5f).SetUpdate(true);
				
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
		if (!context.performed || characterMovement.uiOpen) return;
		if (nearestLevelTrigger == null) return;
		if (!nearestLevelTrigger.inRange) return;
		
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

		SetDialogueActive(false);
		_dialogueHandler.StopAllCoroutines();
		_dialogueHandler.index = 0;
		_dialogueHandler.loadedBodyText.Clear();
		_dialogueHandler.loadedSpeakerText.Clear();
		_dialogueHandler._speakerText.text = "";
		_dialogueHandler._dialogueText.text = "";

		if (_dialogueHandler._dialogueController != null && _dialogueHandler._dialogueController.hasBeforeAfterDialogue)
		{
			_dialogueHandler._dialogueController.isEndText = false;
			_dialogueHandler._dialogueController.LoadDialogue(_dialogueHandler._dialogueController.dialogueToLoad);
		}

		if (_dialogueHandler.flipped)
		{
			_dialogueHandler.flipped = false;
			_player.GetComponentInChildren<SpriteRenderer>().flipX = false;
		}
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
				invCloseSeq.Append(currencyCanvasGroup.DOFade(0f, 0.1f).SetUpdate(true));
			
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
			SwitchSelected(null);

			pauseTitle.transform.DOScale(new Vector3(1, 0, 1), 0.1f).SetUpdate(true).OnComplete(() =>
			{
				var pauseCloseSeq = DOTween.Sequence().SetUpdate(true);

				foreach (var t in menuGui.GetComponentsInChildren<Transform>())
				{
					if (t.CompareTag("Animate"))
					{
						pauseCloseSeq.Join(t.DOScale(new Vector3(1, 0, 1), 0.1f));
					}
				}

				pauseCloseSeq.OnComplete(() =>
				{
					ButtonHandler.Instance.PlayBackSound();
					menuGui.SetActive(false);
					statsGui.SetActive(true);
					toolbarGui.SetActive(true);
				});
			});
		}
		else if (shopGUI != null  && shopGUI.activeSelf)
		{
			var shopContentSeq = DOTween.Sequence().SetUpdate(true);
			SwitchSelected(null);
			
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
			controlGui.GetComponent<CheckControls>().CloseControls(this, null);
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
		else if (loreCanvas != null && loreCanvas.activeSelf)
		{
			if (_loreGUIManager.loreButtonHolder.GetComponentInChildren<Button>().interactable == false)
			{
				// switch to buttons
				foreach (var button in _loreGUIManager.loreButtonHolder.GetComponentsInChildren<Button>())
				{
					button.interactable = true;
				}
				
				foreach (var button in _loreGUIManager.loreLineHolder.GetComponentsInChildren<Button>())
				{
					button.interactable = false;
				}
				
				SwitchSelected(_loreGUIManager.loreButtonHolder.GetComponentInChildren<Button>().gameObject);
			}
			else
			{
				CloseLore();
			}
		}
	}

	public void OnControlsClosed()
	{
		ButtonHandler.Instance.PlayBackSound();
		controlGui.SetActive(false);
		menuGui.SetActive(true);
		SwitchSelected(controlsBtn);
	}
	
	public void PickUpItem(InputAction.CallbackContext context)
	{
		if (!context.performed || characterMovement.uiOpen) return;
         
		foreach (var item in GameObject.FindGameObjectsWithTag("Item"))
		{
			var itemPickup = item.GetComponent<ItemPickup>();
			if (itemPickup == null || !itemPickup.canPickup) continue;
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ItemPickup, transform.position);
			itemPickup.AddItemToInventory();
		}
	}

	public void DeathReload(InputAction.CallbackContext context)
	{
		if (!context.performed) return;

		if (diedScreen.activeSelf)
		{
			if (dataHolder.hardcoreMode)
			{
				SaveData.Instance.EraseData(true, dataHolder.demoMode);
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

	public void TriggerDialogue(bool isDistanceBased, DialogueControllerScript controller)
	{
		if (characterMovement.uiOpen || (mapCamera != null && mapCamera.activeSelf)) return;
		
		if (dialogueController.inRange && isDistanceBased)
		{
			SetDialogueActive(true);

			if (dialogueController.dialogueOrLore == DialogueControllerScript.DialogueOrLore.Dialogue)
			{
				controller.LoadDialogue(controller.dialogueToLoad);
			}
		}
		else if (!isDistanceBased)
		{
			SetDialogueActive(true);
			
			if (dialogueController.dialogueOrLore == DialogueControllerScript.DialogueOrLore.Dialogue)
			{
				controller.LoadDialogue(controller.dialogueToLoad);
			}
		}
	}

	public void ShowLore(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		
		if (nearestLore != null && nearestLore.inRange && nearestLore.gameObject.activeSelf && !nearestLore.hasBeenRead)
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
			dialogueBodyText.transform.localScale = new Vector3(1, 1, 1);
			
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
			dialogueBodyText.transform.localScale = new Vector3(1, 0, 1);
			var dialogueCloseSeq = DOTween.Sequence().SetUpdate(true);
			dialogueCloseSeq.Append(dialogueTitleText.transform.DOScale(new Vector3(1, 0, 1), 0.1f));
			dialogueCloseSeq.Append(dialogueBck.transform.DOScale(new Vector3(1, 0, 1), 0.2f).SetEase(Ease.InBack));
				
			dialogueCloseSeq.OnComplete(() =>
			{
				dialogueGUI.SetActive(false);
				if (dialogueController != null)
				{
					dialogueController.inRange = false;
				}
			});
		}
	}

	// toggle pause menu
	public void Pause(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		if (characterMovement.uiOpen && !menuGui.activeSelf) return;
		if (!_blackoutManager.blackoutComplete) return;
		
		if (!menuGui.activeSelf)
		{
			menuGui.SetActive(true);
			var pauseOpenSeq = DOTween.Sequence().SetUpdate(true);
			statsGui.SetActive(false);
			toolbarGui.SetActive(false);
			pauseTitle.transform.localScale = new Vector3(1, 0, 1);
			
			foreach (var t in menuGui.GetComponentsInChildren<Transform>())
			{
				if (t.CompareTag("Animate"))
				{
					t.localScale = new Vector3(1, 0, 1);
				}
			}
			
			foreach (var t in menuGui.GetComponentsInChildren<Transform>())
			{
				if (t.CompareTag("Animate"))
				{
					pauseOpenSeq.Join(t.DOScale(new Vector3(1, 1, 1), 0.1f));
				}
			}

			pauseOpenSeq.OnComplete(() =>
			{
				pauseTitle.transform.DOScale(Vector3.one, 0.1f).SetUpdate(true);
				SwitchSelected(selectedMenu);
			});
		}
		else if (menuGui.activeSelf)
		{
			SwitchSelected(null);
			
			pauseTitle.transform.DOScale(new Vector3(1, 0, 1), 0.1f).SetUpdate(true).OnComplete(() =>
			{
				var pauseCloseSeq = DOTween.Sequence().SetUpdate(true);
				
				foreach (var t in menuGui.GetComponentsInChildren<Transform>())
				{
					if (t.CompareTag("Animate"))
					{
						pauseCloseSeq.Join(t.DOScale(new Vector3(1, 0, 1), 0.1f));
					}
				}

				pauseCloseSeq.OnComplete(() =>
				{
					ButtonHandler.Instance.PlayBackSound();
					menuGui.SetActive(false);
					statsGui.SetActive(true);
					toolbarGui.SetActive(true);
				});
			});
		}
	}

	public void OpenLore()
	{
		if (loreCanvas != null)
		{
			_hudCanvasGroup.DOFade(0f, 0.25f).SetUpdate(true);
			loreCanvas.SetActive(true);
			menuGui.SetActive(false);
			_loreGUIManager.loreBck.transform.localScale = new Vector3(1, 0, 1);

			foreach (var t in loreCanvas.GetComponentsInChildren<Transform>())
			{
				if (t.CompareTag("Animate"))
				{
					t.localScale = new Vector3(1, 0, 1);
				}
			}

			var loreSeq = DOTween.Sequence().SetUpdate(true);
			foreach (var t in loreCanvas.GetComponentsInChildren<Transform>())
			{
				if (t.CompareTag("Animate"))
				{
					loreSeq.Join(t.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
				}
			}
			
			loreSeq.OnComplete(() =>
			{
				_loreGUIManager.loreBck.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
				SwitchSelected(_loreGUIManager.loreButtonHolder.GetComponentInChildren<Button>().gameObject);
			});
		}
	}

	public void CloseLore()
	{
		if (loreCanvas != null)
		{
			_loreGUIManager.loreBck.transform.DOScale(new Vector3(1, 0, 1), 0.25f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
			{
				var loreSeq2 = DOTween.Sequence().SetUpdate(true);
		
				foreach (var t in loreCanvas.GetComponentsInChildren<Transform>())
				{
					if (t.CompareTag("Animate"))
					{
						loreSeq2.Join(t.DOScale(new Vector3(1, 0, 1), 0.25f).SetEase(Ease.InBack));
					}
				}

				loreSeq2.OnComplete(() =>
				{
					_hudCanvasGroup.DOFade(1f, 0.25f).SetUpdate(true);
					loreCanvas.SetActive(false);
					SwitchSelected(null);
					
					SetDialogueActive(true);
					dialogueController.isEndText = true;
					dialogueController.LoadDialogue(dialogueController.dialogueToLoad);
				});
			});
		}
	}

	public void LoreButtonPressed() // switch to individual lines of dialogue
	{
		if (loreCanvas != null)
		{
			foreach (var button in _loreGUIManager.loreButtonHolder.GetComponentsInChildren<Button>())
			{
				button.interactable = false;
			}
			
			foreach (var button in _loreGUIManager.loreLineHolder.GetComponentsInChildren<Button>())
			{
				button.interactable = true;
			}
			
			SwitchSelected(_loreGUIManager.loreLineHolder.GetComponentInChildren<Button>().gameObject);
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
			_inventoryStore.TriggerNotification(null, "No items held in inventory", false, 2f);
		}
	}

	public void EnergyPurchased(InputAction.CallbackContext context)
	{
		if (!context.performed || characterMovement.uiOpen) return;
		if (rechargeStationHandler == null) return;
		if (!rechargeStationHandler.inRange) return;
		if (rechargeStationHandler.hasBeenPurchased) return;
		
		if (dataHolder.currencyHeld - rechargeStationHandler.cost >= 0)
		{
			_currencyManager.UpdateCurrency(-rechargeStationHandler.cost);
			rechargeStationHandler.InstantiateEnergy();
		}
		else
		{
			_inventoryStore.TriggerNotification(null, "Not enough robot coils held.", false, 2f);
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
			_hudCanvasGroup.DOFade(0, 0.2f).SetUpdate(true);
			
			_mapTxt.text = ReturnCurrentFloorOrSceneAsString();
			mapTxtGroup.DOFade(1f, 0.2f);
		}
		
		if (context.canceled) // when input ends (i.e. when the button is let go)
		{
			mapCamera.SetActive(false);
			_hudCanvasGroup.DOFade(1, 0.2f).SetUpdate(true);
			mapTxtGroup.DOFade(0f, 0.2f);
		}
	}

	private string ReturnCurrentFloorOrSceneAsString()
	{
		var floorTxt = dataHolder.currentLevel.ToString();

		if (SceneManager.GetActiveScene().name == "MainScene" && floorTxt.Contains("Floor"))
		{
			// https://discussions.unity.com/t/extract-number-from-string/4361
			floorTxt = Regex.Replace(floorTxt, @"^(Floor)(.+)$", "$1 $2");
		}
		else if (SceneManager.GetActiveScene().name == "Intermission")
		{
			floorTxt = "Construct Lobby";
		}
		else if (SceneManager.GetActiveScene().name == "Tutorial")
		{
			floorTxt = "Tutorial";
		}

		return floorTxt;
	}

	public void SceneReload() // reloads scene
	{
		ButtonHandler.Instance.PlayBackSound();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void LoadScene(string scene)
	{
		SaveData.Instance.UpdateSave();
		SceneManager.LoadScene(scene);
	}

	public void Quit() // quits game
	{
		ButtonHandler.Instance.PlayBackSound();
		Application.Quit();
	}
}
