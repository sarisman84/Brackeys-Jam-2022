using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Socket", menuName = "Map/Socket")]
public class Socket : ScriptableObject
{
    public Color color = Color.white;
    public bool isCollision = false;
    public Socket[] connectionSockets;//the flipped version of this socket (the same socket for symmetric sockets)

    //NOTE: this wont work for vertical sockets -> new system needed for a 3d collapse algorithm

    public bool MatchesToOneOf(HashSet<Socket> set) {//check of set contains one of the possible neighbouring connection sockets
        for(int cs = 0; cs < connectionSockets.Length; cs++)
            if (set.Contains(connectionSockets[cs]))
                return true;
        return false;
    }
    public bool Matches(Socket s) {//s is one of the possible neighbouring connection sockets
        for (int cs = 0; cs < connectionSockets.Length; cs++)
            if (connectionSockets[cs] == s)
                return true;
        return false;
    }
}