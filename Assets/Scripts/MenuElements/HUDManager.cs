using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public RectTransform healthSegmentParent;
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
            if(state == RuntimeManager.RuntimeState.Playing) {
                gameObject.SetActive(true);
                UpdateHUD();//removes flickering caused by updating too late
            }
            else {
                gameObject.SetActive(false);
            }
        };

        gameObject.SetActive(false);
    }

    private void Update() {
        UpdateHUD();
        //Note: Optimization can be done by only fetching the needed info when they change by using a callback
    }



    public void SetHealthUI(float healthPercentage) {
        int healthSegmentCount = healthSegmentParent.childCount;
        for(int i = 0; i < healthSegmentCount; i++) {
            float segmentPosition = (float)i / healthSegmentCount;
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
