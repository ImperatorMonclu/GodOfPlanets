using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Vector3Short
{
    public short x;
    public short y;
    public short z;
    public Vector3Short(short x, short y, short z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public Vector3 ToVector3Float()
    {
        return new Vector3((float)x, (float)y, (float)z);
    }
}

public class GameManager : Singleton<GameManager>
{
    public GameObject cubeType;
}
