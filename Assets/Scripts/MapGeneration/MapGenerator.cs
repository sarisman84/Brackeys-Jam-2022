using System;
using System.Collections.Generic;
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

    public Vector3Int gridSize = new Vector3Int(5, 0, 5);
    private int GetMapSize() { return gridSize.x * gridSize.y * gridSize.z; }
    private Vector3Int GetPos(int i) {
        Vector3Int vec = new Vector3Int(0, 0, 0);
        vec.z = i % (gridSize.x * gridSize.y);
        vec.y = (i - vec.z*gridSize.x*gridSize.y) % gridSize.y;
        vec.x = i - vec.z*gridSize.x*gridSize.y - vec.y*gridSize.y;
        return vec;
    }
    private int GetIndex(Vector3Int vec) {
        return vec.x + vec.y * gridSize.x + vec.z * gridSize.x * gridSize.y;
    }

    public static int GetRndm(int length) {
        return UnityEngine.Random.Range(0, length);
    }

    private MapSegment[] map;


    public MapSegment[] segments;
    //private Dictionary<MapSocket, List<int>> SocketToSegment;

    void Start()
    {
        //FILL THE DICTIONARY
        /*SocketToSegment = new Dictionary<MapSocket, List<int>>();
        for(int seg = 0; seg < segments.Length; seg++) {
            MapSocket socket = segments[seg].socket;
            if (!SocketToSegment.ContainsKey(socket))
                SocketToSegment[socket] = new List<int>();

            SocketToSegment[socket].Add(seg);
        }*/

        GenerateMap(ref segments);
    }

    //Generate fitting sockets to fill the map by usings the Wave Function Collapse Algorithm
    void GenerateMap(ref MapSegment[] segments) {
        GridBox[] grid = new GridBox[GetMapSize()];
        for (int i = 0; i < grid.Length; i++) grid[i] = new GridBox(ref segments);//fill the grid

        while (LowestEntropyCollapse(ref grid)) {}//keeps collapsing until everything is collapsed

        InstantiateMap(ref grid);
    }

    bool LowestEntropyCollapse(ref GridBox[] grid) {//collapse the box with the smallest number of possibilities
        //Get all the GridBoxes with the lowest Entropy
        int min = segments.Length + 1;
        List<int> minGrids = new List<int>();
        for (int i = 0; i < grid.Length; i++)
            if (grid[i].possibilityCount > 1 && grid[i].possibilityCount < min) {
                min = grid[i].possibilityCount;
                minGrids.Clear();
                minGrids.Add(i);
            }
            else if (grid[i].possibilityCount == min){
                minGrids.Add(i);
            }

        if(min == segments.Length+1)//if everything is already collapsed
            return false;
            
        int toCollapse = GetRndm(minGrids.Count);
        Collapse(ref grid, toCollapse);
        return true;
    }

    bool Collapse(ref GridBox[] grid, int toCollapse) {
        int collapseIndex = GetRndm(grid[toCollapse].possibilities.Count);
        MapSegment collapseOn = grid[toCollapse].possibilities[collapseIndex];//pick one segment of all possible segments
        if (grid[toCollapse].SetResult(collapseOn)) {
            PropergateCollapse(ref grid, toCollapse);
            return true;
        }
        return false;
    }

    public enum Direction { Right, Top, Front, Left, Bottom, Back }
    public static Vector3Int[] directions = { Vector3Int.right, Vector3Int.up, Vector3Int.forward, Vector3Int.left, Vector3Int.down, Vector3Int.back };
    public int InvertDir(int d) { return (d + 3) % 6; }

    void PropergateCollapse(ref GridBox[] grid, int init) {
        Stack<int> toPropergate = new Stack<int>();
        toPropergate.Push(init);

        while (toPropergate.Count > 0) {
            int i = toPropergate.Pop();
            if (grid[i].propergated) continue;
            else grid[i].propergated = true;

            MapSegment segment = grid[i].possibilities[0];//get the only possibility
            Vector3Int pos = GetPos(i);

            for(int d = 0; d < directions.Length; d++) {
                int _i = GetIndex(pos + directions[d]);
                Socket socket = segment.sockets[d];
                grid[_i].OnlyAllow(socket, (Direction)InvertDir(d));

                if (grid[_i].possibilityCount == 1 && !grid[_i].propergated)//if defining this socket fully collapses this gridbox
                    toPropergate.Push(_i);
            }
        }
    }


    void InstantiateMap(ref GridBox[] grid) {
        //Use the grid to pick segments to place and put into the map array

        map = new MapSegment[grid.Length];
        for (int i = 0; i < grid.Length; i++) {
            map[i] = grid[i].possibilities[0];//get the only possibility for this tile
        }
    }

    private void OnDrawGizmosSelected() {
        if(map != null) {
            for (int i = 0; i < map.Length; i++) {
                Vector3Int pos = GetPos(i);
                DrawGizmosSegment(pos, 1.0f, map[i]);
            }
        }
    }
    private void DrawGizmosSegment(Vector3 pos, float size, MapSegment segment) {
        Color[] socketColor = new Color[3] { Color.clear, Color.green, Color.blue };

        for (int d = 0; d < directions.Length; d++) {
            Gizmos.color = socketColor[(int)segment.sockets[d]];
            Gizmos.DrawLine(pos, pos + (Vector3)directions[d] * size);
        }
    }
}
