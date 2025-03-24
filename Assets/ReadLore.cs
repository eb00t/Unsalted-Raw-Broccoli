using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadLore : MonoBehaviour
{
    private string _language;
    private string _lorePath;
    private string _loreObject;
    private string _fullLorePath;
    public enum LoreType
    {
        Book,
        Data,
        ScrapOfPaper,
        Other
    }
    public LoreType loreType;
    private List<ScriptableObject> _allLore;
    void Start()
    {
        switch (loreType)
        {
            case LoreType.Book:
                _loreObject = "Book Lore";
                break;
            case LoreType.Data:
                _loreObject = "Data Lore";
                break;
            case LoreType.ScrapOfPaper:
                _loreObject = "Scrap Of Paper Lore";
                break;
        }
        _lorePath = "Lore";
        _language = "English"; //TODO: Fix this to make it work with whatever language the game is.
        _fullLorePath = _lorePath + "/" + _language + "/" + _loreObject;
        Debug.Log(_fullLorePath);
        _allLore = new List<ScriptableObject>();
        foreach (var lore in Resources.LoadAll<ScriptableObject>(_fullLorePath))
        {
            _allLore.Add(lore);
        }
    }
    
    
    
}
