using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.Rendering;

[CustomEditor(typeof(MapSegment))]
public class MapSegmentEditor : Editor
{
    /*
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        MapSegment segment = (MapSegment)target;

        GUILayout.Space(20);

        if(GUILayout.Button("Add missing rules from other tiles")) {
            Merge(ref segment.whitelist, LoadWhitelist(segment));
        }
        if (GUILayout.Button("Check Whitelist with other tiles")) {
            CompareWhitelist(segment.whitelist, LoadWhitelist(segment), segment);
        }
    }

    private static MapSegment[] GetAllInstances() {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(MapSegment).Name);  //FindAssets uses tags check documentation for more info
        MapSegment[] a = new MapSegment[guids.Length];
        for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            a[i] = AssetDatabase.LoadAssetAtPath<MapSegment>(path);
        }

        return a;
    }

    public bool ContainsInSomeOrientation(TurnSegment[] array, MapSegment segment, out int turn) {
        turn = 0;
        for (int i = 0; i < array.Length; i++) {
            if (array[i].segment == segment) {
                turn = array[i].turn;
                return true;
            }  
        }
        return false;
    }

    public Neighbour3D LoadWhitelist(MapSegment segment) {
        Neighbour3D neighbour3d = new Neighbour3D();
        MapSegment[] mapSegments = GetAllInstances();
        List<TurnSegment>[] whitelist = new List<TurnSegment>[6];
        for (int d = 0; d < 6; d++)
            whitelist[d] = new List<TurnSegment>();

        for (int d = 0; d < 6; d++) {
            Direction dir = (Direction)d;
            for (int i = 0; i < mapSegments.Length; i++) {
                if (ContainsInSomeOrientation(mapSegments[i].whitelist.GetNeighbours(dir), segment, out int turn)) {
                    int invTurn = -turn + 4;
                    int wDir = (int)dir.InvertDir().Turn(invTurn);
                    whitelist[wDir].Add(new TurnSegment(mapSegments[i], invTurn));//add the segment with the inverse turn
                }  
            }
        }

        for(int d = 0; d < 6; d++)
            neighbour3d.SetNeighbours(d, whitelist[d].ToArray());
        return neighbour3d;
    }

    public void Merge(ref Neighbour3D n1, Neighbour3D n2) {//merges n2 into n1
        for (int d = 0; d < 6; d++) {
            List<TurnSegment> union = new List<TurnSegment>(n1.GetNeighbours(d));
            for(int i = 0; i < n2.GetNeighbours(d).Length; i++) {
                bool contains = false;//check if the union contains already this element
                TurnSegment check = n2.GetNeighbours(d)[i];
                for(int u = 0; u < union.Count; u++) {
                    if (union[u].Equals(check)) {
                        contains = true;
                        break;
                    }
                }
                if (!contains)
                    union.Add(check);//otherwise add it
            }
            n1.SetNeighbours(d, union.ToArray());
        }
    }

    public static bool Contains(TurnSegment[] a1, TurnSegment check) {
        for (int i1 = 0; i1 < a1.Length; i1++)
            if (a1[i1].Equals(check)) 
                return true;//rule found in both lists
        return false;
    }



    private struct Rule {
        public MapSegment owner;
        public TurnSegment other;
        public Direction dir;

        public Rule(MapSegment owner, TurnSegment other, Direction dir) {
            this.owner = owner;
            this.other = other;
            this.dir = dir;
        }

        public Rule InverseRule() {
            int invRuleTurn = -other.turn + 4;

            Rule inverse = new Rule();
            inverse.owner = other.segment;
            inverse.other = new TurnSegment(owner, invRuleTurn);
            inverse.dir = dir.InvertDir().Turn(invRuleTurn);
            return inverse;
        }

        public override string ToString() {
            return $"{owner.name}: {other} on {dir}";
        }
    }

    public static void CompareWhitelist(Neighbour3D whitelist, Neighbour3D other, MapSegment thisSegment) {
        static void PrintRuleMatch(Rule rule) {
            Rule inverse = rule.InverseRule();
            Debug.LogWarning($"No rule match for {rule}\n" +
                             $"(rule needed on {inverse})");
        }

        for(int d = 0; d < 6; d++) {
            for(int n1 = 0; n1 < whitelist.GetNeighbours(d).Length; n1++) {
                if (!Contains(other.GetNeighbours(d), whitelist.GetNeighbours(d)[n1])) {
                    Rule rule = new Rule(thisSegment, whitelist.GetNeighbours(d)[n1], (Direction)d);
                    PrintRuleMatch(rule);
                }
            }
            
            for (int o = 0; o < other.GetNeighbours(d).Length; o++) {
                if (!Contains(whitelist.GetNeighbours(d), other.GetNeighbours(d)[o])) {
                    Rule rule = new Rule(thisSegment, other.GetNeighbours(d)[o], (Direction)d);
                    PrintRuleMatch(rule);
                }
            }
        }
    }

    #region Scene Preview
    private void OnEnable() {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable() {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView view)
    {
        MapSegment segment = target as MapSegment;
        MapGenerator mapGen = PollingStation.Instance.mapGenerator;

        var curCam = SceneView.currentDrawingSceneView.camera;



        TileInfo info = new TileInfo(segment, 0);




        for (int i = 0; i < 6; i++)
        {
            Vector3 modelPos = Vector3.zero;
            Vector3 dir = new Vector3();
            switch (i)
            {
                case 0: dir = Vector3.up * 9; break;
                case 1: dir = Vector3.right * 9; break;
                case 2: dir = Vector3.forward * 9; break;
                case 3: dir = Vector3.down * 9; break;
                case 4: dir = Vector3.left * 9; break;
                case 5: dir = Vector3.back * 9; break;
                default: break;
            }

            Vector3 offset = modelPos + Vector3.right * (mapGen.tileSize.x * 3) * i;
            Handles.Label(offset + Vector3.up * 3, new GUIContent(text: GetName(segment, i)));

            //info.RenderTile(modelPos, upDir, Color.green);

            var wMapSeg = segment.whitelist.GetNeighbours(i);
            if (wMapSeg != null)
                for (int map = 0; map < wMapSeg.Length; map++)
                {
                    TurnSegment seg = wMapSeg[map];
                    Vector3 offset2 = offset + new Vector3(0, 0, ((mapGen.tileSize.z * 3) * map));
                    info.RenderTile(offset2, Vector3.zero, Color.green);
                    TileInfo subInfo = new TileInfo(seg.segment, seg.turn);
                    subInfo.RenderTile(offset2, dir, Color.yellow);
                }
        }


    }

    private string GetName(MapSegment segment, int i)
    {
        switch (i)
        {
            case 0: return "top";
            case 1: return "right";
            case 2: return "front";
            case 3: return "bottom";
            case 4: return "left";
            case 5: return "back";
            default: return null;
        }
    }

    struct TileInfo
    {
        public TileInfo(MapSegment segment, int turn)
        {


            currentMeshes = new Mesh[0];
            localPos = new Vector3[0];
            materials = new Material[0];
            localSizes = new Vector3[0];
            localRots = new Quaternion[0];
            prefabScale = Vector3.one;
            m_turn = turn;
            if (!segment) return;
            var prefab = segment.prefab;
            if (!prefab) return;
            var meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
            currentMeshes = new Mesh[meshFilters.Length];
            localPos = new Vector3[meshFilters.Length];
            materials = new Material[meshFilters.Length];
            localSizes = new Vector3[meshFilters.Length];
            localRots = new Quaternion[meshFilters.Length];
            m_turn = turn;
            prefabScale = prefab.transform.localScale;
            for (int i = 0; i < meshFilters.Length; i++)
            {
                currentMeshes[i] = meshFilters[i].sharedMesh;
                localPos[i] = meshFilters[i].transform.position;
                localSizes[i] = meshFilters[i].transform.localScale;
                localRots[i] = meshFilters[i].transform.rotation;

                var render = meshFilters[i].GetComponent<MeshRenderer>();
                materials[i] = render.sharedMaterial;

            }

        }

        Vector3[] localSizes;
        Vector3[] localPos;
        Mesh[] currentMeshes;
        Material[] materials;
        Quaternion[] localRots;
        Vector3 prefabScale;
        int m_turn;


        public void RenderTile(Vector3 aPos, Vector3 anOffset, Color renderColor)
        {
            Graphics.ClearRandomWriteTargets();
            if (currentMeshes.Length == 0)
            {
                Mesh builtinCubeMesh = UnityExtensions.LoadAssetFromUniqueAssetPath<Mesh>("Library/unity default resources::Cube");
                Material defaultMat = new Material(Shader.Find("Diffuse"));
                Matrix4x4 m = Matrix4x4.TRS(aPos + anOffset, Quaternion.identity, Vector3.one * 9);
                Graphics.DrawMesh(builtinCubeMesh, m, defaultMat, 0, SceneView.currentDrawingSceneView.camera);

                return;
            }



            var turnRot = Quaternion.Euler(0, m_turn * 90, 0);



            for (int i = 0; i < currentMeshes.Length; i++)
            {
                Vector3 scale = new Vector3(
                    prefabScale.x * localSizes[i].x,
                    prefabScale.y * localSizes[i].y,
                    prefabScale.z * localSizes[i].z);
                Vector3 pos = m_turn != 0 ? (turnRot * localPos[i] + aPos) + anOffset : localPos[i] + aPos + anOffset;
                Quaternion rot = m_turn != 0 ? localRots[i] * turnRot : localRots[i];

                Matrix4x4 m = Matrix4x4.TRS(pos, rot, scale);
                Graphics.DrawMesh(currentMeshes[i], m, materials[i], 0, SceneView.currentDrawingSceneView.camera);

            }




        }

    }
    #endregion
    */
}
