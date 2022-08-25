using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

[CustomEditor(typeof(MapSegment)), CanEditMultipleObjects]
public class MapSegmentEditor : Editor
{
    int selected = -1;
    private static Color selectionColor { get; } = Color.yellow;

    SerializedProperty sockets;
    SerializedProperty socketBase;

    ReorderableList socketList;


    private GUIContent[] socketNames;
    private Color[] socketColors;


    private void OnEnable()
    {

        SceneView.duringSceneGui += OnSceneGUI;

        socketBase = serializedObject.FindProperty("socket");

        //var socketObj = new SerializedObject(socket.objectReferenceValue);
        sockets = socketBase.FindPropertyRelative("sockets");
        if (sockets == null) Debug.Log($"Couldnt find sockets. Found: {socketBase.CountInProperty()}");

        InitializeList();

        socketList = new ReorderableList(serializedObject, sockets, false, true, false, false);

        socketList.drawElementCallback += OnElementDrawn;
        socketList.onSelectCallback += OnSelectElement;
        socketList.drawHeaderCallback += HeaderDef;




    }



    private void HeaderDef(Rect rect)
    {
        EditorGUI.LabelField(rect, new GUIContent("Sockets"));
        rect.x += 50;
        rect.width = 70;
        if (GUI.Button(rect, new GUIContent("Clear Selection")))
        {
            socketList.Deselect(selected);
            selected = -1;
            SceneView.RepaintAll();
        }
    }

    private void InitializeList()
    {
        if (sockets.arraySize != 6)
        {
            sockets.arraySize = 6;
        }
        socketNames = new GUIContent[6];
        socketColors = new Color[6];


        for (int i = 0; i < socketNames.Length; i++)
        {
            socketNames[i] = new GUIContent(DirectionToName(i));


        }
    }

    private string DirectionToName(int direction)
    {
        switch (direction)
        {
            case 0:
                return "Top";
            case 1:
                return "Right";
            case 2:
                return "Front";
            case 3:
                return "Bottom";
            case 4:
                return "Left";
            case 5:
                return "Back";
            default:
                return "Unknown";
        }
    }

    private void OnSelectElement(ReorderableList list)
    {

        selected = list.index;
        SceneView.RepaintAll();
    }

