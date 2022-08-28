using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class Gun : IEquatable<BaseGun>
{
    public event Func<Gun, IEnumerator> onMuzzleFireCallback, onBulletFireCallback;
    public event Action<Gun> onFireCallback;


    private Vector3 targetRotation, currentRotation;
    private CinemachineRecoil rotOffset;


    //private float m_fireRate;
    //private float m_Damage;
    //private float m_snappiness, m_returnSpeed;

    //private Vector3 m_recoilForce;
    private float currentFireRate;
    private float adsAnimCurveCounter;
    private MonoBehaviour coroutine;
    private Coroutine cUpdateRecoil;



    public BaseGun template { get; private set; }
    public int CurrentAmmo { get; private set; }
    public Transform gunBarrel { get; private set; }
    public Transform adsPOV { get; private set; }
    public GameObject instancedGunModel { get; private set; }
    public bool isAimingDownTheSights { get; private set; } = false;
    public WeaponManager weaponManager { get; private set; }



    public Gun(Transform gunPosition, BaseGun templateGun, MonoBehaviour coroutineStarter, bool applyRecoil, LayerMask renderingLayer)
    {
        coroutine = coroutineStarter;

        if (coroutine is WeaponManager wm)
        {
            weaponManager = wm;
        }

        template = templateGun;


        CurrentAmmo = templateGun.ammunitionAmm;


        instancedGunModel = Object.Instantiate(templateGun.gunPrefab, gunPosition);
        instancedGunModel.transform.localPosition = Vector3.zero;
        instancedGunModel.transform.localRotation = Quaternion.identity;
        instancedGunModel.SetLayer(renderingLayer);

        for (int i = 0; i < instancedGunModel.transform.childCount; i++)
        {
            Transform child = instancedGunModel.transform.GetChild(i);
            if (child.tag.ToLower().Equals("barrel"))
            {
                gunBarrel = child;

            }
            if (child.tag.ToLower().Equals("ads"))
            {
                adsPOV = child;
            }
        }


        var cameraController = PollingStation.Instance.cameraController;
        rotOffset = cameraController.GetComponentInChildren<CinemachineRecoil>();

        if (applyRecoil)
        {

            cUpdateRecoil = coroutine.StartCoroutine(UpdateRecoil());
        }


    }

    public void DeleteGun()
    {
        if (instancedGunModel)
        {
            Object.Destroy(instancedGunModel);
            instancedGunModel = null;
        }


        if (cUpdateRecoil != null)
            coroutine.StopCoroutine(cUpdateRecoil);
    }


    public void UseADS(bool useState, Vector3 offset, AnimationCurve adsTransitionRate)
    {
        isAimingDownTheSights = useState;
        if (useState)
        {
            ADS(offset, adsTransitionRate);
            return;
        }
        Hipfire(adsTransitionRate);

    }

    public void RefillCurrentAmmo(int amm)
    {
        CurrentAmmo += amm;
        CurrentAmmo = Mathf.Min(CurrentAmmo, template.ammunitionAmm);//make sure we cant have more ammunition than the maximum
    }

    private void ADS(Vector3 offset, AnimationCurve adsTransitionRate)
    {
        Vector3 offsetLocal = new Vector3(offset.x, offset.y, -(offset.z + adsPOV.localPosition.z));



        adsAnimCurveCounter += Time.deltaTime;
        adsAnimCurveCounter = Mathf.Clamp(adsAnimCurveCounter, 0, 1);
        instancedGunModel.transform.localPosition = Vector3.Lerp(instancedGunModel.transform.localPosition, offsetLocal, adsTransitionRate.Evaluate(adsAnimCurveCounter));
    }

    private void Hipfire(AnimationCurve adsTransitionRate)
    {
        instancedGunModel.transform.localPosition = Vector3.Lerp(instancedGunModel.transform.localPosition, Vector3.zero, adsTransitionRate.Evaluate(adsAnimCurveCounter));
        adsAnimCurveCounter -= Time.deltaTime;
        adsAnimCurveCounter = Mathf.Clamp(adsAnimCurveCounter, 0, 1);
    }

    private IEnumerator UpdateRecoil()
    {
        while (true)
        {
            yield return null;
            ResetRecoil();
        }

    }
    public void Fire()
    {
        currentFireRate += Time.deltaTime;

        if (currentFireRate >= template.fireRate)
        {
            onFireCallback?.Invoke(this);
            ApplyRecoil(template.recoilForce);
            coroutine?.StartCoroutine(onBulletFireCallback?.Invoke(this));
            coroutine?.StartCoroutine(onMuzzleFireCallback?.Invoke(this));
            currentFireRate = 0;
            if (template.ammunitionAmm > 0)
                CurrentAmmo--;
        }
    }


    private void ApplyRecoil(Vector3 aForce)
    {


        var recoil = aForce;
        targetRotation += new Vector3(recoil.x, Random.Range(-recoil.y, recoil.y), Random.Range(-recoil.z, recoil.z));
    }


    private void ResetRecoil()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, template.returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, template.snappiness * Time.fixedDeltaTime);
        rotOffset.m_RecoilAmount = currentRotation;
    }

    public bool Equals(BaseGun? other)
    {
        return template == other;
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;

        BaseGun gun = obj as BaseGun;
        if (gun == null) return false;
        else
            return Equals(gun);
    }

    public static bool operator ==(Gun gun, BaseGun template)
    {
        if (((object)gun) == null || ((object)template) == null)
            return object.Equals(gun, template);

        return gun.Equals(template);
    }

    public static bool operator !=(Gun gun, BaseGun template)
    {
        if (((object)gun) == null || ((object)template) == null)
            return !object.Equals(gun, template);

        return !gun.Equals(template);
    }


}



