using UnityEngine;

public class TileDecorator : MonoBehaviour
{
    public SpawnOption[] tileDecorations;//TODO; add a custom editor to display decorations, to edit them better
    public float weightOffset = 0.0f;//weight of nothing being spawned

    public float floorOffset = 1.5f;

    // 4 / 5 = 0.8
    //if value >= 0.8 - Spawn something
    //0 -> 0.8 to spawn something and you have a 0.2 chance to not spawn anything
    private void Awake()
    {   
        if(tileDecorations.Length > 0) {
            int i = UnityExtensions.GetWeightedRnd(tileDecorations, SpawnOption.GetWeight, weightOffset);
            if(i >= 0)
                InsertDecoration(i);
        }            
    }

    private void InsertDecoration(int index)
    {
        if (tileDecorations[index].prefab == null) return;

        var obj = Instantiate(tileDecorations[index].prefab, transform);
        obj.transform.localPosition = Vector3.down * floorOffset;
        obj.transform.localRotation = tileDecorations[index].GetRotation();
    }
}
