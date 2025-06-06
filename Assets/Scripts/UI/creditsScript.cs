using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class creditsScript : MonoBehaviour
{
    [SerializeField] private Button mainButton, quitButton;
    [SerializeField] private StartMenuController startMenuController;
    
    private void SetButtonsInteractable()
    {
        mainButton.interactable = true;
        quitButton.interactable = true;
        startMenuController.SwitchSelected(mainButton.gameObject);
    }

    private void TriggerFinished()
    {
        startMenuController.creditsFinished = true;
    }
}
