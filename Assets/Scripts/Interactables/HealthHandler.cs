using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CapsuleCollider))]
public class HealthHandler : MonoBehaviour, IDamageable
{
    public float maxHealth;
    public float currentHealth { private set; get; }

    public UnityEvent onEntityDeath;



    private void OnEnable()
    {
        ResetHealth();
    }

    public void AddHealth(float addHealth) {
        currentHealth += addHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    public void ResetHealth() {
        currentHealth = maxHealth;
    }

    public void OnDamageTaken(float someDamage)
    {
        if (maxHealth <= 0) return;

        Debug.Log($"[Log]<HealthHandler/{gameObject.name}>: Taking Damage - {someDamage}");
        currentHealth -= someDamage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (currentHealth <= 0)
        {
            onEntityDeath?.Invoke();
        }
    }
}
