using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public partial class MapGenerator
{
    private class GridBox
    {
        public List<TurnSegment> possibilities;
        public float weightSum;
        public List<TurnSegment>[] possibleNeighbours;
        public bool PossibilitiesAllow(int dir, TurnSegment[] oneOfThese)
        {
            for (int pn = 0; pn < possibleNeighbours[dir].Count; pn++)
            {
                for (int i = 0; i < oneOfThese.Length; i++)
                {
                    if (possibleNeighbours[dir][pn].Equals(oneOfThese[i]))
                        return true;
                }
            }
            return false;
        }

        public GridBox(ref TurnSegment[] segments, float weightSum, ref TurnSegment[] allSegments)
        { //Socket[] possibleSockets) {
            possibilities = new List<TurnSegment>(segments);
            this.weightSum = weightSum;

            possibleNeighbours = new List<TurnSegment>[6];
            for (int d = 0; d < 6; d++)
                possibleNeighbours[d] = new List<TurnSegment>(allSegments);
        }

        public bool SetResult(TurnSegment segment)
        {
            if (!possibilities.Contains(segment)) return false;//dont collapse on an impossible state

            ForceResult(segment);
            return true;
        }
        public void ForceResult(TurnSegment segment)
        {
            possibilities.Clear();
            possibilities.Add(segment);

            //set the only possibility -> the possible neighbours are going to be also from that possibility
            for (int d = 0; d < 6; d++)
                possibleNeighbours[d] = new List<TurnSegment>(segment.GetNeighbours(d));
        }

        //Return true if the box was fully collapsed
        public int OnlyAllow(List<TurnSegment> allowedNeighbours, Direction dir)//only allow these neighbours in this direction
        {
            int d = (int)dir;

            //---------- REMOVE POSSIBILITIES -----------------
            //remove every tile in this direction that doesnt fit with one of the given tiles
            for (int n = 0; n < possibleNeighbours[d].Count; n++)
            {
                //Apparently Contains check fails (cant find allowedNeighbours), using a mega scuffed comparision check instead - Spyro
                //Nvm still doesnt work... something is wrong here - Spyro

                if (allowedNeighbours.Find(t => possibleNeighbours[d][n].segment && t.segment.GetInstanceID() == possibleNeighbours[d][n].segment.GetInstanceID() && t.turn == possibleNeighbours[d][n].turn).segment == null)
                {
                    Debug.Log($"Removing Possibile Neighbour ({possibleNeighbours[d][n].segment.prefab.gameObject.name})");
                    possibleNeighbours[d].RemoveAt(n);
                }

            }


            //---------- REMOVE TILES ------------------
            int removeCount = 0;
            for (int p = possibilities.Count - 1; p >= 0; p--)
            {
                if (!PossibilitiesAllow(d, possibilities[p].GetNeighbours(d)))
                {//if the possible tiles dont have this tile -> remove possibility
                    weightSum -= possibilities[p].GetWeight();//update weight to make still correct calculations

                    Debug.Log($"Removing Possibility ({possibilities[p].segment.prefab.gameObject.name})");
                    possibilities.RemoveAt(p);

                    removeCount++;
                }
            }

            if (possibilities.Count == 0)
            {
                string exception = "ALL possibilities for this tile have been removed";
                Debug.LogWarning(exception);
                throw new Exception(exception);
            }
            return removeCount;
        }


        public int GetWeightedRnd()
        {
            float rnd = UnityEngine.Random.Range(0.0f, weightSum);

            float current = 0;
            for (int i = 0; i < possibilities.Count; i++)
            {
                current += possibilities[i].GetWeight();
                if (current > rnd)
                    return i;
            }

            Debug.LogError("Error with Weighted Randomness");
            return 0;
        }
    }
}
