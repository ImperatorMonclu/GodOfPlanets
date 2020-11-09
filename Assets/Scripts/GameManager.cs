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
    //public Vector3Int ToVector3Int()
    //{
    //    return new Vector3Int((int)x, (int)y, (int)z);
    //}
    public Vector3 ToVector3Float()
    {
        return new Vector3((float)x, (float)y, (float)z);
    }
}

public class GameManager : Singleton<GameManager>
{
    public GameObject cubeType;
    public uint size;

    private void Start()
    {
        for (int x = 0; x < size; ++x)
        {
            for (int y = 0; y < size; ++y)
            {
                for (int z = 0; z < size; ++z)
                {
                    Player.instance.grid[false].Add(new CubeStruct(new Vector3Int(x, y, z), 1));
                }
            }
        }
    }
}
