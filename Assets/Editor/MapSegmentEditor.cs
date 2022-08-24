using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(MapSegment))]
public class MapSegmentEditor : Editor
{

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
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
}
