using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/*
[System.Serializable]
public struct Socket3D {
    public Socket top;
    public Socket right;
    public Socket front;
    public Socket bottom;
    public Socket left;
    public Socket back;


    //length 6
    //order: Top, Right, Front, Bottom, Left, Back
    public Socket GetSocket(int i) {
        switch (i) {
            case 0: return top;
            case 1: return right;
            case 2: return front;
            case 3: return bottom;
            case 4: return left;
            case 5: return back;
            default: return Socket.Wall;
        }
    }

    public void SetSocket(int i, Socket s) {
        switch (i) {
            case 0:
                top = s;
                return;
            case 1:
                right = s;
                return;
            case 2:
                front = s;
                return;
            case 3:
                bottom = s;
                return;
            case 4:
                left = s;
                return;
            case 5:
                back = s;
                return;
        }
    }

    public Socket3D Clone() {
        Socket3D s = new Socket3D();
        s.top = top;
        s.right = right;
        s.front = front;
        s.bottom = bottom;
        s.left = left;
        s.back = back;
        return s;
    }

    public bool Equals(Socket3D other) {
        return top   == other.top   &&
               right == other.right &&
               front == other.front &&
               bottom== other.bottom&&
               left  == other.left  &&
               back  == other.back;
    }

    public bool IsCollisionOnly() {
        return top.IsCollision() &&
               right.IsCollision() &&
               front.IsCollision() &&
               bottom.IsCollision() &&
               left.IsCollision() &&
               back.IsCollision();
    }

    public override string ToString() {
        return "{ Top:"+top + "; Right:"+right+ "; Front:" + front + "; Bottom:" + bottom + "; Left:" + left + "; Back:" + back + " }";
    }
}*/


[System.Serializable]
public struct Neighbour3D {
    public TurnSegment[] top;
    public TurnSegment[] right;
    public TurnSegment[] front;
    public TurnSegment[] bottom;
    public TurnSegment[] left;
    public TurnSegment[] back;

    public TurnSegment[] GetNeighbours(int i) {
        switch (i) {
            case 0: return top;
            case 1: return right;
            case 2: return front;
            case 3: return bottom;
            case 4: return left;
            case 5: return back;
            default: return null;
        }
    }

    public void SetNeighbours(int i, TurnSegment[] n) {
        switch (i) {
            case 0: top = n;   return;
            case 1: right = n; return;
            case 2: front = n; return;
            case 3: bottom = n;return;
            case 4: left = n;  return;
            case 5: back = n;  return;
        }
    }

    public bool IsEmpty() {
        return top.Length == 0 &&
               right.Length == 0 &&
               front.Length == 0 &&
               bottom.Length == 0 &&
               left.Length == 0 &&
               back.Length == 0;
    }
    public bool IsEmpty(int d) { return GetNeighbours(d).Length == 0; }
}


[CreateAssetMenu(fileName = "New Map Segment", menuName = "Map/MapSegment")]
public class MapSegment : ScriptableObject
{
    public enum Turnable { Full = 4, Once = 2, None = 1 }

    public GameObject prefab;
    public float weight = 1.0f;

    //public Socket3D socket;

    [Space]

    public Turnable turnInstances = Turnable.Full;

    [Space]
    public Neighbour3D whitelist;
}

[System.Serializable]
public struct TurnSegment {
    public MapSegment segment;
    public int turn;//how many 90° turns around the y-Axis we have
    public float weightMultiplier {get; set;}
    public float GetWeight() { return weightMultiplier * segment.weight; }

    public TurnSegment(MapSegment segment, int turn, float weightMul) {
        this.segment = segment;
        this.turn = turn;
        this.weightMultiplier = weightMul;
    }
    public TurnSegment(MapSegment segment, int turn) {
        this.segment = segment;
        this.turn = turn;
        this.weightMultiplier = 1.0f;
    }

    public Quaternion GetRot() {
        return Quaternion.Euler(0, turn*90, 0);
    }

    public TurnSegment[] GetNeighbours(int d) {
        return GetNeighbours((Direction)d);
    }
    public TurnSegment[] GetNeighbours(Direction dir) {
        return segment.whitelist.GetNeighbours((int)DirExt.Turn(dir, turn));
    }

    public bool Fits(TurnSegment[] neighbours) {
        return GetNeighbours(Direction.Top   ).Contains(neighbours[0]) &&
               GetNeighbours(Direction.Right ).Contains(neighbours[1]) &&
               GetNeighbours(Direction.Front ).Contains(neighbours[2]) &&
               GetNeighbours(Direction.Bottom).Contains(neighbours[3]) &&
               GetNeighbours(Direction.Left  ).Contains(neighbours[4]) &&
               GetNeighbours(Direction.Back  ).Contains(neighbours[5]);
    }

    public Neighbour3D GetTurnedNeighbour3D() {
        Neighbour3D neighbours = new Neighbour3D();
        neighbours.top    = GetNeighbours(Direction.Top);
        neighbours.right  = GetNeighbours(Direction.Right);
        neighbours.front  = GetNeighbours(Direction.Front);
        neighbours.bottom = GetNeighbours(Direction.Bottom);
        neighbours.left   = GetNeighbours(Direction.Left);
        neighbours.back   = GetNeighbours(Direction.Back);
        return neighbours;
    }


    public bool IsEmpty() { return segment.whitelist.IsEmpty(); }

    public bool Equals(TurnSegment other) {
        return segment == other.segment &&
               turn    == other.turn;
    }
}