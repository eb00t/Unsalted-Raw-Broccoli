using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideImmediately : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(HideMe());
    }

    IEnumerator HideMe()
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }
}