    private void OnElementDrawn(Rect rect, int index, bool isActive, bool isFocused)
    {

        EditorGUI.PropertyField(rect, sockets.GetArrayElementAtIndex(index), socketNames[index]);

        socketColors[index] = sockets.GetArrayElementAtIndex(index) is SerializedProperty element && element.objectReferenceValue != null ? (element.objectReferenceValue as Socket).color : Color.clear;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public void SocketField(SerializedProperty property)
    {

    }

    public override void OnInspectorGUI()
    {
        SerializedProperty p = serializedObject.GetIterator();
        p.NextVisible(true);
        while (p.NextVisible(false))
        {

            if (p.propertyPath == socketBase.propertyPath) continue;
            EditorGUILayout.PropertyField(p);
        }



        socketList?.DoLayoutList();

        serializedObject.ApplyModifiedProperties();




        //MapSegment mapSegment = (MapSegment)target;
        //if(mapSegment.socket.sockets == null)
        //    mapSegment.socket.sockets = new Socket[6];
        //else if (mapSegment.socket.sockets.Length != 6)
        //    mapSegment.socket.sockets = new Socket[6];

        //for (int d = 0; d < 6; d++) {
        //    EditorGUILayout.BeginHorizontal();

        //    Direction dir = (Direction)d;
        //    GUIStyle style = new GUIStyle(GUI.skin.button);
        //    style.normal.textColor = selected == d ? selectionColor : GUI.skin.button.normal.textColor;
        //    style.hover.textColor = selected == d ? selectionColor : GUI.skin.button.hover.textColor;
        //    if (GUILayout.Button(dir.ToString(), style, GUILayout.Width(70))) {
        //        selected = d;
        //        SceneView.RepaintAll();
        //    }

        //    Socket old = mapSegment.socket.sockets[d];
        //    mapSegment.socket.sockets[d] = (Socket)EditorGUILayout.ObjectField(mapSegment.socket.sockets[d], typeof(Socket), false);
        //    if (mapSegment.socket.sockets[d] != old)
        //        SceneView.RepaintAll();

        //    EditorGUILayout.EndHorizontal();
        //}

        //if(GUILayout.Button("Clear Select")) {
        //    selected = -1;
        //    SceneView.RepaintAll();
        //}
    }


    #region Scene Preview

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

            info.RenderTile(modelPos, Vector3.zero, Color.green);

            /*
            Vector3 offset = modelPos + Vector3.right * (mapGen.tileSize.x * 3) * i;
            Handles.Label(offset + Vector3.up * 3, new GUIContent(text: GetName(segment, i)));

            //info.RenderTile(modelPos, upDir, Color.green);

            var wMapSeg = segment.whitelist.GetNeighbours(i);
            if (wMapSeg != null)
                for (int map = 0; map < wMapSeg.Length; map++) {
                    TurnSegment seg = wMapSeg[map];
                    Vector3 offset2 = offset + new Vector3(0, 0, ((mapGen.tileSize.z * 3) * map));
                    info.RenderTile(offset2, Vector3.zero, Color.green);
                    TileInfo subInfo = new TileInfo(seg.segment, seg.turn);
                    subInfo.RenderTile(offset2, dir, Color.yellow);
                }*/

            DrawSegmentDirection();
        }
    }

    /*
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
    */

    struct TileInfo
    {
        int turn;
        Transform[] transformInfo;
        Mesh[] currentMeshes;
        Material[] materials;
        Vector3 prefabScale;

        public TileInfo(MapSegment segment, int turn)
        {
            currentMeshes = new Mesh[0];
            transformInfo = new Transform[0];
            materials = new Material[0];

            prefabScale = Vector3.one;
            this.turn = turn;
            if (!segment) return;
            var prefab = segment.prefab;
            if (!prefab) return;
            var meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
            currentMeshes = new Mesh[meshFilters.Length];

            materials = new Material[meshFilters.Length];
            transformInfo = new Transform[meshFilters.Length];
            this.turn = turn;
            prefabScale = prefab.transform.localScale;
            for (int i = 0; i < meshFilters.Length; i++)
            {
                currentMeshes[i] = meshFilters[i].sharedMesh;
                transformInfo[i] = meshFilters[i].transform;

                var render = meshFilters[i].GetComponent<MeshRenderer>();
                materials[i] = render.sharedMaterial;

            }

        }




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

            var turnRot = Quaternion.Euler(0, turn * 90, 0);

            for (int i = 0; i < currentMeshes.Length; i++)
            {
                Vector3 scale = new Vector3(
                    prefabScale.x * transformInfo[i].localScale.x,
                    prefabScale.y * transformInfo[i].localScale.y,
                    prefabScale.z * transformInfo[i].localScale.z);

                Vector3 pos = turn != 0 ? (turnRot * transformInfo[i].position + aPos) + anOffset : transformInfo[i].position + aPos + anOffset;
                Quaternion rot = turn != 0 ? transformInfo[i].rotation * turnRot : transformInfo[i].rotation;

                Matrix4x4 m = Matrix4x4.TRS(pos, rot, scale);
                Graphics.DrawMesh(currentMeshes[i], m, materials[i], 0, SceneView.currentDrawingSceneView.camera);

            }
        }
    }


    private void DrawSegmentDirection()
    {


        for (int d = 0; d < DirExt.directions.Length; d++)
        {
            var element = sockets.GetArrayElementAtIndex(d);


            if (selected == d)
            {
                Handles.color = selectionColor;
            }
            else if (element != null)
            {
                Handles.color = socketColors[d];
            }
            else
                Handles.color = Color.clear;


            Handles.Label((Vector3)DirExt.directions[d] * 9, socketNames[d]);
            Handles.DrawLine(Vector3.zero, (Vector3)DirExt.directions[d] * 9);
        }
    }
    #endregion


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
    */
}
