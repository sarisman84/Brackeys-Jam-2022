using UnityEngine;

public class HealthPickup : MonoBehaviour, IInteractable
{
    public float addHealth = 5;
    public void OnInteraction(GameObject owner) {
        if (owner && owner.GetComponent<HealthHandler>() is HealthHandler healthHandler) {
            healthHandler.AddHealth(addHealth);
            Destroy(gameObject);
        }
    }
}
