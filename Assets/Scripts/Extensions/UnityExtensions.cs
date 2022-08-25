﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEditor;
public static class UnityExtensions
{
    public static NewType Cast<OldType, NewType>(this OldType original)
    {
        NewType result = default;
        switch (original)
        {
            case Vector3 vector3:

                switch (result)
                {
                    case Vector3Int sV3Int:
                        int x = Mathf.FloorToInt(vector3.x);
                        int y = Mathf.FloorToInt(vector3.y);
                        int z = Mathf.FloorToInt(vector3.z);

                        result = AssignValue<NewType, Vector3Int>(new Vector3Int(x, y, z));
                        break;
                    case Vector2Int sV2Int:
                        x = Mathf.FloorToInt(vector3.x);
                        y = Mathf.FloorToInt(vector3.y);
                        result = AssignValue<NewType, Vector2Int>(new Vector2Int(x, y));
                        break;

                    default:
                        break;
                }

                break;

            default:
                break;
        }


        return result;
    }
    private static T AssignValue<T, A>(A value)
    {
        return (T)Convert.ChangeType(value, typeof(T));
    }



    public static T LoadAssetFromUniqueAssetPath<T>(string aAssetPath) where T : UnityEngine.Object
    {
        if (aAssetPath.Contains("::"))
        {
            string[] parts = aAssetPath.Split(new string[] { "::" }, System.StringSplitOptions.RemoveEmptyEntries);
            aAssetPath = parts[0];
            if (parts.Length > 1)
            {
                string assetName = parts[1];
                System.Type t = typeof(T);
                var assets = AssetDatabase.LoadAllAssetsAtPath(aAssetPath)
                    .Where(i => t.IsAssignableFrom(i.GetType())).Cast<T>();
                var obj = assets.Where(i => i.name == assetName).FirstOrDefault();
                if (obj == null)
                {
                    int id;
                    if (int.TryParse(parts[1], out id))
                        obj = assets.Where(i => i.GetInstanceID() == id).FirstOrDefault();
                }
                if (obj != null)
                    return obj;
            }
        }
        return AssetDatabase.LoadAssetAtPath<T>(aAssetPath);
    }
    public static string GetUniqueAssetPath(UnityEngine.Object aObj)
    {
        string path = AssetDatabase.GetAssetPath(aObj);
        if (!string.IsNullOrEmpty(aObj.name))
            path += "::" + aObj.name;
        else
            path += "::" + aObj.GetInstanceID();
        return path;
    }



    public static int GetRndm(int length) {
        return UnityEngine.Random.Range(0, length);
    }

    public static Vector3 CompMul(this Vector3 vec1, Vector3 vec2) {
        return new Vector3(vec1.x * vec2.x, vec1.y * vec2.y, vec1.z * vec2.z);
    }
    public static Vector3 CompDiv(this Vector3 vec1, Vector3 vec2) {
        return new Vector3(vec1.x / vec2.x, vec1.y / vec2.y, vec1.z / vec2.z);
    }
}

