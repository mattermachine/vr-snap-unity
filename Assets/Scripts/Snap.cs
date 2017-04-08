using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Snap
{
    public Vector3 position;
    public Vector3 normal;

    public Snap (Vector3 position, Vector3 normal)
    {
        this.position = position;
        this.normal = normal;
    }
}