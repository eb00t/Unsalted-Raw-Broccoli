using System;
using UnityEngine;

public class TutorialEvent : MonoBehaviour
{
    [SerializeField] private TutorialEventType eventType;
    private TutorialController _tutorialController;

    private void Start()
    {
        _tutorialController = GameObject.FindGameObjectWithTag("Player").GetComponent<TutorialController>();
    }

    private enum TutorialEventType
    {
        ItemPickUp
    }

    private void OnDisable()
    {
        if (eventType == TutorialEventType.ItemPickUp)
        {
            _tutorialController.OnItemPickedUp();
        }
    }
}
