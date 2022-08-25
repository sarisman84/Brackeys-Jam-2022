using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
    public GameObject prefab;
    public Bounds spawnArea;
    public float timeDelay = 0.0f;

    private void Start() {
        StartCoroutine(SpawnOj(timeDelay));
    }

    public IEnumerator SpawnOj(float delay) {
        yield return new WaitForSeconds(delay);
        Vector3 pos = transform.position + spawnArea.GetRndm();
        Instantiate(prefab, pos, Quaternion.identity, PollingStation.Instance.gameManager.GetEntityParent());
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = new Color(0.1f, 0.6f, 0.2f);
        Gizmos.DrawWireCube(transform.position + spawnArea.center, spawnArea.size);
    }
}
