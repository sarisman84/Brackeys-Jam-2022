using System.Collections.Generic;
using UnityEngine;
public partial class MapGenerator
{
    private class GridBox {
        public List<MapSegment> possibilities;
        public bool propergated = false;

        public GridBox(ref MapSegment[] segments) {
            possibilities = new List<MapSegment>(segments);
        }

        public bool SetResult(MapSegment socket) {
            if (!possibilities.Contains(socket)) return false;//dont collapse on an impossible state

            ForceResult(socket);
            return true;
        }
        public void ForceResult(MapSegment socket) {
            possibilities.Clear();
            possibilities.Add(socket);
        }

        public void OnlyAllow(Socket socket, Direction dir) {//Return true if the box was fully collapsed
            int removeCount = 0;
            int d = (int)dir;
            for(int p = possibilities.Count-1; p >= 0; p--) {
                if (possibilities[p].GetSocket(d) != socket) {
                    possibilities.RemoveAt(p);
                    removeCount++;
                }
            }

            if (possibilities.Count == 0) {
                Debug.LogWarning("ALL possibilities for this tile have been removed");
                ForceResult(instance.empty);
            } 
        }
    }
}
