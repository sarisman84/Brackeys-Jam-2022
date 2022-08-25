using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
    public GameObject prefab;
    public Bounds spawnArea;
    public float timeDelay = 0.0f;

    public float probability = 0.5f;

    private void Start()
    {

        if (Random.value >= probability)
        {
            return;
        }

        StartCoroutine(SpawnOj(timeDelay));
    }

    public IEnumerator SpawnOj(float delay)
    {
        yield return new WaitForSeconds(delay);
        //Vector3 pos = transform.localToWorldMatrix * spawnArea.GetRndm();
        Vector3 pos = transform.TransformPoint(spawnArea.GetRndm());
        Instantiate(prefab, pos, Quaternion.identity, PollingStation.Instance.gameManager.GetEntityParent());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.1f, 0.6f, 0.2f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(spawnArea.center, spawnArea.size);
    }
}
