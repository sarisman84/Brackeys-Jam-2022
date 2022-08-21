using System.Collections;
using UnityEngine;

public abstract class BaseGun : ScriptableObject
{
    public float ammunitionAmm;
    public float fireRate;
    public GameObject gunPrefab;
    public abstract void OnGunUse(RaycastHit hitInfo, GameObject gunModel, MonoBehaviour coroutineStarter);

}
