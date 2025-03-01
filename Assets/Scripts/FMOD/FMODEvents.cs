using FMODUnity;
using UnityEngine;

public class FMODEvents : MonoBehaviour
{ 
    /* EVENT RULES
    1. Use separate events for different entities, e.g., the Player getting hurt and an Infantry getting hurt should
    have two events (PlayerHurt and InfantryHurt).
    2. Use the same event for different intensities of the same sound effect. Use a local parameter in most cases.
    3. Do not touch any code that is written already, as this may cause the assigned event references to be lost.
    This means someone will have to reassign them all again manually. Nobody wants to do that. */
    public static FMODEvents Instance { get; private set; }
    
    /*[field: Header("INSERT NAME IF NECESSARY")]
    [field: SerializeField] public EventReference INSERT NAME HERE { get; private set; }*/
    
    
    [field: Header("Ambience")]
    [field: SerializeField] public EventReference Ambience { get; private set; }
    [field: SerializeField] public EventReference Loading { get; private set; }
    
    [field: Header("Music")]
    [field: SerializeField] public EventReference Music { get; private set; }
    [field: Header("SFX")]
    [field: SerializeField] public EventReference DoorSlam { get; private set; }
    [field: SerializeField] public EventReference Footsteps { get; private set; }
    [field:Header("UI")]
    
    
     
    
    private void Awake() 
    {
        if (Instance != null)
        {
            Debug.LogError("More than one FMOD Events script in scene.");
        }

        Instance = this;
    }
}
