using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHandler : MonoBehaviour
{
    public Transform gunPosition;
    [Space]
    public InputActionReference fireInput;
    public InputActionReference interactInput;


    private BaseGun currentGun;
    private Camera mainCam;
    private GameObject currentGunModel;
    private float currentFireRate;
    private float currentAmmoCount;

    void Start()
    {
        mainCam = Camera.main;
    }


    void Update()
    {

        if (currentGun == null)
        {
            if (Intersect(out var hitInfo, 10.0f) && hitInfo.collider.GetComponent<Pickup>() is Pickup pickup && interactInput.action.ReadValue<float>() > 0)
            {
                currentGun = pickup.assignedGun;
                VisualiseGun(currentGun);
                currentFireRate = 0;
                pickup.gameObject.SetActive(false);
                currentAmmoCount = currentGun.ammunitionAmm;
            }
        }
        else
        {
            currentFireRate += Time.deltaTime;
            Intersect(out var hitInfo);
            if (fireInput.action.ReadValue<float>() > 0 && currentFireRate >= currentGun.fireRate)
            {
                currentGun.OnGunUse(hitInfo, currentGunModel, this);
                currentFireRate = 0;
                currentAmmoCount--;


                if (currentAmmoCount <= 0)
                {
                    currentGun = null;
                    ClearVisualisation();
                }
            }
        }


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

    bool Intersect(out RaycastHit hitInfo, float distance = -1f, bool showDebug = false)
    {
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        bool intersect = Physics.Raycast(ray, out hitInfo, distance < 0 ? float.MaxValue : distance);
        if (showDebug)
            Debug.DrawRay(ray.origin, ray.direction, intersect ? Color.green : Color.red);
        return intersect;
    }

    private void OnEnable()
    {
        interactInput.action.Enable();
        fireInput.action.Enable();
    }

    private void OnDisable()
    {
        interactInput.action.Disable();
        fireInput.action.Disable();
    }



}
