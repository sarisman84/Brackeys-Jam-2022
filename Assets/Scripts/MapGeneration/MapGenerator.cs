using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public partial class MapGenerator : MonoBehaviour
{
    public static MapGenerator instance; //<-- [Depecrated]: PollingStation in use! (Spyro)
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning($"There is more than one {GetType().Name}");
            return;
        }
        instance = this;
    }

    public Vector3Int mapSize = new Vector3Int(5, 1, 5);
    public Vector3 tileSize = Vector3.one;

    [Space]
    [Header("Segments")]

    public MapSegment startSegment;//open only to z direction (forward)
    public MapSegment endSegment;//open only to negative z direction (backward)

    [Space]

    private Array3D<TurnSegment> map;
    private Array3D<GridBox> grid;//FOR DEBUGGING ONLY
    private MapSections sections;//FOR DEBUGGING ONLY

    public MapSegment empty;
    private TurnSegment emptySegment;
    public static bool IsEmpty(TurnSegment segment) { return segment.segment.socket.IsCollisionOnly(); }

    public MapSegment[] segments;
    private TurnSegment[] turnSegments;

    [Space]
    [Header("Debug")]
    public bool createOnAwake = true;


    private Transform tileParent;

    public void LoadProcedualMap()
    {



        //generate TurnSegments from the mapSegment
        float weightSum = 0;
        turnSegments = new TurnSegment[segments.Length * 4];
        for (int i = 0; i < segments.Length; i++)
        {
            for (int t = 0; t < 4; t++)
            {
                turnSegments[i * 4 + t] = new TurnSegment(segments[i], t);
                weightSum += segments[i].weight;
            }
        }


        emptySegment = new TurnSegment(empty, 0);

        GenerateMap(ref turnSegments, weightSum);

        sections = new MapSections();
        sections.GenerateSections(ref map);
        sections.ConnectSections(ref map);

        InstantiateMap();
    }

    void Start()
    {
        PollingStation.Instance.runtimeManager.onStateChangeCallback += (RuntimeManager.RuntimeState state) =>
        {
            switch (state)
            {
                case RuntimeManager.RuntimeState.MainMenu: //Delete map on main menu transition
                    if (tileParent)
                        Destroy(tileParent.gameObject);
                    break;
                case RuntimeManager.RuntimeState.Playing: //Generate map on starting the game from the main menu.
                    LoadProcedualMap();
                    break;
            }

        };


        if (createOnAwake)
            LoadProcedualMap();
    }

    TurnSegment FindFittingSegment(Socket3D socket)
    {
        for (int s = 0; s < turnSegments.Length; s++)
        {
            if (turnSegments[s].Fits(socket))
                return turnSegments[s];
        }
        Debug.LogError($"No fitting segment found for {socket}");
        return null;
    }

    #region MapGeneration
    //Generate fitting sockets to fill the map by usings the Wave Function Collapse Algorithm
    void GenerateMap(ref TurnSegment[] segments, float weightSum)
    {
        grid = new Array3D<GridBox>(mapSize + Vector3Int.one * 2);//make grid bigger -> 2 tiles empty for no dead ends

        for (int i = 0; i < grid.Length; i++) grid[i] = new GridBox(ref segments, weightSum);//fill the grid

        SetupGrid(ref grid);

        int iterLimit = grid.Length;
        //int iterLimit = 0;
        for (int iter = 0; iter < iterLimit; iter++)
        {
            if (!LowestEntropyCollapse(ref grid))
            {//keeps collapsing until everything is collapsed
                Debug.Log("Collapse is done");
                break;
            }
        }

        FillMap(ref grid);
    }

    void SetupGrid(ref Array3D<GridBox> grid)
    {
        void EmptyPlanePossible(ref Array3D<GridBox> grid, int x, int y, int z)
        {
            Vector3Int vec = Vector3Int.zero;
            for (vec[x] = 0; vec[x] < grid.size[x]; vec[x]++)
                for (vec[y] = 0; vec[y] < grid.size[y]; vec[y]++)
                {
                    vec[z] = 0;
                    grid[vec].possibilities.Add(emptySegment);
                    vec[z] = grid.size[z] - 1;
                    grid[vec].possibilities.Add(emptySegment);
                }
        }
        void EmptyPlane(ref Array3D<GridBox> grid, int x, int y, int z)
        {
            Vector3Int vec = Vector3Int.zero;
            for (vec[x] = 0; vec[x] < grid.size[x]; vec[x]++)
                for (vec[y] = 0; vec[y] < grid.size[y]; vec[y]++)
                {
                    vec[z] = 0;
                    Collapse(ref grid, grid.GetIndex(vec), emptySegment);
                    vec[z] = grid.size[z] - 1;
                    Collapse(ref grid, grid.GetIndex(vec), emptySegment);
                }
        }

        EmptyPlanePossible(ref grid, 0, 1, 2);
        EmptyPlanePossible(ref grid, 1, 2, 0);
        EmptyPlanePossible(ref grid, 2, 0, 1);

        //do a collapse change in here (change wont be overridden due to a soft collapse)
        AddStartAndEnd(ref grid);

        EmptyPlane(ref grid, 0, 1, 2);
        EmptyPlane(ref grid, 1, 2, 0);
        EmptyPlane(ref grid, 2, 0, 1);
    }

    void AddStartAndEnd(ref Array3D<GridBox> grid)
    {
        Vector3Int startPos = new Vector3Int(1, grid.size.y - 2, 0);
        Vector3Int endPos = new Vector3Int(grid.size.x - 2, 1, grid.size.z - 1);

        grid[startPos].ForceResult(new TurnSegment(startSegment, 0));
        grid[endPos].ForceResult(new TurnSegment(endSegment, 0));
    }

    bool LowestEntropyCollapse(ref Array3D<GridBox> grid)
    {//collapse the box with the smallest number of possibilities
        //Get all the GridBoxes with the lowest Entropy
        int min = turnSegments.Length + 1;
        List<int> minGrids = new List<int>();
        for (int i = 0; i < grid.Length; i++)
        {
            if (grid[i].possibilities.Count <= 1 || grid[i].possibilities.Count > min) continue;

            if (grid[i].possibilities.Count < min)
            {
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

    bool CollapseRndm(ref Array3D<GridBox> grid, int toCollapse)
    {//tries to collapse a gridbox. returns if successful
        int collapseIndex = grid[toCollapse].GetWeightedRnd();
        return Collapse(ref grid, toCollapse, grid[toCollapse].possibilities[collapseIndex]);//pick one segment of all possible segments
    }
    bool Collapse(ref Array3D<GridBox> grid, int toCollapse, TurnSegment segment)
    {//tries to collapse a gridbox. returns if successful
        if (grid[toCollapse].SetResult(segment))
        {
            PropergateCollapse(ref grid, toCollapse);
            return true;
        }
        return false;
    }
    void ForceCollapse(ref Array3D<GridBox> grid, int toCollapse, TurnSegment segment)
    {
        grid[toCollapse].ForceResult(segment);
        PropergateCollapse(ref grid, toCollapse);
    }

    void PropergateCollapse(ref Array3D<GridBox> grid, int init)
    {
        Stack<int> toPropergate = new Stack<int>();
        toPropergate.Push(init);

        while (toPropergate.Count > 0)
        {
            int i = toPropergate.Pop();
            if (grid[i].propergated) continue;
            else grid[i].propergated = true;

            TurnSegment segment = grid[i].possibilities[0];//get the only possibility
            Vector3Int pos = grid.GetPos(i);

            for (int d = 0; d < DirExt.directions.Length; d++)
            {
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

    void FillMap(ref Array3D<GridBox> grid)
    {//Use the grid to pick segments to put into the map array
        map = new Array3D<TurnSegment>(grid.size);//NOTE: dont save the empty outermost layer
        for (int i = 0; i < map.Length; i++)
        {
            map[i] = grid[i].possibilities[0];//get the only possibility for this tile
        }
    }

    #endregion


    void InstantiateMap()
    {
        tileParent = new GameObject("Tile Parent").transform;
        for (int i = 0; i < map.Length; i++)
        {
            Vector3 pos = (Vector3)map.GetPos(i);
            pos.x *= tileSize.x;
            pos.y *= tileSize.y;
            pos.z *= tileSize.z;

            if (map[i].segment.prefab != null)
                Instantiate(map[i].segment.prefab, pos, map[i].GetRot(), tileParent);
        }
    }

    private void OnDrawGizmos()
    {
        if (map != null)
        {
            for (int i = 0; i < map.Length; i++)
            {
                Vector3Int pos = map.GetPos(i);
                if (pos.y != 1) continue;

                if (IsEmpty(map[i])) continue;//skip empty

                for (int j = 0; j < grid[i].possibilities.Count; j++)
                {
                    DrawGizmosSegment(pos + Vector3.down * j, 0.5f, grid[i].possibilities[j]);
                }
            }
        }
    }
    private void DrawGizmosSegment(Vector3 pos, float size, TurnSegment segment)
    {
        for (int d = 0; d < DirExt.directions.Length; d++)
        {
            Gizmos.color = new Color[] { Color.gray, Color.green, Color.blue, Color.red }[(int)segment.GetSocket(d)];
            Gizmos.DrawLine(pos, pos + (Vector3)DirExt.directions[d] * size);
        }
    }


    public static int GetRndm(int length)
    {
        return UnityEngine.Random.Range(0, length);
    }
}
