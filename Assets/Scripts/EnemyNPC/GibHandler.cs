using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GibHandler : MonoBehaviour
{
  public List<GameObject> allGibs = new List<GameObject>();

  private void Start()
  {
    foreach (var gib in GetComponentsInChildren<Transform>())
    {
      allGibs.Add(gib.gameObject);
    }

    allGibs.Remove(gameObject);
    StartCoroutine(LockPosition());

  }

  IEnumerator LockPosition()
  {
    yield return new WaitForSecondsRealtime(20);
    foreach (var gib in allGibs)
    {
      gib.GetComponent<Rigidbody>().isKinematic = true;
      gib.GetComponent<Collider>().enabled = false;
    }
  }
}
