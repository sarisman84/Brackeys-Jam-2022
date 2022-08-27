using System.Collections.Generic;
using UnityEngine;

public partial class MapGenerator : MonoBehaviour
{
    public Vector3Int mapSize = new Vector3Int(5, 1, 5);
    public Vector3 tileSize = Vector3.one;
    public Vector3Int GetGridPos(Vector3 pos) { return Vector3Int.FloorToInt(pos.CompDiv(tileSize)); }
    public Vector3 GetWorldPos(Vector3Int pos) { return ((Vector3)pos).CompMul(tileSize); }


    [Space]
    [Header("Segments")]

    public MapSegment startSegment;//open only to z direction (forward)
    public MapSegment endSegment;//open only to negative z direction (backward)

    [Space]

    public MapSegment borderSegment;
    public MapSegment[] segments;

    [Space]
    [Header("Debug")]
    public bool createOnAwake = true;
    public int generateSeed = 0;
    public bool randomSeed = true;

    public Array3D<TurnSegment> map;
    private Array3D<GridBox> grid;//FOR DEBUGGING ONLY
    private MapSections sections;//FOR DEBUGGING ONLY

    private TurnSegment[] turnSegments;

    private Transform tileParent;

    public void DestroyMap() { if (tileParent) Destroy(tileParent.gameObject); }


    void Start()
    {
        if (createOnAwake)
            LoadProcedualMap();
    }

    public void LoadProcedualMap()
    {
        int seed = generateSeed;
        if (randomSeed)
            seed = Random.Range(int.MinValue, int.MaxValue);


        Random.InitState(seed);
        Debug.Log("Start Procedural Map Generation with the Seed " + seed);

        //generate TurnSegments from the mapSegment
        float weightSum = 0;
        {
            List<TurnSegment> turned = new List<TurnSegment>();
            for (int i = 0; i < segments.Length; i++)
            {
                int turns = (int)segments[i].turnInstances;
                float weightMul = 4.0f / turns;
                for (int t = 0; t < turns; t++)
                {
                    turned.Add(new TurnSegment(segments[i], t, weightMul));
                    weightSum += weightMul * segments[i].weight;
                }
            }
            turnSegments = turned.ToArray();
        }


        int maxGen = 50;
        int gen;
        for (gen = 0; gen < maxGen; gen++)
        {
            try
            {
                GenerateMap(ref turnSegments, weightSum);

                sections = new MapSections();
                sections.GenerateSections(ref map);
                sections.ConnectSections(ref map);

                break;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("Failed to Generate Map");
            }
        }

        InstantiateMap();
        Debug.Log($"Map Generated after {gen + 1} tries");
    }

    #region MapGeneration
    //Generate fitting sockets to fill the map by usings the Wave Function Collapse Algorithm
    void GenerateMap(ref TurnSegment[] segments, float weightSum)
    {
        //------------- FIND ALL POSSIBLE SOCKETS -------------------------------
        //go through all tiles and find which sockets they have and add them
        List<Socket>[] basePossibilities = new List<Socket>[6];
        for (int d = 0; d < 6; d++)
        {
            basePossibilities[d] = new List<Socket>();
            for (int s = 0; s < segments.Length; s++)
            {
                if (!basePossibilities[d].Contains(segments[s].GetSocket(d)))
                    basePossibilities[d].Add(segments[s].GetSocket(d));
            }
        }

        //INITIALIZE GRID
        grid = new Array3D<GridBox>(mapSize + Vector3Int.one * 2);//make grid bigger -> 2 tiles empty for no dead ends
        for (int i = 0; i < grid.Length; i++) grid[i] = new GridBox(ref segments, weightSum, basePossibilities);//fill the grid

        SetupGrid(ref grid);


        //----------- WAVE FUNCTION COLLAPSE --------------------
        int iterLimit = grid.Length;
        //int iterLimit = 20;
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
        TurnSegment emptySegment = new TurnSegment(borderSegment, 0);

        #region Setup Helper Functions
        void EmptyPlanePossible(ref Array3D<GridBox> grid, int x, int y, int z)//Add the possibility for empty tiles
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
        void EmptyPlane(ref Array3D<GridBox> grid, int x, int y, int z)//collpse grids on empty tiles
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

        void AddStartAndEnd(ref Array3D<GridBox> grid)
        {
            Vector3Int startPos = new Vector3Int(1, grid.size.y - 2, 0);
            Vector3Int endPos = new Vector3Int(grid.size.x - 2, 1, grid.size.z - 1);

            ForceCollapse(ref grid, grid.GetIndex(startPos), new TurnSegment(startSegment, 0));
            ForceCollapse(ref grid, grid.GetIndex(endPos), new TurnSegment(endSegment, 0));
        }
        #endregion

        EmptyPlanePossible(ref grid, 0, 1, 2);
        EmptyPlanePossible(ref grid, 1, 2, 0);
        EmptyPlanePossible(ref grid, 2, 0, 1);

        //do collapse changes in here (change wont be overridden due to a soft collapse for the empty planes)
        AddStartAndEnd(ref grid);//collapse start and end points

        EmptyPlane(ref grid, 0, 1, 2);
        EmptyPlane(ref grid, 1, 2, 0);
        EmptyPlane(ref grid, 2, 0, 1);
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

        int toCollapse = minGrids[UnityExtensions.GetRndm(minGrids.Count)];
        bool collapseSuccess = CollapseRndm(ref grid, toCollapse);
        if (!collapseSuccess)
            Debug.Log("Collapse error");
        return collapseSuccess;
    }

