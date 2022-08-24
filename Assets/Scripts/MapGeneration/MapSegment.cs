using System.Linq;
using UnityEngine;


[System.Serializable]
public struct Neighbour3D {
    public TurnSegment[] top;
    public TurnSegment[] right;
    public TurnSegment[] front;
    public TurnSegment[] bottom;
    public TurnSegment[] left;
    public TurnSegment[] back;

    public TurnSegment[] GetNeighbours(Direction d) { return GetNeighbours((int)d); }
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

    public TurnSegment[] GetTurnedNeighbours(Direction d, int turn) {
        return GetNeighbours(d.Turn(turn));
    }
}


[CreateAssetMenu(fileName = "New Map Segment", menuName = "Map/MapSegment")]
public class MapSegment : ScriptableObject
{
    public enum Turnable { Full = 4, Once = 2, None = 1 }//uses indices for indicating the turn value

    public GameObject prefab;
    public float weight = 1.0f;

    [Space]

    public Turnable turnInstances = Turnable.Full;
    [Space]
    public Neighbour3D whitelist;

    public bool IsEmpty() { return this == PollingStation.Instance.mapGenerator.empty; }
}

[System.Serializable]
public struct TurnSegment {
    public MapSegment segment;
    public int turn;//how many 90° turns around the y-Axis we have (anti-clockwise)
    public float weightMultiplier {get; set;}
    public float GetWeight() { return weightMultiplier * segment.weight; }

    public TurnSegment(MapSegment segment, int turn, float weightMul) {
        this.segment = segment;
        this.turn = turn%4;
        this.weightMultiplier = weightMul;
    }
    public TurnSegment(MapSegment segment, int turn) {
        this.segment = segment;
        this.turn = turn%4;
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


    public bool IsEmpty() { return segment.IsEmpty(); }

    public bool Equals(TurnSegment other) {
        return segment == other.segment &&
               turn%4  == other.turn%4;
    }

    public override string ToString() {
        return segment.name + "; t: " + turn;
    }
}