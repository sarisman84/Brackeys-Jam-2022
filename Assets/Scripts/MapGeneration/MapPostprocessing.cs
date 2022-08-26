using System.Collections.Generic;
using UnityEngine;

public partial class MapGenerator
{
    [Header("Post Processing")]
    public Socket[] connectionSockets;

    private struct MapSections {
        public List<List<int>> sections;

        #region Find Sections
        public void GenerateSections(ref Array3D<TurnSegment> map) {
            HashSet<int> visited = new HashSet<int>();
            for (int i = 0; i < map.Length; i++) {
                if (map[i].IsEmpty())
                    visited.Add(i);//add all empty tiles as visited
            }
            
            sections = new List<List<int>>();
            bool newSectionFound;
            do {
                newSectionFound = GetSection(ref map, ref visited, out List<int> section);
                if(newSectionFound)
                    sections.Add(section);
            } while (newSectionFound);
        }

        public bool GetSection(ref Array3D<TurnSegment> map, ref HashSet<int> visited, out List<int> section) {
            int init;
            for (init = 0; init <= map.Length; init++) {
                if (!visited.Contains(init))
                    break;
            }
            if(init == map.Length) {
                section = null;
                return false;
            }
                
            section = new List<int>();
            Stack<int> toVisit = new Stack<int>();
            toVisit.Push(init);

            while (toVisit.Count > 0) {
                int i = toVisit.Pop();
                if (visited.Contains(i))
                    continue;

                section.Add(i);
                visited.Add(i);
                Vector3Int pos = map.GetPos(i);

                //Add all connected tiles
                for (int d = 0; d < DirExt.directions.Length; d++) {
                    Vector3Int neighbour = pos + DirExt.directions[d];
                    if (!map.InBounds(neighbour)) continue;

                    if (map[i].GetSocket(d).isCollision) continue;//only add connected

                    int _i = map.GetIndex(neighbour);
                    if (visited.Contains(_i)) continue;//only add not visited
                    
                    toVisit.Push(_i);
                    section.Add(_i);
                }
            }
            return true;
        }
        #endregion


