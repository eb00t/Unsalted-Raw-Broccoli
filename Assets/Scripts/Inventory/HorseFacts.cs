﻿using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class HorseFacts : MonoBehaviour
{
     public static HorseFacts Instance { get; private set; }
     [SerializeField] private DataHolder dataHolder;

     private void Awake()
     {
          if (Instance != null)
          {
               Debug.LogError("More than one HorseFacts script in the scene.");
          }

          Instance = this;
     }

     private readonly List<string> _horseFactsStatic = new() // this stores what horse facts exist and doesnt change
     {
          "When the horse loses his patience...The devil shivers.",
          "When a horse wants something, he must pursue it with all he has.",
          "There is one rule, above all others, for being a horse. Whatever happens, face it on your hooves.",
          "A motivated horse is strong, but a disciplined horse is deadly.",
          "Nowadays, the alpha horse is the loser, and the sigma horse is the winner.",
          "Fear the horse of few words and even fewer opinions.",
          "The horse is the greatest teacher for how to come to the light.",
          "Things change, and horses leave. Life doesn't stop for anybody.",
          "Think big horses but relish small horses.",
          "Caring what horses think is the biggest jail one can put oneself in.",
          "Be careful who you trust. Horses and other horses look the same.",
          "Life is made of ever so many horses welded together.",
          "Horses will take you under until you learn to swim through them.",
          "It's ok to let some horses leave.",
          "The horses are calling, and you must go.",
          "Be happy for this horse. This horse is your life.",
          "One good thing about the horse, when it hits you, you feel no pain.",
          "Not all those who wander are horses.",
          "Pain is inevitable. The horse is here. Give up.",
          "When you're happy, you enjoy the horse. When you're sad, you understand the horse.",
          "You know, maybe we're all horses on the inside.",
          "Stop looking for horses in the same place you lost them.",
          "60+ best sigma horse quotes and captions for your photos.",
          "Every next level of your horse will demand a different version of you.",
          "Attitude is the difference between a horse and a killer horse.",
          "Manifesting an August full of horses.",
          "Horses will not be less valiant because they are unpraised.",
          "A horse's heart is a deep ocean of secrets.",
          "Horses, with their built-in sense of order, service and discipline, should really be running the world.",
          "You must be the horse you wish to see in the world",
          "Know thy self, know thy horse, a thousand horses, a thousand victories.",
          "Horse is just a label you can give to a set of conditions you're experiencing.",
          "Don't lose your horse while chasing your horse.",
          "A horse is alive in a way that no robot horse will ever be.",
     };

     [NonSerialized] public List<string> _horseFacts =  new List<string>();
     
     public string HorseFact()
     {
          if (_horseFacts.Count == 0)
          {
               _horseFacts = new List<string>(_horseFactsStatic);
          }
          
          var chosenFact = _horseFacts[Random.Range(0, _horseFacts.Count)];
          _horseFacts.Remove(chosenFact);
          return chosenFact;
     }

     public void ResetReadHorseFacts()
     {
          _horseFacts = new List<string>(_horseFactsStatic);
     }
}
