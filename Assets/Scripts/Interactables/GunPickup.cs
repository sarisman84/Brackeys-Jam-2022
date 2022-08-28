using UnityEngine;

public class GunPickup : MonoBehaviour, IInteractable
{
    public BaseGun assignedGun;

    public void OnInteraction(GameObject owner)
    {
        if (owner && owner.GetComponent<WeaponManager>() is WeaponManager weaponManager)
        {
            PollingStation.Instance.audioManager.Play("GunPickup");
            weaponManager.EquiptNewWeapon(assignedGun);
            Destroy(gameObject);
        }
    }
}