public abstract class BaseGun : ScriptableObject
{
    [Header("Base")]
    public int ammunitionAmm;
    public float fireRate;
    public float damage;
    public LayerMask hitMask;

    [Header("Recoil Settings")]
    public Vector3 recoilForce = new Vector3(-2, 2, 0.35f);
    public float snappiness = 6, returnSpeed = 2;

    [Header("Prefab Settings")]
    public GameObject gunPrefab;
    public GameObject muzzleEffect;
    public GameObject bulletEffect;
    public GameObject impactDecal;


    public Gun Initialize<T>(T coroutineInitiator, Transform gunPosition, LayerMask renderingLayer, bool applyRecoil = true) where T : MonoBehaviour
    {

        Gun newGun = new Gun(gunPosition, this, coroutineInitiator, applyRecoil, renderingLayer);

        newGun.onFireCallback += Fire;
        newGun.onBulletFireCallback += BulletDefinition;
        newGun.onMuzzleFireCallback += MuzzleDefinition;

        return newGun;


    }








    ////public void OnWeaponFire(MonoBehaviour coroutineInitiator, bool applyRecoil = true)
    ////{
    ////    Fire(gunBarrel);
    ////    if (applyRecoil)
    ////        ApplyRecoil(recoilForce);
    ////    coroutineInitiator.StartCoroutine(MuzzleDefinition(gunBarrel));
    ////    coroutineInitiator.StartCoroutine(BulletDefinition(gunBarrel));
    ////}

    protected Ray FiringDirection(Gun gun)
    {
        Ray ray = gun.weaponManager ?
          Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f)) :
          new Ray(gun.gunBarrel.transform.position, gun.gunBarrel.transform.forward.normalized);
        return ray;
    }

    protected abstract void Fire(Gun gun);
    protected abstract IEnumerator MuzzleDefinition(Gun gun);
    protected abstract IEnumerator BulletDefinition(Gun gun);


    protected void CreateImpactDecal(RaycastHit hitInfo) {
        Vector3 tangent = hitInfo.normal.GetOrtho();
        Quaternion rot = Quaternion.LookRotation(-hitInfo.normal, tangent);
        Instantiate(impactDecal, hitInfo.point, rot, PollingStation.Instance.gameManager.GetEntityParent());
    }
}
