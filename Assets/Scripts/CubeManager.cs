using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public struct QueueActionCube
{
    public bool add;
    public ChunkPosition position;
    public QueueActionCube(bool add, ChunkPosition position)
    {
        this.add = add;
        this.position = position;
    }
}
public struct Chunk
{
    public byte[,,] chunk;
    public ChunkPosition position;
    public Chunk(byte[,,] chunk, ChunkPosition position)
    {
        this.chunk = chunk;
        this.position = position;
    }
}
public struct ChunkPosition
{
    public long bytePosition;
    public Vector3Short position;
    public ChunkPosition(long bytePosition, Vector3Short position)
    {
        this.bytePosition = bytePosition;
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

    private List<Chunk> loadedChunks;
    private Queue<QueueActionCube> processedChunks;
    private Dictionary<Vector3Short, byte[,,]> dictChunks;
    private Queue<Vector3Short> writeQueue;
    private Transform playerTransform;

    private void Awake()
    {
        processedDistance *= chunkSize;
        loadedChunks = new List<Chunk>();
        processedChunks = new Queue<QueueActionCube>();
        dictChunks = new Dictionary<Vector3Short, byte[,,]>();
        writeQueue = new Queue<Vector3Short>();
        playerTransform = Player.instance.GetComponent<Transform>();
    }

    private void Start()
    {
        if (!ReadChunkInformation(new Vector3Short(Convert.ToInt16(playerTransform.position.x / chunkSize), Convert.ToInt16(playerTransform.position.y / chunkSize), Convert.ToInt16(playerTransform.position.z / chunkSize))))
        {
            writeQueue.Enqueue(new Vector3Short(Convert.ToInt16(playerTransform.position.x / chunkSize), Convert.ToInt16(playerTransform.position.y / chunkSize), Convert.ToInt16(playerTransform.position.z / chunkSize)));
        }
        StartCoroutine(WriteChunkInformation());
        //StartCoroutine(WriteWorldInformation());
    }

    private void FixedUpdate()
    {
        foreach (var chunk in loadedChunks)
        {
            if ((playerTransform.position - chunk.position.position.ToVector3Float() * chunkSize).magnitude >= processedDistance)
            {
                processedChunks.Enqueue(new QueueActionCube(false, chunk.position));
            }
        }
        if (processedChunks.Count > 0)
        {
            Debug.Log(processedChunks.Count);
            Debug.Log(loadedChunks.Count);
            Debug.Log(dictChunks.Count);
            var chunkAction = processedChunks.Dequeue();
            var chunkPosition = chunkAction.position;
            var chunkVectorPosition = chunkPosition.position;
            if (chunkAction.add)
            {
                if ((playerTransform.position - chunkVectorPosition.ToVector3Float() * chunkSize).magnitude < processedDistance)
                {
                    if (dictChunks.ContainsKey(chunkVectorPosition) && dictChunks[chunkVectorPosition] != null)
                    {
                        var halfChunkSize = (int)chunkSize / 2;
                        for (var x = 0; x < chunkSize; x++)
                        {
                            for (var y = 0; y < chunkSize; y++)
                            {
                                for (var z = 0; z < chunkSize; z++)
                                {
                                    if (dictChunks[chunkVectorPosition][x, y, z] != 0)
                                    {
                                        var addedCube = new CubeStruct(new Vector3Int(chunkVectorPosition.x * chunkSize + x - halfChunkSize, chunkVectorPosition.y * chunkSize + y - halfChunkSize, chunkVectorPosition.z * chunkSize + z - halfChunkSize), dictChunks[chunkVectorPosition][x, y, z]);
                                        if (!Player.instance.grid[false].Contains(addedCube) && !Player.instance.grid[true].Contains(addedCube))
                                        {
                                            Player.instance.grid[false].Add(addedCube);
                                        }
                                    }
                                }
                            }
                        }
                        var chunk = new Chunk(dictChunks[chunkVectorPosition], chunkPosition);
                        if (!loadedChunks.Contains(chunk))
                        {
                            loadedChunks.Add(chunk);
                            var rightChunk = new Chunk(null, new ChunkPosition(-1, new Vector3Short(++chunkVectorPosition.x, chunkVectorPosition.y, chunkVectorPosition.z)));
                            chunkVectorPosition.x--;
                            var leftChunk = new Chunk(null, new ChunkPosition(-1, new Vector3Short(--chunkVectorPosition.x, chunkVectorPosition.y, chunkVectorPosition.z)));
                            chunkVectorPosition.x++;
                            var upChunk = new Chunk(null, new ChunkPosition(-1, new Vector3Short(chunkVectorPosition.x, ++chunkVectorPosition.y, chunkVectorPosition.z)));
                            chunkVectorPosition.y--;
                            var downChunk = new Chunk(null, new ChunkPosition(-1, new Vector3Short(chunkVectorPosition.x, --chunkVectorPosition.y, chunkVectorPosition.z)));
                            chunkVectorPosition.y++;
                            var forwardChunk = new Chunk(null, new ChunkPosition(-1, new Vector3Short(chunkVectorPosition.x, chunkVectorPosition.y, ++chunkVectorPosition.z)));
                            chunkVectorPosition.z--;
                            var backwardChunk = new Chunk(null, new ChunkPosition(-1, new Vector3Short(chunkVectorPosition.x, chunkVectorPosition.y, --chunkVectorPosition.z)));
                            chunkVectorPosition.z++;
                            if (!dictChunks.ContainsKey(rightChunk.position.position))
                            {
                                if (!ReadChunkInformation(rightChunk.position.position))
                                {
                                    writeQueue.Enqueue(rightChunk.position.position);
                                    processedChunks.Enqueue(new QueueActionCube(true, rightChunk.position));
                                }
                            }
                            if (!dictChunks.ContainsKey(leftChunk.position.position))
                            {
                                if (!ReadChunkInformation(leftChunk.position.position))
                                {
                                    writeQueue.Enqueue(leftChunk.position.position);
                                    processedChunks.Enqueue(new QueueActionCube(true, leftChunk.position));
                                }
                            }
                            if (!dictChunks.ContainsKey(upChunk.position.position))
                            {
                                if (!ReadChunkInformation(upChunk.position.position))
                                {
                                    writeQueue.Enqueue(upChunk.position.position);
                                    processedChunks.Enqueue(new QueueActionCube(true, upChunk.position));
                                }
                            }
                            if (!dictChunks.ContainsKey(downChunk.position.position))
                            {
                                if (!ReadChunkInformation(downChunk.position.position))
                                {
                                    writeQueue.Enqueue(downChunk.position.position);
                                    processedChunks.Enqueue(new QueueActionCube(true, downChunk.position));
                                }
                            }
                            if (!dictChunks.ContainsKey(forwardChunk.position.position))
                            {
                                if (!ReadChunkInformation(forwardChunk.position.position))
                                {
                                    writeQueue.Enqueue(forwardChunk.position.position);
                                    processedChunks.Enqueue(new QueueActionCube(true, forwardChunk.position));
                                }
                            }
                            if (!dictChunks.ContainsKey(backwardChunk.position.position))
                            {
                                if (!ReadChunkInformation(backwardChunk.position.position))
                                {
                                    writeQueue.Enqueue(backwardChunk.position.position);
                                    processedChunks.Enqueue(new QueueActionCube(true, backwardChunk.position));
                                }
                            }
                        }

                    }
                    else
                    {
                        ReadChunkInformation(chunkPosition.position);
                        processedChunks.Enqueue(chunkAction);
                    }
                }
            }
            else
            {
                if (dictChunks.ContainsKey(chunkVectorPosition))
                {
                    var chunk = new Chunk(dictChunks[chunkVectorPosition], chunkPosition);
                    if (loadedChunks.Contains(chunk))
                    {
                        loadedChunks.Remove(chunk);
                    }
                    dictChunks.Remove(chunkVectorPosition);
                }
            }
        }
    }

    private bool ReadChunkInformation(Vector3Short position)
    {
        var bytePosition = (long)-1;
        try
        {
            using (var fileStreamChunk = new FileStream(fileWorld + ".bm", FileMode.Open))
            {
                var chunkByteSize = chunkSize * chunkSize * chunkSize;
                for (var i = 0; i < fileStreamChunk.Length; i += 6 + chunkByteSize)
                {
                    fileStreamChunk.Seek(i, SeekOrigin.Begin);
                    var positionX = new byte[2];
                    for (var j = 0; j < 2; j++)
                    {
                        positionX[j] = (byte)fileStreamChunk.ReadByte();
                    }
                    var positionY = new byte[2];
                    for (var j = 0; j < 2; j++)
                    {
                        positionY[j] = (byte)fileStreamChunk.ReadByte();
                    }
                    var positionZ = new byte[2];
                    for (var j = 0; j < 2; j++)
                    {
                        positionZ[j] = (byte)fileStreamChunk.ReadByte();
                    }
                    if (position.x == BitConverter.ToInt16(positionX, 0) && position.y == BitConverter.ToInt16(positionY, 0) && position.z == BitConverter.ToInt16(positionZ, 0))
                    {
                        var bytesChunk = new byte[chunkSize, chunkSize, chunkSize];
                        bytePosition = fileStreamChunk.Position;
                        var chunkSize2D = chunkSize * chunkSize;
                        var x = 0;
                        var y = 0;
                        var z = 0;
                        bytesChunk[0, 0, 0] = (byte)fileStreamChunk.ReadByte();
                        for (var j = 1; j < chunkByteSize; j++)
                        {
                            if (j % chunkSize == 0)
                            {
                                y++;
                                z = 0;
                            }
                            if (j % chunkSize2D == 0)
                            {
                                x++;
                                y = 0;
                                z = 0;
                            }
                            bytesChunk[x, y, z] = (byte)fileStreamChunk.ReadByte();
                            z++;
                        }
                        dictChunks[position] = bytesChunk;
                        goto FindIt;
                    }
                }
            }
        }
        catch
        {
            Debug.Log("Exception caught in process reading chunk information");
            return false;
        }
        if (bytePosition == -1)
        {
            return false;
        }
    FindIt:
        processedChunks.Enqueue(new QueueActionCube(true, new ChunkPosition(bytePosition, position)));
        return true;
    }

    private IEnumerator WriteChunkInformation()
    {
        while (true)
        {
        NoFindIt:
            while (writeQueue.Count > 0)
            {
                var position = writeQueue.Dequeue();
                try
                {
                    using (var fileStreamChunk = new FileStream(fileWorld + ".bm", FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        var chunkByteSize = chunkSize * chunkSize * chunkSize;
                        if (dictChunks.ContainsKey(position))
                        {
                            Chunk chunk;
                            foreach (var loadedChunk in loadedChunks)
                            {
                                if (loadedChunk.position.position.Equals(position) && loadedChunk.position.bytePosition != -1)
                                {
                                    chunk = loadedChunk;
                                    goto FindIt;
                                }
                            }
                            goto NoFindIt;
                        FindIt:
                            var bytesChunk = new byte[chunkByteSize];
                            var x = 0;
                            for (var i = 0; i < chunkSize; i++)
                            {
                                for (var j = 0; j < chunkSize; j++)
                                {
                                    for (var k = 0; k < chunkSize; k++)
                                    {
                                        bytesChunk[x] = chunk.chunk[i, j, k];
                                        x++;
                                    }
                                }
                            }
                            fileStreamChunk.Seek(chunk.position.bytePosition, SeekOrigin.Begin);
                            fileStreamChunk.Lock(0, chunkByteSize);
                            fileStreamChunk.Write(bytesChunk, 0, chunkByteSize);
                            fileStreamChunk.Flush();
                            fileStreamChunk.Unlock(0, chunkByteSize);
                        }
                        else
                        {
                            var bytesChunkInformation = new byte[6 + chunkByteSize];
                            BitConverter.GetBytes(position.x).CopyTo(bytesChunkInformation, 0);
                            BitConverter.GetBytes(position.y).CopyTo(bytesChunkInformation, 2);
                            BitConverter.GetBytes(position.z).CopyTo(bytesChunkInformation, 4);
                            for (var i = 5; i < bytesChunkInformation.Length; i++)
                            {
                                bytesChunkInformation[i] = 0;
                            }
                            fileStreamChunk.Seek(fileStreamChunk.Length, SeekOrigin.Begin);
                            fileStreamChunk.Lock(0, bytesChunkInformation.Length);
                            fileStreamChunk.Write(bytesChunkInformation, 0, bytesChunkInformation.Length);
                            fileStreamChunk.Flush();
                            fileStreamChunk.Unlock(0, bytesChunkInformation.Length);
                        }
                    }
                }
                catch
                {
                    Debug.Log("Exception caught in process writting chunk information");
                }
            }
            yield return new WaitWhile(() => writeQueue.Count <= 0);
        }
    }

    /*private IEnumerator WriteWorldInformation()
    {
        while (true)
        {
            var loadedChunks = this.loadedChunks;
            var chunkByteSize = chunkSize * chunkSize * chunkSize;
            try
            {
                using (var fileStreamWorld = new FileStream(fileWorld + ".bm", FileMode.Open))
                {
                    foreach (var chunk in loadedChunks)
                    {

                    }
                }
            }
            catch
            {
                Debug.Log("Exception caught in process writting world information");
            }
            yield return new WaitWhile(() => loadedChunks.Equals(this.loadedChunks));
        }
    }*/
}
