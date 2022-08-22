using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class WeaponManager : MonoBehaviour
{
    const float XAXis_ADSOffset = -0.325f;
    const float YAxis_ADSOffset = 0.26f;
    const float ZAxis_ADSOffset = 0.765f / 2.0f;

    public float adsTransitionRate = 5.25f;

    public Transform gunPosition;
    public CinemachineVirtualCamera cameraController;
    [Header("Input Settings")]
    public InputActionReference fireInput;
    public InputActionReference adsInput;

    public bool isAimingDownSights { get; private set; }

    private BaseGun currentGun;
    private float currentFireRate;
    private int currentAmmo;
    private GameObject currentGunModel;
    private Transform currentGunBarrel;
    private Transform currentADSPOV;

    //Recoil logic
    private Vector3 currentRotation;
    private Vector3 targetRotation;
    private CinemachineRecoil rotOffset;

    private void Update()
    {
        if (PollingStation.Instance.optionsManager.currentState != OptionsManager.RuntimeState.Playing) return;

        ResetRecoil();
        if (!currentGun) return;

        isAimingDownSights = adsInput.action.ReadValue<float>() > 0;
        currentFireRate += Time.deltaTime;

        if (currentFireRate >= currentGun.fireRate && fireInput.action.ReadValue<float>() > 0)
        {
            currentGun.OnWeaponFire(this, currentGunBarrel, this);

            currentAmmo--;
            currentFireRate = 0;

            if (currentAmmo <= 0)
            {
                DiscardCurrentGun();
            }
        }


        if (isAimingDownSights)
        {
            ADS();
        }
        else
        {
            Hipfire();
        }

    }


    private void ADS()
    {
        if (!currentGunModel) return;
        Vector3 offsetLocal = new Vector3(XAXis_ADSOffset, YAxis_ADSOffset, -(ZAxis_ADSOffset + currentADSPOV.localPosition.z));




        currentGunModel.transform.localPosition = Vector3.Lerp(currentGunModel.transform.localPosition, offsetLocal, Time.deltaTime * adsTransitionRate);
    }


    private void Hipfire()
    {
        if (!currentGunModel) return;
        currentGunModel.transform.localPosition = Vector3.Lerp(currentGunModel.transform.localPosition, Vector3.zero, Time.deltaTime * adsTransitionRate);
    }

    private void Awake()
    {
        rotOffset = cameraController.GetComponentInChildren<CinemachineRecoil>();
    }

    public void ApplyRecoil(Vector3 recoilForce)
    {
        var recoil = recoilForce;
        targetRotation += new Vector3(recoil.x, Random.Range(-recoil.y, recoil.y), Random.Range(-recoil.z, recoil.z));
    }

    private void ResetRecoil()
    {
        float returnSpeed = currentGun ? currentGun.returnSpeed : 2.0f;
        float snappiness = currentGun ? currentGun.snappiness : 6.0f;


        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        rotOffset.m_RecoilAmount = currentRotation;


    }



    public void RefreshAmmunition(BaseGun aNewGun)
    {
        currentAmmo = aNewGun.ammunitionAmm / 2;
    }


    public void EquiptNewWeapon(BaseGun aNewGun)
    {
        if (currentGun == aNewGun)
        {
            RefreshAmmunition(aNewGun);
            return;
        }
        currentGun = aNewGun;
        currentFireRate = 0;
        currentAmmo = aNewGun.ammunitionAmm;
        VisualiseGun(aNewGun);

        currentGunBarrel = null;
        currentADSPOV = null;

        for (int i = 0; i < currentGunModel.transform.childCount; i++)
        {
            if (currentADSPOV && currentGunBarrel) break;

            Transform child = currentGunModel.transform.GetChild(i);
            if (child.tag.ToLower().Equals("barrel"))
            {
                currentGunBarrel = child;

            }
            if (child.tag.ToLower().Equals("ads"))
            {
                currentADSPOV = child;
            }
        }
    }


    public void DiscardCurrentGun()
    {
        currentGun = null;
        ClearVisualisation();
    }



    void DisplayAmmoCount()
    {

    }



    void ClearVisualisation()
    {
        if (currentGunModel)
        {
            Destroy(currentGunModel);
            currentGunModel = null;
        }
    }

    void VisualiseGun(BaseGun gun)
    {
        ClearVisualisation();


        currentGunModel = Instantiate(gun.gunPrefab, gunPosition);
        currentGunModel.transform.localPosition = Vector3.zero;
        currentGunModel.transform.localRotation = Quaternion.identity;


    }





    private void OnEnable()
    {
        fireInput.action.Enable();
        adsInput.action.Enable();
    }

    private void OnDisable()
    {
        fireInput.action.Disable();
        adsInput.action.Disable();
    }


}
