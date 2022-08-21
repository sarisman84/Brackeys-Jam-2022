using System;
using System.Collections;
using System.Collections.Generic;
public partial class MapGenerator
{
    private class GridBox {
        public int possibilityCount;
        public List<MapSegment> possibilities;
        public bool propergated = false;

        public GridBox(ref MapSegment[] segments) {
            possibilityCount = segments.Length;
            possibilities = new List<MapSegment>(segments);
        }

        public bool SetResult(MapSegment socket) {
            if (!possibilities.Contains(socket)) return false;//dont collapse on an impossible state

            possibilityCount = 1;
            possibilities.Clear();
            possibilities.Add(socket);
            return true;
        }

        public void OnlyAllow(Socket socket, Direction dir) {//Return true if the box was fully collapsed
            int removeCount = 0;
            int d = (int)dir;
            for(int p = possibilities.Count-1; p >= 0; p--) {
                if (possibilities[p].sockets[d] != socket) {
                    possibilities.RemoveAt(p);
                    removeCount++;
                }
            }

            possibilityCount -= removeCount;
        }
    }
}
