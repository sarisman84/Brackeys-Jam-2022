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


    public static int GetRndm(int length) {
        return UnityEngine.Random.Range(0, length);
    }

    private Array3D<MapSegment> map;


    public MapSegment empty;
    public MapSegment[] segments;

    void Start()
    {
        GenerateMap(ref segments);
    }

    //Generate fitting sockets to fill the map by usings the Wave Function Collapse Algorithm
    void GenerateMap(ref MapSegment[] segments) {
        Array3D<GridBox> grid = new Array3D<GridBox>(mapSize + Vector3Int.one * 2);//make grid bigger -> 2 tiles empty for no dead ends

        for (int i = 0; i < grid.Length; i++) grid[i] = new GridBox(ref segments);//fill the grid

        MakeBorderEmpty(ref grid);

        int iterLimit = grid.Length;
        //int iterLimit = 1;
        for (int iter = 0; iter < iterLimit; iter++) {
            if (!LowestEntropyCollapse(ref grid))//keeps collapsing until everything is collapsed
                break;
        }

        InstantiateMap(ref grid);
    }

    void MakeBorderEmpty(ref Array3D<GridBox> grid) {
        void EmptyPlane(ref Array3D<GridBox> grid, int x, int y, int z) {
            Vector3Int vec = Vector3Int.zero;
            for (vec[x] = 0; vec[x] < grid.size[x]; vec[x]++)
                for (vec[y] = 0; vec[y] < grid.size[y]; vec[y]++) {
                    vec[z] = 0;
                    ForceCollapse(ref grid, grid.GetIndex(vec), empty);
                    vec[z] = grid.size[z] - 1;
                    ForceCollapse(ref grid, grid.GetIndex(vec), empty);
                }
        }

        EmptyPlane(ref grid, 0, 1, 2);
        EmptyPlane(ref grid, 1, 2, 0);
        EmptyPlane(ref grid, 2, 0, 1);
    }

    bool LowestEntropyCollapse(ref Array3D<GridBox> grid) {//collapse the box with the smallest number of possibilities
        //Get all the GridBoxes with the lowest Entropy
        int min = segments.Length + 1;
        List<int> minGrids = new List<int>();
        for (int i = 0; i < grid.Length; i++) {
            if (grid[i].possibilities.Count <= 1 || grid[i].possibilities.Count > min) continue;

            if (grid[i].possibilities.Count < min) {
                min = grid[i].possibilities.Count;
                minGrids.Clear();
            }
            minGrids.Add(i);
        }
            

        if(min == segments.Length+1 || minGrids.Count == 0)//if everything is already collapsed
            return false;
            
        int toCollapse = minGrids[GetRndm(minGrids.Count)];
        bool collapseSuccess = CollapseRndm(ref grid, toCollapse);
        if (!collapseSuccess)
            Debug.Log("Collapse error");
        return collapseSuccess; 
    }

    bool CollapseRndm(ref Array3D<GridBox> grid, int toCollapse) {//tries to collapse a gridbox. returns if successful
        int collapseIndex = GetRndm(grid[toCollapse].possibilities.Count);
        return Collapse(ref grid, toCollapse, collapseIndex);
    }
    bool Collapse(ref Array3D<GridBox> grid, int toCollapse, int collapseIndex) {//tries to collapse a gridbox. returns if successful
        MapSegment collapseOn = grid[toCollapse].possibilities[collapseIndex];//pick one segment of all possible segments
        if (grid[toCollapse].SetResult(collapseOn)) {
            PropergateCollapse(ref grid, toCollapse);
            return true;
        }
        return false;
    }
    void ForceCollapse(ref Array3D<GridBox> grid, int toCollapse, MapSegment segment) {
        grid[toCollapse].ForceResult(segment);
        PropergateCollapse(ref grid, toCollapse);
    }


    public enum Direction { Right, Top, Front, Left, Bottom, Back }
    public static Vector3Int[] directions = { Vector3Int.right, Vector3Int.up, Vector3Int.forward, Vector3Int.left, Vector3Int.down, Vector3Int.back };
    public int InvertDir(int d) { return (d + 3) % 6; }

    void PropergateCollapse(ref Array3D<GridBox> grid, int init) {
        Stack<int> toPropergate = new Stack<int>();
        toPropergate.Push(init);

        while (toPropergate.Count > 0) {
            int i = toPropergate.Pop();
            if (grid[i].propergated) continue;
            else grid[i].propergated = true;

            MapSegment segment = grid[i].possibilities[0];//get the only possibility
            Vector3Int pos = grid.GetPos(i);

            for(int d = 0; d < directions.Length; d++) {
                Vector3Int neighbour = pos + directions[d];
                if (!grid.InBounds(neighbour)) continue;

                int _i = grid.GetIndex(neighbour);
                Socket socket = segment.sockets[d];
                grid[_i].OnlyAllow(socket, (Direction)InvertDir(d));

                if (grid[_i].possibilities.Count == 1 && !grid[_i].propergated)//if defining this socket fully collapses this gridbox
                    toPropergate.Push(_i);
            }
        }
    }


    void InstantiateMap(ref Array3D<GridBox> grid) {
        //Use the grid to pick segments to place and put into the map array
        map = new Array3D<MapSegment>(mapSize);//NOTE: dont save the empty outermost layer
        
        Vector3Int vec = Vector3Int.zero;
        for (vec.x = 1; vec.x < grid.size.x - 1; vec.x++)
            for (vec.y = 1; vec.y < grid.size.y - 1; vec.y++)
                for (vec.z = 1; vec.z < grid.size.z - 1; vec.z++) {
                    int gridI = grid.GetIndex(vec);
                    int mapI = map.GetIndex(vec - Vector3Int.one);

                    map[mapI] = grid[gridI].possibilities[0];//get the only possibility for this tile

                    //if (grid[gridI].possibilities.Count == 1)
                        //map[mapI] = grid[gridI].possibilities[0];//get the only possibility for this tile
                    //else
                        //map[mapI] = null;//ONLY FOR TESTING
                }

        /*
        for (int i = 0; i < map.Length; i++) {
            if (grid[i].possibilities.Count == 1)
                map[i] = grid[i].possibilities[0];//get the only possibility for this tile
            else
                map[i] = null;//ONLY FOR TESTING
        }*/
    }

    private void OnDrawGizmos() {
        if(map != null) {
            for (int i = 0; i < map.Length; i++) {
                Vector3Int pos = map.GetPos(i);
                DrawGizmosSegment(pos, 0.5f, map[i], new Color[] { Color.clear, Color.green, Color.blue });    
            }
        }
    }
    private void DrawGizmosSegment(Vector3 pos, float size, MapSegment segment, Color[] socketColor) {//{ Color.clear, Color.green, Color.blue }
        for (int d = 0; d < directions.Length; d++) {
            Gizmos.color = socketColor[(int)segment.sockets[d]];
            Gizmos.DrawLine(pos, pos + (Vector3)directions[d] * size);
        }
    }
}
