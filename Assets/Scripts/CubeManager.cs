using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public struct Chunk
{
    public byte[,,] chunk;
    public Vector3Short position;
    public Chunk(byte[,,] chunk, Vector3Short position)
    {
        this.chunk = chunk;
        this.position = position;
    }
}

public class CubeManager : Singleton<CubeManager>
{
    [Range(1, 64)]
    public int chunkSize = 16;
    [Range(2, 64)]
    public float processedDistance = 8;
    public string fileWorld = "Saves/World";
    public string filePlayer = "Saves/Player";

    private List<Chunk> loadedChunks;
    private Queue<Chunk> processedChunks;
    private Transform playerTransform;

    private void Awake()
    {
        playerTransform = Player.instance.GetComponent<Transform>();
        loadedChunks = new List<Chunk>();
        processedChunks = new Queue<Chunk>();
        try
        {
            using (var fileStreamPlayer = new FileStream(filePlayer + ".bm", FileMode.Open))
            {
                var positionX = new byte[4];
                for (var i = 0; i < 4; i++)
                {
                    positionX[i] = (byte)fileStreamPlayer.ReadByte();
                }
                var positionY = new byte[4];
                for (var i = 0; i < 4; i++)
                {
                    positionY[i] = (byte)fileStreamPlayer.ReadByte();
                }
                var positionZ = new byte[4];
                for (var i = 0; i < 4; i++)
                {
                    positionZ[i] = (byte)fileStreamPlayer.ReadByte();
                }
                Player.instance.transform.position = new Vector3(BitConverter.ToSingle(positionX, 0), BitConverter.ToSingle(positionY, 0), BitConverter.ToSingle(positionZ, 0));
            }
        }
        catch
        {
            Debug.Log("Exception caught in process reading player information");
        }
    }

    private void Start()
    {
        //StartCoroutine(ReadWorldInfo());
        //StartCoroutine(WriteWorldInfo());
        StartCoroutine(UpdatePlayerInfo());
    }

    private void FixedUpdate()
    {
        var playerPosition = playerTransform.position;
        if (processedChunks.Count > 0)
        {
            var chunk = processedChunks.Dequeue();
            var chunkPosition = chunk.position;
            if ((playerTransform.position - chunkPosition.ToVector3Float() * chunkSize).magnitude < processedDistance && chunk.chunk != null)
            {
                var halfChunkSize = (int)chunkSize / 2;
                for (var x = 0; x < chunkSize; x++)
                {
                    for (var y = 0; y < chunkSize; y++)
                    {
                        for (var z = 0; z < chunkSize; z++)
                        {
                            if (chunk.chunk[x, y, z] != 0)
                            {
                                var addedChunk = new CubeStruct(new Vector3Int(chunkPosition.x * chunkSize + x - halfChunkSize, chunkPosition.y * chunkSize + y - halfChunkSize, chunkPosition.z * chunkSize + z - halfChunkSize), chunk.chunk[x, y, z]);
                                if (!Player.instance.grid[false].Contains(addedChunk))
                                {
                                    Player.instance.grid[false].Add(addedChunk);
                                }
                            }
                        }
                    }
                }
                if (!loadedChunks.Contains(chunk))
                {
                    loadedChunks.Add(chunk);
                    var forwardChunk = new Chunk(null, new Vector3Short(++chunkPosition.x, chunkPosition.y, chunkPosition.z));
                    chunkPosition.x--;
                    var backwardChunk = new Chunk(null, new Vector3Short(--chunkPosition.x, chunkPosition.y, chunkPosition.z));
                    chunkPosition.x++;
                    var rightChunk = new Chunk(null, new Vector3Short(chunkPosition.x, ++chunkPosition.y, chunkPosition.z));
                    chunkPosition.y++;
                    var leftChunk = new Chunk(null, new Vector3Short(chunkPosition.x, --chunkPosition.y, chunkPosition.z));
                    chunkPosition.y--;
                    var upChunk = new Chunk(null, new Vector3Short(chunkPosition.x, chunkPosition.y, ++chunkPosition.z));
                    chunkPosition.z++;
                    var downChunk = new Chunk(null, new Vector3Short(chunkPosition.x, chunkPosition.y, --chunkPosition.z));
                    chunkPosition.z--;
                    if (!loadedChunks.Contains(forwardChunk) && !processedChunks.Contains(forwardChunk))
                    {
                        processedChunks.Enqueue(forwardChunk);
                    }
                    if (!loadedChunks.Contains(backwardChunk) && !processedChunks.Contains(backwardChunk))
                    {
                        processedChunks.Enqueue(backwardChunk);
                    }
                    if (!loadedChunks.Contains(rightChunk) && !processedChunks.Contains(rightChunk))
                    {
                        processedChunks.Enqueue(rightChunk);
                    }
                    if (!loadedChunks.Contains(leftChunk) && !processedChunks.Contains(leftChunk))
                    {
                        processedChunks.Enqueue(leftChunk);
                    }
                    if (!loadedChunks.Contains(upChunk) && !processedChunks.Contains(upChunk))
                    {
                        processedChunks.Enqueue(upChunk);
                    }
                    if (!loadedChunks.Contains(downChunk) && !processedChunks.Contains(downChunk))
                    {
                        processedChunks.Enqueue(downChunk);
                    }
                }
            }
            else
            {
                processedChunks.Enqueue(chunk);
            }
        }
    }

