using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PassiveData", menuName = "ScriptableObjects/PassiveData", order = 1)]
public class PassiveDataBase : ScriptableObject
{
    public List<PermanentPassiveItem> allPassives;
}