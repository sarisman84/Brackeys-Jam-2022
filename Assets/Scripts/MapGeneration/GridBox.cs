using System.Collections.Generic;
using UnityEngine;
public partial class MapGenerator
{
    private class GridBox {
        public List<TurnSegment> possibilities;
        public float weightSum;
        public Socket[] possibleSockets;

        public GridBox(ref TurnSegment[] segments, float weightSum, Socket[] possibleSockets) {
            possibilities = new List<TurnSegment>(segments);
            this.weightSum = weightSum;

            this.possibleSockets = new Socket[6];
            for(int s = 0; s < 6; s++)
                this.possibleSockets[s] = possibleSockets[s];
        }

        public bool SetResult(TurnSegment socket) {
            if (!possibilities.Contains(socket)) return false;//dont collapse on an impossible state

            ForceResult(socket);
            return true;
        }
        public void ForceResult(TurnSegment socket) {
            possibilities.Clear();
            possibilities.Add(socket);

            for (int s = 0; s < 6; s++)
                possibleSockets[s] = socket.GetSocket(s);
        }

        public int OnlyAllow(Socket socket, Direction dir) {//Return true if the box was fully collapsed
            int d = (int)dir;

            //---------- REMOVE SOCKETS ----------------
            //remove every socket in this direction that doesnt fit with one of the given "socket" 
            possibleSockets[d] &= socket;


            //---------- REMOVE TILES ------------------
            int removeCount = 0;
            for (int p = possibilities.Count - 1; p >= 0; p--) {               
                if (!possibleSockets[d].HasFlag(possibilities[p].GetSocket(d))) {//if the possible sockets dont have this socket -> remove possibility
                    weightSum -= possibilities[p].GetWeight();//update weight to make still correct calculations
                    possibilities.RemoveAt(p);
                    removeCount++;
                }
            }

            if (possibilities.Count == 0) {
                Debug.LogError("ALL possibilities for this tile have been removed");
            }
            return removeCount;
        }


        public int GetWeightedRnd() {
            float rnd = Random.Range(0.0f, weightSum);

            float current = 0;
            for (int i = 0; i < possibilities.Count; i++) {
                current += possibilities[i].GetWeight();
                if (current > rnd)
                    return i;
            }

            Debug.LogError("Error with Weighted Randomness");
            return 0;
        }
    }
}
