using UnityEngine;
interface IDamageable
{
    void OnDamageTaken(float someDamage);
}


interface IInteractable
{
    void OnInteraction(GameObject owner);
}
