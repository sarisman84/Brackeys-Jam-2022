using UnityEngine;

public enum Direction { Top, Right, Front, Bottom, Left, Back }
public static class DirExt
{

    public static Vector3Int[] directions = {Vector3Int.up, Vector3Int.right, Vector3Int.forward, Vector3Int.down, Vector3Int.left, Vector3Int.back };
    public static int InvertDir(int d) { return (d + 3) % 6; }

    public static Direction Turn(Direction d, int turn) {
        turn = turn % 4;
        if (d == Direction.Top || d == Direction.Bottom)
            return d;
        
        int d_ = (int)d;
        d_ = (d_ + (d_ % 3) * (turn % 2) + 3 * (turn / 2)) % 6;//s += mod3 if s=1 or 3     s += 3 if s>1 (inverse)
        return (Direction)d_;
    }
}
