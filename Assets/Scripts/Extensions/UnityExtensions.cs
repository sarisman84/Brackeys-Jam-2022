using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
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

}

