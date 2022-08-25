using UnityEngine;

[System.Serializable]
public class Socket3D {

    //length 6
    //order: Top, Right, Front, Bottom, Left, Back
    public Socket[] sockets { get; set; }
    public Socket3D() {
        sockets = new Socket[6];
    }

    public Socket GetSocket(int i) { return sockets[i]; }
    public void SetSocket(int i, Socket s) { sockets[i] = s; }

    public Socket3D Clone() {
        Socket3D s = new Socket3D();
        s.sockets = new Socket[6];
        for (int d = 0; d < 6; d++)
            s.sockets[d] = sockets[6];
        return s;
    }

    public bool Equals(Socket3D other) {
        for (int d = 0; d < 6; d++)
            if (sockets[d] != other.sockets[d])
                return false;
        return true;
    }

    public bool IsCollisionOnly() {
        for (int d = 0; d < 6; d++)
            if (!sockets[d].isCollision)
                return false;
        return true;
    }

    public override string ToString() {
        string str = "{";
        for (int d = 0; d < 6; d++)
            str += $" {(Direction)d}: {sockets[d]} ;";
        return str + "}";
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
        for (int d = 0; d < 6; d++)
            if (!GetSocket(d).Matches(other.sockets[d]))
                return false;
        return true;
    }

    public Socket3D GetTurnedSocket3D() {
        Socket3D socket = new Socket3D();
        for (int d = 0; d < 6; d++)
            socket.sockets[d] = GetSocket(d);
        return socket;
    }


    public bool IsEmpty() { return segment.socket.IsCollisionOnly(); }
}