        #region Fix Sections
        public void ConnectSections(ref Array3D<TurnSegment> map) {
            int maxIter = 0;

            //------------ FIND ALL NEIGHBOURS OF EACH SECTION -------------------
            List<(int, int)>[] neighbours = new List<(int, int)>[sections.Count];
            for (int s = 0; s < sections.Count; s++) {
                neighbours[s] = new List<(int, int)>();
                for(int sec_i = 0; sec_i < sections[s].Count; sec_i++) {
                    int i = sections[s][sec_i];
                    Vector3Int pos = map.GetPos(i);

                    for (int d = 0; d < DirExt.directions.Length; d++) {
                        Vector3Int neighbour = pos + DirExt.directions[d];
                        if (!map.InBounds(neighbour)) continue;

                        int _i = map.GetIndex(neighbour);
                        if (map[_i].IsEmpty()) continue;//ignore empty tiles
                        if (sections[s].Contains(_i)) continue;//only add tiles from other sections

                        neighbours[s].Add((i, _i));//add neighbours from different sections
                        maxIter++;
                    }
                }
            }

            
            //------------------ FIND CONNECTIONS ----------------------
            List<(int, int)> connections = new List<(int, int)>();
            {
                List<(int, int)> mainNeighbours = new List<(int, int)>();
                HashSet<int> mainSection = new HashSet<int>();
                HashSet<int> addedSections = new HashSet<int>();
                mainSection.UnionWith(sections[0]);//add the first section as initial section
                addedSections.Add(0);
                mainNeighbours.AddRange(neighbours[0]);
                for (int iter = 0; iter < maxIter; iter++) {
                    if(mainNeighbours.Count == 0) {
                        Debug.LogError("Not all Sections could be connected");
                        break;
                    }

                    //pick a random neighbour from all mainsections
                    int nIndex = Random.Range(0, mainNeighbours.Count);
                    (int self, int neighbour) = mainNeighbours[nIndex];

                    //remove and repeat, if it leads to the main section again
                    if (mainSection.Contains(neighbour)) {
                        mainNeighbours.RemoveAt(nIndex);
                        continue;
                    }

                    connections.Add((self, neighbour));//save connection over this neighbour

                    //add new section to main section
                    int otherSection = GetSection(neighbour);//VERY SLOW
                    mainSection.UnionWith(sections[otherSection]);
                    addedSections.Add(otherSection);
                    mainNeighbours.AddRange(neighbours[otherSection]);

                    if (addedSections.Count == sections.Count) {
                        Debug.Log("All connections found successfully");
                        break;
                    } 
                }
            }


            //------------------ CONNECT SECTIONS ----------------------
            for (int c = 0; c < connections.Count; c++) {
                (int a, int b) = connections[c];
                Vector3Int vecA = map.GetPos(a);
                Vector3Int vecB = map.GetPos(b);

                //Specify the sockets INCOMING from the surounding tiles
                Socket3D suroundingA = PollingStation.Instance.mapGenerator.GetSuroundingSockets(ref map, vecA);
                Socket3D suroundingB = PollingStation.Instance.mapGenerator.GetSuroundingSockets(ref map, vecB);

                Vector3Int A_ConVec = vecB - vecA;
                if(A_ConVec.sqrMagnitude != 1) {
                    Debug.LogError("Connectiondistance is too big");
                    continue;
                }

                Direction A_ConDir = DirExt.ToDir(A_ConVec);


                //Find a tile, which sockets fit to the specified sockets while making sure that the connection socket is a connection

                //First go through all sockets and find one that fits for A
                Socket[] noColSockets = PollingStation.Instance.mapGenerator.connectionSockets;
                map[a] = PollingStation.Instance.mapGenerator.FindFittingSegment(ref suroundingA, (int)A_ConDir, noColSockets, out Socket B_Socket);

                //then use that socket to find one for B as well
                suroundingB.SetSocket((int)A_ConDir.InvertDir(), B_Socket);
                map[b] = PollingStation.Instance.mapGenerator.FindFittingSegment(ref suroundingB);
            }
        }
        #endregion



        //tries to find the section a given tile [i] is in
        public int GetSection(int i) {
            for(int s = 0; s < sections.Count; s++) {
                if (sections[s].Contains(i))
                    return s;
            }
            Debug.LogWarning($"Section for index {i} not found");
            return 0;
        }

    }

    private Socket3D GetSuroundingSockets(ref Array3D<TurnSegment> map, Vector3Int pos) {
        Socket3D s3d = new Socket3D();
        for(int d = 0; d < DirExt.directions.Length; d++) {
            Vector3Int nPos = pos + DirExt.directions[d];
            Direction connectionDir = ((Direction)d).InvertDir();//direction facing to this tile from the other tile
            s3d.SetSocket(d, map[nPos].GetSocket(connectionDir));
        }
        return s3d;
    }


    private TurnSegment FindFittingSegment(ref Socket3D surounding, int d, Socket[] possibleConnections, out Socket connector) {
        for (int c = 0; c < possibleConnections.Length; c++) {
            connector = possibleConnections[c];
            surounding.SetSocket(d, connector);
            for (int seg = 0; seg < turnSegments.Length; seg++) {
                if (turnSegments[seg].SocketFits(surounding))
                    return turnSegments[seg];
            }
        }

        connector = null;
        //Debug.LogError($"No fitting segment found for {surounding} with all possible connection Sockets");
        throw new System.Exception($"No fitting segment found for {surounding} with all possible connection Sockets");
        //return null;
    }
    private TurnSegment FindFittingSegment(ref Socket3D surounding) {
        for (int s = 0; s < turnSegments.Length; s++) {
            if (turnSegments[s].SocketFits(surounding))
                return turnSegments[s];
        }
        //Debug.LogError($"No fitting segment found for {surounding}");
        //return null;
        throw new System.Exception($"No fitting segment found for {surounding}");
    }
}
