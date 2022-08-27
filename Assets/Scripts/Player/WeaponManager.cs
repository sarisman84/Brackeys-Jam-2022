using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class WeaponManager : MonoBehaviour
{
    const float XAXis_ADSOffset = -0.325f;
    const float YAxis_ADSOffset = 0.26f;
    const float ZAxis_ADSOffset = 0.765f / 2.0f;

    public AnimationCurve adsTransitionRate;

    public Transform gunPosition;

    [Header("Input Settings")]
    public InputActionReference fireInput;
    public InputActionReference adsInput;

    public bool isAimingDownSights { get; private set; }

    public Gun currentGun { get; private set; }






    private void Update()
    {
        if (PollingStation.Instance.runtimeManager.currentState != RuntimeManager.RuntimeState.Playing) return;

        if (currentGun == null) return;

        isAimingDownSights = adsInput.action.ReadValue<float>() > 0;
        currentGun.UseADS(isAimingDownSights, new Vector3(XAXis_ADSOffset, YAxis_ADSOffset, ZAxis_ADSOffset), adsTransitionRate);

        if (fireInput.action.ReadValue<float>() > 0)
        {
            currentGun.Fire();


        }

        if (currentGun.CurrentAmmo <= 0)
        {
            RemoveCurrentGun();
        }
    }



    public void RefreshAmmunition(BaseGun aNewGun)
    {
        currentGun.RefillCurrentAmmo(aNewGun.ammunitionAmm / 2);
    }


    public void EquiptNewWeapon(BaseGun aNewGun)
    {
        if (currentGun != null)
        {
            if (currentGun == aNewGun)
            {
                RefreshAmmunition(aNewGun);
                return;
            }
        }
        else
            currentGun = aNewGun.Initialize(this, gunPosition, LayerMask.NameToLayer("FPSWeapon"));


    }

    public void RemoveCurrentGun() {
        currentGun.DeleteGun();
        currentGun = null;
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
