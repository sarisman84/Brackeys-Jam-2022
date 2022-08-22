using System.Collections;
using UnityEngine;

public class Pickup : MonoBehaviour, IInteractable
{
    public BaseGun assignedGun;

    public void OnInteraction(GameObject owner)
    {
        if (owner && owner.GetComponent<WeaponManager>() is WeaponManager weaponManager)
        {
            weaponManager.EquiptNewWeapon(assignedGun);
            Destroy(gameObject);
        }
    }
}
