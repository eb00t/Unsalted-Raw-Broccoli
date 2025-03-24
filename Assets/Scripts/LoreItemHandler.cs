using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
[CreateAssetMenu(fileName = "New Lore Item", menuName = "ScriptableObjects/Lore Item Handler", order = 2)]
public class LoreItemHandler : ScriptableObject
{
    public string loreTitle;
    public string whoWroteThis;
    [TextArea(3, 10)]
    public string[] loreBodyText;
    public bool discoveredByPlayer;

}
