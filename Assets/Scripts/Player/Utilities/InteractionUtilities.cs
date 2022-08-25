using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class InteractionUtilities
{
    public static bool IntersectFromCamera(Camera aCamera, float maxRange, LayerMask mask, out RaycastHit hitInfo)
    {
        Ray ray = aCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        bool intersect = Physics.Raycast(ray, out hitInfo, maxRange, mask);

        return intersect;
    }

    public static bool IntersectFromCamera(Camera aCamera, Vector3 addedDirection, float maxRange, LayerMask mask, out RaycastHit hitInfo)
    {
        Ray ray = aCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        ray.direction += addedDirection;
        bool intersect = Physics.Raycast(ray, out hitInfo, maxRange, mask);

        return intersect;
    }


    public static bool Raycast(Ray ray, Vector3 addedDirection, float maxRange, LayerMask mask, out RaycastHit hitInfo)
    {
        ray.direction += addedDirection;
        bool intersect = Physics.Raycast(ray, out hitInfo, maxRange, mask);

        return intersect;
    }

}

