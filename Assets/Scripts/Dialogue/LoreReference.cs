using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoreReference : MonoBehaviour
{ 
    public static LoreReference Instance { get; private set; }
    public List<LoreItemHandler> allLoreItems;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one LoreReference script in the scene.");
        }

        Instance = this;
        
        allLoreItems.Add(AICommunication1);
        allLoreItems.Add(AICommunication2);
        allLoreItems.Add(AICommunication3);
        allLoreItems.Add(TravelersLog1);
        allLoreItems.Add(TravelersLog2);
        allLoreItems.Add(TravelersLog3);
        allLoreItems.Add(SpyGettingHitByACar);
        allLoreItems.Add(YummyBot);
        allLoreItems.Add(AllAboutRocks);
        allLoreItems.Add(EmailTest);
        allLoreItems.Add(RichardSecret);
        allLoreItems.Add(WhyTheConstruct);
        allLoreItems.Add(DistressCall);
        allLoreItems.Add(Breadcrumbs);
        allLoreItems.Add(Fortunes);
        allLoreItems.Add(OldFriend);
        allLoreItems.Add(MessageToSelf);
        allLoreItems.Add(BasicEnemyTips);
        allLoreItems.Add(FlyingEnemyTips);
        allLoreItems.Add(SilentEnemyTips);
        allLoreItems.Add(BombEnemyTips);
    }
    
    [field: Header("AI Communication")]
    [field: SerializeField] public LoreItemHandler AICommunication1 { get; private set; }
    [field: SerializeField] public LoreItemHandler AICommunication2 { get; private set; }
    [field: SerializeField] public LoreItemHandler AICommunication3 { get; private set; }
    
    [field: Header("Traveler's Log")]
    [field: SerializeField] public LoreItemHandler TravelersLog1 { get; private set; }
    [field: SerializeField] public LoreItemHandler TravelersLog2 { get; private set; }
    [field: SerializeField] public LoreItemHandler TravelersLog3 { get; private set; }
    
    [field: Header("Silly Things")]
    [field: SerializeField] public LoreItemHandler SpyGettingHitByACar { get; private set; }
    [field: SerializeField] public LoreItemHandler YummyBot { get; private set; }
    [field: SerializeField] public LoreItemHandler AllAboutRocks { get; private set; }
    [field: SerializeField] public LoreItemHandler EmailTest { get; private set; }

    [field: Header("Intermission Character Lore")]
    [field: SerializeField] public LoreItemHandler RichardSecret { get; private set; }
    [field: SerializeField] public LoreItemHandler WhyTheConstruct { get; private set; }
    [field: SerializeField] public LoreItemHandler MessageToSelf { get; private set; }
    [field: SerializeField] public LoreItemHandler DistressCall { get; private set; }
    
    [field: Header("Enemy Tips")]
    [field: SerializeField] public LoreItemHandler BasicEnemyTips { get; private set; }
    [field: SerializeField] public LoreItemHandler FlyingEnemyTips { get; private set; }
    [field: SerializeField] public LoreItemHandler SilentEnemyTips { get; private set; }
    [field: SerializeField] public LoreItemHandler BombEnemyTips { get; private set; }
    
    [field: Header("Other")]
    [field: SerializeField] public LoreItemHandler Breadcrumbs { get; private set; }
    [field: SerializeField] public LoreItemHandler Fortunes { get; private set; }
    [field: SerializeField] public LoreItemHandler OldFriend { get; private set; }

}
