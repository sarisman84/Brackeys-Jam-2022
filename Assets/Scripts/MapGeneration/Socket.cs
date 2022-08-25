using UnityEngine;

[CreateAssetMenu(fileName = "New Socket", menuName = "Map/Socket")]
public class Socket : ScriptableObject
{
    public Color color;
    public bool isCollision = false;
}
