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
        public bool ArePossibleNeighboursAllowed(int dir, List<TurnSegment> possibleNeighbours)
        {
            for (int p = 0; p < possibilities.Count; p++)
            {
                for(int n = 0; n < possibilities[p].GetNeighbours(dir).Length; n++) {
                    TurnSegment turnSegment = possibilities[p].GetNeighbours(dir)[n];
                    for (int pn = 0; pn < possibleNeighbours.Count; pn++) {
                        //if the current focused neighbour in this loop is actually part of our neighbour -> pieces fit together (returns true)
                        if (turnSegment.Equals(possibleNeighbours[pn]))
                            return true;
                    }
                }
                
            }
            return false;
        }

        public GridBox(ref TurnSegment[] segments, float weightSum)
        {
            possibilities = new List<TurnSegment>(segments);
            this.weightSum = weightSum;
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
        }

        //What we know:
        /*
         >We start with every possibility
         >We have a weight sum
         */


        //Return true if the box was fully collapsed
        public int OnlyAllow(List<TurnSegment> neighbourPossibilities, Direction dir)//these neighbourPossibilities are in this direction
        {
            int d = (int)dir;

            /*
            //---------- REMOVE POSSIBILITIES -----------------
            //remove every tile in this direction that doesnt fit with one of the given tiles
            for (int n = 0; n < possibleNeighbours[d].Count; n++)
            {
                //if(!allowedNeighbours.Contains(possibleNeighbours[n][d]))
                //      possibleNeighbours[d].RemoveAt(n);


                //Apparently Contains check fails (cant find allowedNeighbours), using a mega scuffed comparision check instead - Spyro
                //Nvm still doesnt work... something is wrong here - Spyro
                if (allowedNeighbours.Find(t => possibleNeighbours[d][n].segment && t.segment.GetInstanceID() == possibleNeighbours[d][n].segment.GetInstanceID() && t.turn == possibleNeighbours[d][n].turn).segment == null)
                {
                    Debug.Log($"Removing Possibile Neighbour ({possibleNeighbours[d][n].segment.prefab.gameObject.name})");
                    possibleNeighbours[d].RemoveAt(n);
                }
            }*/


            //collapse this cell, so that every possibility works with the given neighbours

            //---------- REMOVE TILES ------------------
            int removeCount = 0;
            for (int p = possibilities.Count - 1; p >= 0; p--)
            {
                if (!ArePossibleNeighboursAllowed(d, neighbourPossibilities))
                {//if the possible tiles dont have this tile -> remove possibility
                    weightSum -= possibilities[p].GetWeight();//update weight to make still correct calculations

                    Debug.Log($"Removing Invalid Possibility ({possibilities[p].segment.prefab.gameObject.name})");
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
