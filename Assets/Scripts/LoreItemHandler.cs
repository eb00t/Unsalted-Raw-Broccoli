using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
[CreateAssetMenu(fileName = "New Lore Item", menuName = "ScriptableObjects/Lore Item Handler", order = 2)]
public class LoreItemHandler : ScriptableObject
{
    [field: Tooltip("Whatever you want to call your lore document.")]
    public string loreTitle;
    [field: Tooltip("If your document is written from the perspective of an unknown person, put ???? here.")]
    public string whoWroteThis; 
    [field: Tooltip("The contents of the lore document.")]
    [TextArea(3, 10)]
    public string[] loreBodyText;
    [field: Tooltip("If this is set to true, the lore will not spawn in levels, only within the lore room of the intermission.")]
    public bool discoveredByPlayer;
    

}
