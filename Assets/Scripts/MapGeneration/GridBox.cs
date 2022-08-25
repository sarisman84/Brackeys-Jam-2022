using System;
using System.Collections.Generic;
using UnityEngine;
public partial class MapGenerator
{
    private class GridBox {
        public List<TurnSegment> possibilities;
        public float weightSum;
        public HashSet<Socket>[] possibleSockets;

        public GridBox(ref TurnSegment[] segments, float weightSum, List<Socket>[] possibleSockets) {
            possibilities = new List<TurnSegment>(segments);
            this.weightSum = weightSum;

            this.possibleSockets = new HashSet<Socket>[6];
            for(int d = 0; d < 6; d++)
                this.possibleSockets[d] = new HashSet<Socket>(possibleSockets[d]);
        }


        public bool SetResult(TurnSegment socket) {
            if (!possibilities.Contains(socket)) return false;//dont collapse on an impossible state

            ForceResult(socket);
            return true;
        }
        public void ForceResult(TurnSegment socket) {
            possibilities.Clear();
            possibilities.Add(socket);

            ReloadPossibleSockets();
        }


        //Return true if the box was fully collapsed
        public int OnlyAllow(HashSet<Socket> sockets, Direction dir)
        {
            int d = (int)dir;

            //---------- REMOVE SOCKETS ----------------
            //remove every socket in this direction that doesnt fit with one of the given "sockets"
            possibleSockets[d].RemoveWhere(s => !s.MatchesToOneOf(sockets));


            //---------- REMOVE TILES ------------------
            int removeCount = 0;
            for (int p = possibilities.Count - 1; p >= 0; p--) {           
                if (!possibleSockets[d].Contains(possibilities[p].GetSocket(d))) {//if the possible sockets dont have this socket -> remove possibility
                    weightSum -= possibilities[p].GetWeight();//update weight to make still correct calculations
                    possibilities.RemoveAt(p);
                    removeCount++;
                }
            }

            //update sockets from possible changes of the tiles
            ReloadPossibleSockets();

            if (possibilities.Count == 0) {
                string exception = "ALL possibilities for this tile have been removed";
                Debug.LogWarning(exception);
                throw new Exception(exception);
            }
            return removeCount;
        }


        public void ReloadPossibleSockets() {
            for (int d = 0; d < 6; d++) {
                possibleSockets[d].Clear();
                for(int p = 0; p < possibilities.Count; p++) {
                    if (!possibleSockets[d].Contains(possibilities[p].GetSocket(d)))
                         possibleSockets[d].Add(     possibilities[p].GetSocket(d));
                }
            }  
        }


        public int GetWeightedRnd() {
            float rnd = UnityEngine.Random.Range(0.0f, weightSum);

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
