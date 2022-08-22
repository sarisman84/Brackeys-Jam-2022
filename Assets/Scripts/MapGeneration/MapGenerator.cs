using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public partial class MapGenerator : MonoBehaviour
{
    public static MapGenerator instance;
    private void Awake() {
        if(instance != null) {
            Debug.LogWarning($"There is more than one {GetType().Name}");
            return;
        }
        instance = this;
    }
    
    public Vector3Int mapSize = new Vector3Int(5, 1, 5);
    public Vector3 tileSize = Vector3.one;


    private Array3D<TurnSegment> map;
    private Array3D<GridBox> grid;//FOR DEBUGGING ONLY

    public MapSegment empty;
    private TurnSegment emptySegment;

    public MapSegment[] segments;
    private TurnSegment[] turnSegments;

    void Start()
    {
        emptySegment = new TurnSegment(empty, 0);

        //generate TurnSegments from the mapSegment
        float weightSum = 0;
        turnSegments = new TurnSegment[segments.Length * 4];
        for (int i = 0; i < segments.Length; i++) {
            for (int t = 0; t < 4; t++) {
                turnSegments[i * 4 + t] = new TurnSegment(segments[i], t);
                weightSum += segments[i].weight;
            }
        }

        GenerateMap(ref turnSegments, weightSum);
    }

    //Generate fitting sockets to fill the map by usings the Wave Function Collapse Algorithm
    void GenerateMap(ref TurnSegment[] segments, float weightSum) {
        grid = new Array3D<GridBox>(mapSize + Vector3Int.one * 2);//make grid bigger -> 2 tiles empty for no dead ends

        for (int i = 0; i < grid.Length; i++) grid[i] = new GridBox(ref segments, weightSum);//fill the grid

        MakeBorderEmpty(ref grid);

        int iterLimit = grid.Length;
        //int iterLimit = 0;
        for (int iter = 0; iter < iterLimit; iter++) {
            if (!LowestEntropyCollapse(ref grid)) {//keeps collapsing until everything is collapsed
                Debug.Log("Collapse is done");
                break;
            }
        }

        InstantiateMap(ref grid);
    }

    void MakeBorderEmpty(ref Array3D<GridBox> grid) {
        void EmptyPlanePossible(ref Array3D<GridBox> grid, int x, int y, int z) {
            Vector3Int vec = Vector3Int.zero;
            for (vec[x] = 0; vec[x] < grid.size[x]; vec[x]++)
                for (vec[y] = 0; vec[y] < grid.size[y]; vec[y]++) {
                    vec[z] = 0;
                    grid[vec].possibilities.Add(emptySegment);
                    vec[z] = grid.size[z] - 1;
                    grid[vec].possibilities.Add(emptySegment);
                }
        }
        void EmptyPlane(ref Array3D<GridBox> grid, int x, int y, int z) {
            Vector3Int vec = Vector3Int.zero;
            for (vec[x] = 0; vec[x] < grid.size[x]; vec[x]++)
                for (vec[y] = 0; vec[y] < grid.size[y]; vec[y]++) {
                    vec[z] = 0;
                    Collapse(ref grid, grid.GetIndex(vec), emptySegment);
                    vec[z] = grid.size[z] - 1;
                    Collapse(ref grid, grid.GetIndex(vec), emptySegment);
                }
        }

        EmptyPlanePossible(ref grid, 0, 1, 2);
        EmptyPlanePossible(ref grid, 1, 2, 0);
        EmptyPlanePossible(ref grid, 2, 0, 1);

        EmptyPlane(ref grid, 0, 1, 2);
        EmptyPlane(ref grid, 1, 2, 0);
        EmptyPlane(ref grid, 2, 0, 1);
    }

    bool LowestEntropyCollapse(ref Array3D<GridBox> grid) {//collapse the box with the smallest number of possibilities
        //Get all the GridBoxes with the lowest Entropy
        int min = turnSegments.Length + 1;
        List<int> minGrids = new List<int>();
        for (int i = 0; i < grid.Length; i++) {
            if (grid[i].possibilities.Count <= 1 || grid[i].possibilities.Count > min) continue;

            if (grid[i].possibilities.Count < min) {
                min = grid[i].possibilities.Count;
                minGrids.Clear();
            }
            minGrids.Add(i);//add every tile that has min possibilities
        }


        if (min == turnSegments.Length + 1 || minGrids.Count == 0)//if everything is already collapsed
            return false;
            
        int toCollapse = minGrids[GetRndm(minGrids.Count)];
        bool collapseSuccess = CollapseRndm(ref grid, toCollapse);
        if (!collapseSuccess)
            Debug.Log("Collapse error");
        return collapseSuccess; 
    }

    bool CollapseRndm(ref Array3D<GridBox> grid, int toCollapse) {//tries to collapse a gridbox. returns if successful
        int collapseIndex = grid[toCollapse].GetWeightedRnd();
        return Collapse(ref grid, toCollapse, grid[toCollapse].possibilities[collapseIndex]);//pick one segment of all possible segments
    }
    bool Collapse(ref Array3D<GridBox> grid, int toCollapse, TurnSegment segment) {//tries to collapse a gridbox. returns if successful
        if (grid[toCollapse].SetResult(segment)) {
            PropergateCollapse(ref grid, toCollapse);
            return true;
        }
        return false;
    }
    void ForceCollapse(ref Array3D<GridBox> grid, int toCollapse, TurnSegment segment) {
        grid[toCollapse].ForceResult(segment);
        PropergateCollapse(ref grid, toCollapse);
    }

    void PropergateCollapse(ref Array3D<GridBox> grid, int init) {
        Stack<int> toPropergate = new Stack<int>();
        toPropergate.Push(init);

        while (toPropergate.Count > 0) {
            int i = toPropergate.Pop();
            if (grid[i].propergated) continue;
            else grid[i].propergated = true;

            TurnSegment segment = grid[i].possibilities[0];//get the only possibility
            Vector3Int pos = grid.GetPos(i);

            for(int d = 0; d < DirExt.directions.Length; d++) {
                Vector3Int neighbour = pos + DirExt.directions[d];
                if (!grid.InBounds(neighbour)) continue;

                int _i = grid.GetIndex(neighbour);
                Socket socket = segment.GetSocket(d);
                grid[_i].OnlyAllow(socket, (Direction)DirExt.InvertDir(d));

                if (grid[_i].possibilities.Count == 1 && !grid[_i].propergated)//if defining this socket fully collapses this gridbox
                    toPropergate.Push(_i);
            }
        }
    }

    //Use the grid to pick segments to place and put into the map array
    void InstantiateMap(ref Array3D<GridBox> grid) {
        
        //---------------- FILLING THE MAP ARRAY -----------------------
        map = new Array3D<TurnSegment>(grid.size);//NOTE: dont save the empty outermost layer
        for (int i = 0; i < map.Length; i++) {
            map[i] = grid[i].possibilities[0];//get the only possibility for this tile
        }
        

        
        //---------------- BUILDING THE MAP ---------------------------
        Transform tileParent = new GameObject("Tile Parent").transform;
        for (int i = 0; i < map.Length; i++) {
            Vector3 pos = (Vector3)map.GetPos(i);
            pos.x *= tileSize.x;
            pos.y *= tileSize.y;
            pos.z *= tileSize.z;

            if (map[i].segment.prefab != null)
                Instantiate(map[i].segment.prefab, pos, map[i].GetRot(), tileParent);
        }
    }

    private void OnDrawGizmos() {
        if(map != null) {
            for (int i = 0; i < map.Length; i++) {
                Vector3Int pos = map.GetPos(i);
                if (pos.y != 1) continue;

                for (int j = 0; j < grid[i].possibilities.Count; j++) {
                    DrawGizmosSegment(pos + Vector3.down*j, 0.5f, grid[i].possibilities[j], new Color[] { Color.gray, Color.green, Color.blue });
                }
            }
        }
    }
    private void DrawGizmosSegment(Vector3 pos, float size, TurnSegment segment, Color[] socketColor) {
        for (int d = 0; d < DirExt.directions.Length; d++) {
            Gizmos.color = socketColor[(int)segment.GetSocket(d)];
            Gizmos.DrawLine(pos, pos + (Vector3)DirExt.directions[d] * size);
        }
    }

    public static int GetRndm(int length) {
        return UnityEngine.Random.Range(0, length);
    }
}