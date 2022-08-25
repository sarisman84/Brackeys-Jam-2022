using UnityEngine;
using UnityEngine.Events;

public class PlayerTriggerEvent : MonoBehaviour
{
    public UnityEvent OnPlayerEnter;
    public UnityEvent OnPlayerExit;

    private void OnTriggerEnter(Collider other) {
        if (other.GetComponent<FPSController>())
            OnPlayerEnter.Invoke();
    }

    private void OnTriggerExit(Collider other) {
        if (other.GetComponent<FPSController>())
            OnPlayerExit.Invoke();
    }
}
