using System.Collections.Generic;
using UnityEngine;
public partial class MapGenerator
{
    private class GridBox {
        public List<TurnSegment> possibilities;
        public bool propergated = false;
        public float weightSum;

        public GridBox(ref TurnSegment[] segments, float weightSum) {
            possibilities = new List<TurnSegment>(segments);
            this.weightSum = weightSum;
        }

        public bool SetResult(TurnSegment socket) {
            if (!possibilities.Contains(socket)) return false;//dont collapse on an impossible state

            ForceResult(socket);
            return true;
        }
        public void ForceResult(TurnSegment socket) {
            possibilities.Clear();
            possibilities.Add(socket);
        }

        public void OnlyAllow(Socket socket, Direction dir) {//Return true if the box was fully collapsed
            int removeCount = 0;
            int d = (int)dir;
            for(int p = possibilities.Count-1; p >= 0; p--) {
                if (possibilities[p].GetSocket(d) != socket) {
                    weightSum -= possibilities[p].segment.weight;//update weight to make still correct calculations
                    possibilities.RemoveAt(p);
                    removeCount++;
                }
            }

            if (possibilities.Count == 0) {
                Debug.LogWarning("ALL possibilities for this tile have been removed");
                ForceResult(instance.emptySegment);
            } 
        }


        public int GetWeightedRnd() {
            float rnd = Random.Range(0.0f, weightSum);

            float current = 0;
            for (int i = 0; i < possibilities.Count; i++) {
                current += possibilities[i].segment.weight;
                if (current > rnd)
                    return i;
            }

            Debug.LogError("Error with Weighted Randomness");
            return 0;
        }
    }
}
