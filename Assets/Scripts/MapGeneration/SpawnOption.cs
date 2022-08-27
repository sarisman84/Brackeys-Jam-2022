using UnityEngine;

[CreateAssetMenu(fileName = "New SpawnOption", menuName = "SpawnOption")]
public class SpawnOption : ScriptableObject
{
    public GameObject prefab;
    public float weight = 1.0f;
    public int turn;

    public Quaternion GetRotation() { return Quaternion.Euler(0, 90*turn, 0); }
    public static float GetWeight(SpawnOption SO) { return SO.weight; }
}
