using UnityEngine;

public class LaserTutorialCheck : MonoBehaviour
{
    [SerializeField] private TriggerType triggerType;
    public enum TriggerType
    {
        BeforeLaser,
        AfterLaser
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerType == TriggerType.BeforeLaser)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<TutorialController>().OnLaserFound();
            }
        }
        else if (triggerType == TriggerType.AfterLaser)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<TutorialController>().OnLaserPassed();
            }
        }
    }
}