    bool CollapseRndm(ref Array3D<GridBox> grid, int toCollapse)
    {//tries to collapse a gridbox. returns if successful
        int collapseSegment = grid[toCollapse].GetWeightedRnd();
        return Collapse(ref grid, toCollapse, grid[toCollapse].possibilities[collapseSegment]);//pick one segment of all possible segments
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

        //while (toPropergate.Count > 0) {
        for (int iter = 0; iter < 6 * grid.Length; iter++)
        {//ONLY FOR DEBUGGING
            if (toPropergate.Count <= 0) break;

            int i = toPropergate.Pop();
            Vector3Int pos = grid.GetPos(i);

            for (int d = 0; d < DirExt.directions.Length; d++)
            {
                Vector3Int neighbour = pos + DirExt.directions[d];
                if (!grid.InBounds(neighbour)) continue;

                int _i = grid.GetIndex(neighbour);
                //if (grid[_i].possibilities.Count <= 1) continue;

                //only allow sockets that can connect to the possible sockets on the other side
                HashSet<Socket> sockets = grid[i].possibleSockets[d];
                int changeCount = grid[_i].OnlyAllow(sockets, (Direction)DirExt.InvertDir(d));

                if (changeCount > 0 && !toPropergate.Contains(_i))//if grid[_i] has a change in possibilities
                    toPropergate.Push(_i);//propergate this change
            }
        }

        if (toPropergate.Count > 0)
            Debug.LogError("Propergation took to many iterations");
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
            pos = pos.CompMul(tileSize);

            if (map[i].segment.prefab != null)
            {
                var obj = Instantiate(map[i].segment.prefab, pos, map[i].GetRot(), tileParent);

                if (obj.tag.Equals("Exit"))
                {
                    Debug.Log("[Log]<MapGen>: Generated Exit! Caching!");
                    PollingStation.Instance.gameManager.exitRoom = obj;
                }
            }

        }
        tileParent.transform.position = transform.position;//put the map origion to the position of the MapGenerator
    }


    #region Gizmos for Debugging
    private void OnDrawGizmos()
    {
        if (map != null)
        {
            for (int i = 0; i < map.Length; i++)
            {
                Vector3Int pos = map.GetPos(i);
                if (pos.y != 1) continue;

                //if (IsEmpty(map[i])) continue;//skip empty

                //DrawGizmosSegmentPSockets(pos + Vector3.up, 0.5f, grid[i]);
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
            Gizmos.color = segment.GetSocket(d).color;
            Gizmos.DrawLine(pos, pos + (Vector3)DirExt.directions[d] * size);
        }
    }
    private void DrawGizmosSegmentPSockets(Vector3 pos, float size, GridBox cell)
    {
        for (int d = 0; d < DirExt.directions.Length; d++)
        {
            float segSize = size / cell.possibleSockets[d].Count;
            int i = 0;
            foreach (Socket s in cell.possibleSockets[d])
            {
                float t1 = (float)i / cell.possibleSockets[d].Count;
                Vector3 p1 = pos + Vector3.Lerp(Vector3.zero, DirExt.directions[d], t1);
                Vector3 p2 = p1 + (Vector3)DirExt.directions[d] * segSize;
                Gizmos.color = s.color;
                Gizmos.DrawLine(p1, p2);
                i++;
            }
        }
    }
    #endregion
}
