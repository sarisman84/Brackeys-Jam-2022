using UnityEngine;
using System.Collections;
using System;
using Unity.VisualScripting;

public class Spawn : MonoBehaviour
{
    public enum SpawnType { SpawnOnAwake, SpawnOnPlayerNear }

    public GameObject prefab;
    public Bounds spawnArea;
    public float timeDelay = 0.0f;

    [Range(0.0f, 1.0f)]
    public float probability = 0.5f;

    [Space]
    public SpawnType spawnType = SpawnType.SpawnOnAwake;
    public float playerProximity;


    public event Action<GameObject> OnSpawn;
    private bool spawned = false;

    private void Awake()
    {
        if (UnityEngine.Random.value > probability)
            return;

        if(spawnType == SpawnType.SpawnOnAwake)
            StartCoroutine(SpawnOj(timeDelay));
    }

    private void Update() {
        if (spawnType != SpawnType.SpawnOnPlayerNear)
            return;

        float playerDistance = Vector3.Distance(transform.position, PollingStation.Instance.player.transform.position);
        if (playerDistance <= playerProximity) {
            StartCoroutine(SpawnOj(timeDelay));
        }
    }

    public IEnumerator SpawnOj(float delay)
    {
        if (spawned) yield break;
        spawned = true;

        yield return new PausableWaitForSeconds(delay);

        //Vector3 pos = transform.localToWorldMatrix * spawnArea.GetRndm();
        Vector3 pos = transform.TransformPoint(spawnArea.GetRndm());
        GameObject GO = Instantiate(prefab, pos, Quaternion.identity, PollingStation.Instance.gameManager.GetEntityParent());
        OnSpawn?.Invoke(GO);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerProximity);

        Gizmos.color = new Color(0.1f, 0.6f, 0.2f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(spawnArea.center, spawnArea.size);
    }
}
