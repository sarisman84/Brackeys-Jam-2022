using UnityEngine;

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
}

[CreateAssetMenu(fileName = "New Map Segment", menuName = "Map/MapSegment")]
public class MapSegment : ScriptableObject
{
    public enum Turnable { Full = 4, Once = 2, None = 1 }

    public GameObject prefab;
    public float weight = 1.0f;

    public Socket3D socket;
    public Turnable turnInstances = Turnable.Full;
}


public class TurnSegment{
    public MapSegment segment;
    public int turn = 0;//how many 90° turns around the y-Axis we have
    public float weightMultiplier = 1.0f;
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

    public Socket GetSocket(int d) {
        return GetSocket((Direction)d);
    }
    public Socket GetSocket(Direction dir) {
        return segment.socket.GetSocket((int)DirExt.Turn(dir, turn));
    }

    public bool Fits(Socket3D other) {
        return GetSocket(Direction.Top)    == other.top    &&
               GetSocket(Direction.Right)  == other.right  &&
               GetSocket(Direction.Front)  == other.front  &&
               GetSocket(Direction.Bottom) == other.bottom &&
               GetSocket(Direction.Left)   == other.left   &&
               GetSocket(Direction.Back)   == other.back;
    }

    public Socket3D GetTurnedSocket3D() {
        Socket3D socket = new Socket3D();
        socket.top = GetSocket(Direction.Top);
        socket.right = GetSocket(Direction.Right);
        socket.front = GetSocket(Direction.Front);
        socket.bottom = GetSocket(Direction.Bottom);
        socket.left = GetSocket(Direction.Left);
        socket.back = GetSocket(Direction.Back);
        return socket;
    }


    public bool IsEmpty() { return segment.socket.IsCollisionOnly(); }
}