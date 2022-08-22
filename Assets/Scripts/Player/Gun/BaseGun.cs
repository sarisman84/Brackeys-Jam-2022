using System.Collections;
using UnityEngine;

public abstract class BaseGun : ScriptableObject
{
    [Header("Base")]
    public int ammunitionAmm;
    public float fireRate;
    public float damage;

    [Header("Recoil Settings")]
    public Vector3 recoilForce = new Vector3(-2, 2, 0.35f);
    public float snappiness = 6, returnSpeed = 2;

    [Header("Prefab Settings")]
    public GameObject gunPrefab;
    public GameObject muzzleEffect;
    public GameObject bulletEffect;

    public void OnWeaponFire(MonoBehaviour coroutineInitiator, Transform barrel, WeaponManager manager)
    {
        Fire(manager, barrel);
        manager.ApplyRecoil(recoilForce);
        coroutineInitiator.StartCoroutine(MuzzleDefinition(manager, barrel));
        coroutineInitiator.StartCoroutine(BulletDefinition(manager, barrel));
    }
    protected abstract void Fire(WeaponManager weaponManager, Transform barrel);
    protected abstract IEnumerator MuzzleDefinition(WeaponManager weaponManager, Transform barrel);
    protected abstract IEnumerator BulletDefinition(WeaponManager weaponManager, Transform barrel);

}
