using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class TileDecorator : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float probabilityToNotSpawnAnything = 0.2f;
    public List<GameObject> tileDecorations;


    float AddedProbability()
    {
        float probability = probabilityToNotSpawnAnything * tileDecorations.Count / (1 - probabilityToNotSpawnAnything);
        return probability;
    }


    // 4 / 5 = 0.8
    //if value >= 0.8 - Spawn something
    //0 -> 0.8 to spawn something and you have a 0.2 chance to not spawn anything
    private void Awake()
    {

        float i = Random.Range(0.0f, tileDecorations.Count + AddedProbability());
        if (i < tileDecorations.Count)
            InsertDecoration(Mathf.FloorToInt(i));
    }

    private void InsertDecoration(int index)
    {
        var obj = Instantiate(tileDecorations[index], transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }
}
