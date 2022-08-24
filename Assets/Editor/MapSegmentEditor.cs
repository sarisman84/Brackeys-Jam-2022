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

            Vector3 offset = modelPos + Vector3.right * 36 * i;
            Handles.Label(offset + Vector3.up * 3, new GUIContent(text: GetName(segment, i)));

            //info.RenderTile(modelPos, upDir, Color.green);

            var wMapSeg = segment.whitelist.GetNeighbours(i);
            for (int map = 0; map < wMapSeg.Length; map++)
            {
                TurnSegment seg = wMapSeg[map];
                Vector3 offset2 = offset + new Vector3(0, 0, (18.0f * map));
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
            for (int i = 0; i < meshFilters.Length; i++)
            {
                currentMeshes[i] = meshFilters[i].sharedMesh;
                localPos[i] = meshFilters[i].transform.localPosition;
                localSizes[i] = meshFilters[i].transform.localScale;
                localRots[i] = meshFilters[i].transform.localRotation;

                var render = meshFilters[i].GetComponent<MeshRenderer>();
                materials[i] = render.sharedMaterial;

            }

        }

        Vector3[] localSizes;
        Vector3[] localPos;
        Mesh[] currentMeshes;
        Material[] materials;
        Quaternion[] localRots;
        int m_turn;


        public void RenderTile(Vector3 aPos, Vector3 anOffset, Color renderColor)
        {
            Graphics.ClearRandomWriteTargets();
            Gizmos.color = renderColor - new Color(0, 0, 0, 0.25f);
            var rot = Quaternion.Euler(0, m_turn * 90, 0);

            for (int i = 0; i < currentMeshes.Length; i++)
            {

                Matrix4x4 m = Matrix4x4.TRS((rot * localPos[i]) + aPos + anOffset, rot * localRots[i], localSizes[i]);
                Graphics.DrawMesh(currentMeshes[i], m, materials[i], 0, SceneView.currentDrawingSceneView.camera);

            }




        }

    }
}
