using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    public static ButtonHandler Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one ButtonHandler script in the scene.");
        }

        Instance = this;
    }
    public void PlayConfirmSound()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.UISelect, transform.position);
    }

    public void PlayBackSound()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.UIBack, transform.position);
    }
}
