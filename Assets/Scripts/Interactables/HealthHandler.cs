using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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

        currentHealth -= someDamage;

        if (currentHealth <= 0)
        {
            onEntityDeath?.Invoke();
        }
    }
}
