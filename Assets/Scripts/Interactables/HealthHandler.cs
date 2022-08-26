using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CapsuleCollider))]
public class HealthHandler : MonoBehaviour, IDamageable
{
    public float maxHealth;
    private float currentHealth;

    public UnityEvent onEntityDeath;



    private void OnEnable()
    {
        currentHealth = maxHealth;
    }

    public void OnDamageTaken(float someDamage)
    {
        if (maxHealth <= 0) return;

        Debug.Log($"[Log]<HealthHandler/{gameObject.name}>: Taking Damage - {someDamage}");
        currentHealth -= someDamage;

        if (currentHealth <= 0)
        {
            onEntityDeath?.Invoke();
        }
    }
}