    private IEnumerator ReadWorldInfo()
    {
        while (true)
        {
            var processedChunks = this.processedChunks;
            var loadedChunks = this.loadedChunks;
            var chunkByteSize = chunkSize * chunkSize * chunkSize;
            try
            {
                using (var fileStreamPlayer = new FileStream(fileWorld + ".bm", FileMode.Open))
                {
                    Chunk[] auxiliarChunks = new Chunk[processedChunks.Count];
                    processedChunks.CopyTo(auxiliarChunks, 0);
                    for (var i = 0; i < fileStreamPlayer.Length; i += chunkByteSize)
                    {
                        var positionX = new byte[2];
                        positionX[i++] = (byte)fileStreamPlayer.ReadByte();
                        positionX[i++] = (byte)fileStreamPlayer.ReadByte();
                        var positionY = new byte[2];
                        positionY[i++] = (byte)fileStreamPlayer.ReadByte();
                        positionY[i++] = (byte)fileStreamPlayer.ReadByte();
                        var positionZ = new byte[2];
                        positionZ[i++] = (byte)fileStreamPlayer.ReadByte();
                        positionZ[i++] = (byte)fileStreamPlayer.ReadByte();
                        var positionShortX = BitConverter.ToInt16(positionX, 0);
                        var positionShortY = BitConverter.ToInt16(positionY, 0);
                        var positionShortZ = BitConverter.ToInt16(positionZ, 0);
                        if (processedChunks.Contains(new Chunk(null, new Vector3Short(positionShortX, positionShortY, positionShortZ))))
                        {
                            for (var j = 0; j < auxiliarChunks.Length; j++)
                            {
                                if (positionShortX == auxiliarChunks[j].position.x && positionShortY == auxiliarChunks[j].position.y && positionShortZ == auxiliarChunks[j].position.z)
                                {
                                    var bytesChunk = new byte[chunkByteSize];
                                    for (var k = 0; k < chunkByteSize; k++)
                                    {
                                        bytesChunk[k] = (byte)fileStreamPlayer.ReadByte();
                                    }
                                }
                            }
                        }
                        else
                        {
                            fileStreamPlayer.Position += chunkByteSize;
                        }
                    }
                }
            }
            catch
            {
                Debug.Log("Exception caught in process reading world information");
            }
            yield return new WaitWhile(() => processedChunks.Equals(this.processedChunks) && loadedChunks.Equals(this.loadedChunks));
        }
    }

    private IEnumerator WriteWorldInfo()
    {
        while (true)
        {
            var loadedChunks = this.loadedChunks;
            var chunkByteSize = chunkSize * chunkSize * chunkSize;
            try
            {
                using (var fileStreamPlayer = new FileStream(fileWorld + ".bm", FileMode.Create))
                {
                    Chunk[] auxiliarChunks = new Chunk[processedChunks.Count];
                    processedChunks.CopyTo(auxiliarChunks, 0);
                    for (var i = 0; i < fileStreamPlayer.Length; i += chunkByteSize)
                    {
                        var positionX = new byte[2];
                        positionX[i++] = (byte)fileStreamPlayer.ReadByte();
                        positionX[i++] = (byte)fileStreamPlayer.ReadByte();
                        var positionY = new byte[2];
                        positionY[i++] = (byte)fileStreamPlayer.ReadByte();
                        positionY[i++] = (byte)fileStreamPlayer.ReadByte();
                        var positionZ = new byte[2];
                        positionZ[i++] = (byte)fileStreamPlayer.ReadByte();
                        positionZ[i++] = (byte)fileStreamPlayer.ReadByte();
                        var positionShortX = BitConverter.ToInt16(positionX, 0);
                        var positionShortY = BitConverter.ToInt16(positionY, 0);
                        var positionShortZ = BitConverter.ToInt16(positionZ, 0);
                        if (processedChunks.Contains(new Chunk(null, new Vector3Short(positionShortX, positionShortY, positionShortZ))))
                        {
                            for (var j = 0; j < auxiliarChunks.Length; j++)
                            {
                                if (positionShortX == auxiliarChunks[j].position.x && positionShortY == auxiliarChunks[j].position.y && positionShortZ == auxiliarChunks[j].position.z)
                                {
                                    var bytesChunk = new byte[chunkByteSize];
                                    for (var k = 0; k < chunkByteSize; k++)
                                    {
                                        bytesChunk[k] = (byte)fileStreamPlayer.ReadByte();
                                    }
                                }
                            }
                        }
                        else
                        {
                            fileStreamPlayer.Position += chunkByteSize;
                        }
                    }
                }
            }
            catch
            {
                Debug.Log("Exception caught in process writting world information");
            }
            yield return new WaitWhile(() => loadedChunks.Equals(this.loadedChunks));
        }
    }

    private IEnumerator UpdatePlayerInfo()
    {
        while (true)
        {
            var playerPosition = Player.instance.transform.position;
            try
            {
                using (var fileStreamPlayer = new FileStream(filePlayer + ".bm", FileMode.Create))
                {

                    var bytePlayerPosition = new byte[12];
                    BitConverter.GetBytes(playerPosition.x).CopyTo(bytePlayerPosition, 0);
                    BitConverter.GetBytes(playerPosition.y).CopyTo(bytePlayerPosition, 4);
                    BitConverter.GetBytes(playerPosition.z).CopyTo(bytePlayerPosition, 8);
                    fileStreamPlayer.Write(bytePlayerPosition, 0, 12);
                }
            }
            catch
            {
                Debug.Log("Exception caught in process writting player information");
            }
            yield return new WaitWhile(() => Player.instance.transform.position.Equals(playerPosition));
        }
    }
}
