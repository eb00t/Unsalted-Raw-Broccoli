using TMPro;
using System;
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
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private LoreItemHandler statsLore;

    private void Start()
    {
        _eventSystem = MenuHandler.Instance.eventSystem;
        LoadStatsToLore();
        PopulateButtons();
        
        PopulateLines(LoreReference.Instance.Welcome);
    }

    private void LoadStatsToLore()
    {
        statsLore.loreBodyText[0] = dataHolder.playerEnemiesKilled.ToString();
        statsLore.loreBodyText[1] = dataHolder.playerDeaths.ToString();
        statsLore.loreBodyText[2] = dataHolder.totalCoilsCollected.ToString();
        var timeToClear = TimeSpan.FromSeconds(dataHolder.playerTimeToClear);
        statsLore.loreBodyText[3] = timeToClear.ToString(@"hh\:mm\:ss");

        if (dataHolder.playerPersonalBestTime > 0)
        {
            statsLore.loreBodyText[4] = dataHolder.playerPersonalBestTime.ToString();
        }
        else
        {
            statsLore.loreBodyText[4] = "NO DATA FOUND";
        }

        if (dataHolder.demoMode)
        {
            statsLore.whoWroteThis[6] = "TOTAL ENEMIES KILLED";
            statsLore.loreBodyText[6] = dataHolder.totalEnemiesKilled.ToString();
            
            statsLore.whoWroteThis[7] = "TOTAL DEATHS";
            statsLore.loreBodyText[7] = dataHolder.totalDeaths.ToString();
            
            statsLore.whoWroteThis[8] = "TOTAL COINS COLLECTED";
            statsLore.loreBodyText[8] = dataHolder.totalCoilsCollected.ToString();
            
            statsLore.whoWroteThis[9] = "GLOBAL FASTEST RUN TIME";
            if (dataHolder.fastestClearTime > 0)
            {
                var globalTimeToClear = TimeSpan.FromSeconds(dataHolder.fastestClearTime);
                statsLore.loreBodyText[9] = globalTimeToClear.ToString(@"hh\:mm\:ss");
            }
            else
            {
                statsLore.loreBodyText[9] = "NO DATA FOUND";
            }
        }
        else
        {
            statsLore.whoWroteThis[6] = "";
            statsLore.loreBodyText[6] = "";
            
            statsLore.whoWroteThis[7] = "";
            statsLore.loreBodyText[7] = "";
            
            statsLore.whoWroteThis[8] = "";
            statsLore.loreBodyText[8] = "";
            
            statsLore.whoWroteThis[9] = "";
            statsLore.loreBodyText[9] = "";
        }
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
