using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoreGUIManager : MonoBehaviour
{
    [SerializeField] private Transform loreButton, loreLine;
    public GameObject loreButtonHolder, loreLineHolder;
    public GameObject loreBck, loreBtn;
    private EventSystem _eventSystem;
    private GameObject _lastSelected;
    private int _siblingIndex;

    private void Start()
    {
        _eventSystem = MenuHandler.Instance.eventSystem;
        PopulateButtons();
        PopulateLines(LoreReference.Instance.Welcome);
    }

    private void Update()
    {
        var current = _eventSystem.currentSelectedGameObject;

        if (current == null || current == _lastSelected) return;

        var loreHolder = current.GetComponent<LoreHolder>();
        if (loreHolder != null)
        {
            PopulateLines(loreHolder.loreItemHandler);
        }

        _lastSelected = current;
    }


    private void PopulateButtons()
    {
        foreach (var readLoreItem in LoreReference.Instance.allViewedLoreItems)
        {
            var newLoreButton = Instantiate(loreButton, loreButton.position, loreButton.transform.rotation, loreButtonHolder.transform);
            
            newLoreButton.GetComponent<LoreHolder>().loreItemHandler = readLoreItem;

            if (readLoreItem.loreBodyText[0] != "FILE NOT FOUND")
            {
                newLoreButton.SetSiblingIndex(_siblingIndex);
                _siblingIndex++;
            }

            /*
            foreach (var img in newLoreButton.GetComponentsInChildren<Image>())
            {
                if (img.name == "Image") {} // set image here (right now there's no lore specific images)
            }
            */
            
            foreach (var txt in newLoreButton.GetComponentsInChildren<TextMeshProUGUI>())
            {
                if (txt.name == "Title") txt.text = readLoreItem.doesThisHaveATitle ?  readLoreItem.loreTitle : "Untitled";
            }
        }
    }

    public void PopulateLines(LoreItemHandler loreItemHandler)
    {
        ClearExistingLines();

        for (var i = 0; i < loreItemHandler.loreBodyText.Length; i++)
        {
            var bodyText = loreItemHandler.loreBodyText[i];
            var newLoreLine = Instantiate(loreLine, loreLine.position, loreLine.transform.rotation, loreLineHolder.transform);

            foreach (var txt in newLoreLine.GetComponentsInChildren<TextMeshProUGUI>())
            {
                switch (txt.name)
                {
                    case "BodyText":
                        txt.text = bodyText;
                        break;
                    case "Speaker":
                    {
                        if (loreItemHandler.didAnyoneWriteThis)
                        {
                            txt.text = loreItemHandler.whoWroteThis[i];
                        }

                        break;
                    }
                }
            }
        }
    }
    
    private void ClearExistingLines()
    {
        foreach (var line in loreLineHolder.GetComponentsInChildren<Button>())
        {
            if (line.name.Contains("LoreLine"))
            {
                Destroy(line.gameObject);
            }
        }
    }
}
