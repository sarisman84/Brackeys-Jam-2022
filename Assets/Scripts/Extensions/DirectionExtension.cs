using Unity.VisualScripting;
using UnityEngine;


public enum Direction { Top, Right, Front, Bottom, Left, Back }
public static class DirExt
{
    public static Vector3Int[] directions = {Vector3Int.up, Vector3Int.right, Vector3Int.forward, Vector3Int.down, Vector3Int.left, Vector3Int.back };
    public static int InvertDir(int d) { return (d + 3) % 6; }
    public static Direction InvertDir(this Direction d) { return (Direction)InvertDir((int)d); }

    public static Direction Turn(this Direction d, int turn) {//clockwise
        turn = turn % 4;
        if (d == Direction.Top || d == Direction.Bottom)
            return d;
        
        int d_ = (int)d;
        d_ = (d_ + (d_ % 3) * (turn % 2) + 3 * (turn / 2)) % 6;//s += mod3 if s=1 or 3     s += 3 if s>1 (inverse)
        return (Direction)d_;//right -> front -> left -> back
    }

    public static Direction ToDir(Vector3Int dir) {
        for (int d = 0; d < directions.Length; d++) {
            if (dir == directions[d])
                return (Direction)d;
        }
        Debug.LogError("Vector could not be converted to Direction");
        return Direction.Top;
    }
}
