using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class NPCPopup : MonoBehaviour
{
    [SerializeField] private GameObject popup;
    private Animator _animator;
    private TextMeshProUGUI _text;

    private void Start()
    {
        _animator = popup.GetComponent<Animator>();
        _text = popup.GetComponentInChildren<TextMeshProUGUI>();
        /*
         EXAMPLE TRIGGER:
        TriggerPopup("I'm DR. BLANK, and I have trapped you in this simulation AHAHAHAHAHAHAHA. You will never escape!!", 5f);
        */
    }

    public void TriggerPopup(string message, float duration)
    {
        _text.text = message;
        StartCoroutine(ShowTimedMessage(duration));
    }

    private IEnumerator ShowTimedMessage(float dur)
    {
        _animator.SetTrigger("MessageIn");
        _animator.SetBool("IsTalking", true);
        yield return new WaitForSecondsRealtime(dur);
        _animator.SetBool("IsTalking", false);
    }
}
