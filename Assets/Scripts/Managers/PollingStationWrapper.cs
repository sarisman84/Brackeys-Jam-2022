using UnityEngine;

public class PollingStationWrapper : MonoBehaviour
{
    public void GameManager_OnPlayerExitMap() {
        PollingStation.Instance.gameManager.OnPlayerExitMap();
    }
}
