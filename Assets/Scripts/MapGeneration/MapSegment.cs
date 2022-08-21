using UnityEngine;

//[System.Flags]
public enum Socket {
    Wall,
    Path,
    Door
}

[CreateAssetMenu(fileName = "New Map Segment", menuName = "Map/MapSegment")]
public class MapSegment : ScriptableObject
{
    public GameObject prefab;

    public Socket[] sockets;//length 6
    //order: Right, Top, Front, Left, Bottom, Back
}
