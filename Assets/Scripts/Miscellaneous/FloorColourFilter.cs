using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorColourFilter : MonoBehaviour
{
  private Light ambientLight;

  void Start()
  {
    ambientLight = GetComponent<Light>();
    switch (LevelBuilder.Instance.currentFloor)
    {
      case LevelBuilder.LevelMode.Floor1:
        ambientLight.color = new Color(0.764151f, 0.676939f, 0.637994f);
        break;
      case LevelBuilder.LevelMode.Floor2:
        ambientLight.color = new Color(0.7330484f, 0.6392157f, 0.7647059f);
        break;
      case LevelBuilder.LevelMode.Floor3:
        ambientLight.color = new Color(0.6392157f, 0.7000099f, 0.7647059f);
        break;
      case LevelBuilder.LevelMode.FinalBoss:
        ambientLight.color = new Color(0.7647059f, 0.6430389f, 0.6392157f);
        break;
    }
  }
}
