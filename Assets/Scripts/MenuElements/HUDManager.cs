using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public RectTransform HealthDisplay;
    [Space]
    public RectTransform AmmoDisplay;
    public TextMeshProUGUI maxAmmoText;
    public TextMeshProUGUI currentAmmoText;

    private HealthHandler healthHandler;
    private WeaponManager weaponManager;
    private void Start() {
        healthHandler = PollingStation.Instance.player.GetComponent<HealthHandler>();
        weaponManager = PollingStation.Instance.weaponManager;

        PollingStation.Instance.runtimeManager.onPostStateChangeCallback += (RuntimeManager.RuntimeState previousState, RuntimeManager.RuntimeState state) =>
        {
            if(state == RuntimeManager.RuntimeState.Playing || state == RuntimeManager.RuntimeState.GameOver) {
                SetHUDActive(true);
            }
            else {
                SetHUDActive(false);
            }
        };
    }

    private void Update() {
        UpdateHUD();
        //Note: Optimization can be done by only fetching the needed info when they change by using a callback
    }

    public void SetHUDActive(bool active) {
        HealthDisplay.gameObject.SetActive(active);
        UpdateHUD();
    }


    public void SetHealthUI(float healthPercentage) {
        Transform healthSegmentParent = HealthDisplay.GetChild(0);
        int healthSegmentCount = healthSegmentParent.childCount;
        for(int i = 0; i < healthSegmentCount; i++) {
            float segmentPosition = (i + 1.0f) / healthSegmentCount;
            healthSegmentParent.GetChild(i).GetChild(0).gameObject.SetActive(segmentPosition <= healthPercentage);
        }
    }
    public void SetAmmoUI(int maxAmmo, int currentAmmo) {
        maxAmmoText.text = "/ " + maxAmmo.ToString();
        currentAmmoText.text = currentAmmo.ToString();
    }

    public void UpdateHUD() {
        SetHealthUI(healthHandler.currentHealth / healthHandler.maxHealth);

        if(weaponManager.currentGun != null) {
            AmmoDisplay.gameObject.SetActive(true);
            SetAmmoUI(weaponManager.currentGun.template.ammunitionAmm, weaponManager.currentGun.CurrentAmmo);
        }
        else {
            AmmoDisplay.gameObject.SetActive(false);
        }
        
    }
}
