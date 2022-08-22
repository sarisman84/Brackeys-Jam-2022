using UnityEngine;

public enum Socket {
    Wall,
    Path,
    Door
}

[CreateAssetMenu(fileName = "New Map Segment", menuName = "Map/MapSegment")]
public class MapSegment : ScriptableObject
{
    public GameObject prefab;
    public float weight = 1.0f;

    [Space]
    [Header("Sockets")]
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
            case 0: 
                return top; 
            case 1:
                return right;
            case 2:
                return front;
            case 3:
                return bottom;
            case 4:
                return left;
            case 5:
                return back;
            default:
                return Socket.Wall;
        }
    }
}


public class TurnSegment{
    public MapSegment segment;
    public int turn = 0;//how many 90° turns around the y-Axis we have

    public TurnSegment(MapSegment segment, int turn) {
        this.segment = segment;
        this.turn = turn;
    }

    public Quaternion GetRot() {
        return Quaternion.Euler(0, turn*90, 0);
    }

    public Socket GetSocket(int i) {
        return segment.GetSocket((int)DirExt.Turn((Direction)i, turn));
    }
}