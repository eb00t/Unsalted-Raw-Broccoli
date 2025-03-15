using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableDisableSounds : MonoBehaviour
{
    //Add this script to an object that is directly turned on or off by a UI button
    void OnEnable()
    {
        ButtonHandler.Instance.PlayConfirmSound();
    }

    void OnDisable()
    {
        ButtonHandler.Instance.PlayBackSound();
    }
}
