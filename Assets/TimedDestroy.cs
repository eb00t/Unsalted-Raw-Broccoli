using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDestroy : MonoBehaviour
{
	public IEnumerator DestroyThis(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		Destroy(gameObject);
	}
}
