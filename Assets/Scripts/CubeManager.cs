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
        StartCoroutine(CheckPositionPlayer());
        StartCoroutine(WriteChunkInformation());
        //StartCoroutine(WriteWorldInformation());
    }

    private void FixedUpdate()
    {
        foreach (var chunk in loadedChunks)
        {
            Debug.Log("foreach loadedChunks " + chunk.position.bytePosition);
            Debug.Log("foreach loadedChunks player " + playerTransform.position.x + " " + playerTransform.position.y + " " + playerTransform.position.z);
            var auxPos = chunk.position.position.ToVector3Float() * chunkSize;
            Debug.Log("foreach loadedChunks chunk " + auxPos.x + " " + auxPos.y + " " + auxPos.z);
            if ((playerTransform.position - (chunk.position.position.ToVector3Float() * chunkSize)).magnitude >= processedDistance)
            {
                Debug.Log("foreach loadedChunks >=processedDistance " + chunk.position.bytePosition);
                processedChunks.Enqueue(new QueueActionCube(false, chunk.position));
            }
        }
        if (processedChunks.Count > 0)
        {
            Debug.Log("processedChunks Count>0 " + processedChunks.Count);
            var chunkAction = processedChunks.Dequeue();
            var chunkPosition = chunkAction.position;
            var chunkVectorPosition = chunkPosition.position;
            if (chunkAction.add)
            {
                Debug.Log("true " + chunkVectorPosition.x);
                if ((playerTransform.position - chunkVectorPosition.ToVector3Float() * chunkSize).magnitude < processedDistance)
                {
                    Debug.Log("true <processedDistance " + chunkVectorPosition.x);
                    if (dictChunks.ContainsKey(chunkVectorPosition) && dictChunks[chunkVectorPosition] != null)
                    {
                        Debug.Log("true dictChunks Contains !=null " + chunkVectorPosition.x);
                        int halfChunkSize = chunkSize / 2;
                        for (int x = 0; x < chunkSize; x++)
                        {
                            for (int y = 0; y < chunkSize; y++)
                            {
                                for (int z = 0; z < chunkSize; z++)
                                {
                                    if (dictChunks[chunkVectorPosition][x, y, z] != 0)
                                    {
                                        var addedCube = new CubeStruct(new Vector3Int(chunkVectorPosition.x * chunkSize + x - halfChunkSize, chunkVectorPosition.y * chunkSize + y - halfChunkSize, chunkVectorPosition.z * chunkSize + z - halfChunkSize), dictChunks[chunkVectorPosition][x, y, z]);
                                        if (!Player.instance.grid[false].Contains(addedCube) && !Player.instance.grid[true].Contains(addedCube))
                                        {
                                            Player.instance.grid[false].Add(addedCube);
                                            Debug.Log("true dictChunks Contains !=null for " + chunkVectorPosition.x * chunkSize + x);
                                        }
                                    }
                                }
                            }
                        }
                        Debug.Log("true dictChunks Contains !=null after for " + chunkVectorPosition.x);
                        var chunk = new Chunk(dictChunks[chunkVectorPosition], chunkPosition);
                        if (!loadedChunks.Contains(chunk))
                        {
                            Debug.Log("true dictChunks Contains !=null loadChunks NoContains " + chunk.position.bytePosition);
                            loadedChunks.Add(chunk);
                            var rightChunk = new Chunk(null, new ChunkPosition(-1, new Vector3Short(++chunkVectorPosition.x, chunkVectorPosition.y, chunkVectorPosition.z)));
                            chunkVectorPosition.x--;
                            Debug.Log("true dictChunks Contains !=null loadChunks NoContains right " + chunkVectorPosition.x);
                            var leftChunk = new Chunk(null, new ChunkPosition(-1, new Vector3Short(--chunkVectorPosition.x, chunkVectorPosition.y, chunkVectorPosition.z)));
                            chunkVectorPosition.x++;
                            Debug.Log("true dictChunks Contains !=null loadChunks NoContains left " + chunkVectorPosition.x);
                            var upChunk = new Chunk(null, new ChunkPosition(-1, new Vector3Short(chunkVectorPosition.x, ++chunkVectorPosition.y, chunkVectorPosition.z)));
                            chunkVectorPosition.y--;
                            Debug.Log("true dictChunks Contains !=null loadChunks NoContains up " + chunkVectorPosition.y);
                            var downChunk = new Chunk(null, new ChunkPosition(-1, new Vector3Short(chunkVectorPosition.x, --chunkVectorPosition.y, chunkVectorPosition.z)));
                            chunkVectorPosition.y++;
                            Debug.Log("true dictChunks Contains !=null loadChunks NoContains down " + chunkVectorPosition.y);
                            var forwardChunk = new Chunk(null, new ChunkPosition(-1, new Vector3Short(chunkVectorPosition.x, chunkVectorPosition.y, ++chunkVectorPosition.z)));
                            chunkVectorPosition.z--;
                            Debug.Log("true dictChunks Contains !=null loadChunks NoContains forward " + chunkVectorPosition.z);
                            var backwardChunk = new Chunk(null, new ChunkPosition(-1, new Vector3Short(chunkVectorPosition.x, chunkVectorPosition.y, --chunkVectorPosition.z)));
                            chunkVectorPosition.z++;
                            Debug.Log("true dictChunks Contains !=null loadChunks NoContains backward " + chunkVectorPosition.z);
                            if (!dictChunks.ContainsKey(rightChunk.position.position))
                            {
                                Debug.Log("true dictChunks Contains !=null loadChunks NoContains right dictChunks NoContains " + chunkVectorPosition.x);
                                if (!ReadChunkInformation(rightChunk.position.position))
                                {
                                    Debug.Log("true dictChunks Contains !=null loadChunks NoContains right dictChunks NoContains !Read" + chunkVectorPosition.x);
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
                else
                {
                    processedChunks.Enqueue(new QueueActionCube(false, chunkPosition));
                }
            }
            else
            {
                Debug.Log("false " + chunkVectorPosition.x);
                if (dictChunks.ContainsKey(chunkVectorPosition))
                {
                    Debug.Log("false dictChunks Contains " + chunkVectorPosition.x);
                    var chunk = new Chunk(dictChunks[chunkVectorPosition], chunkPosition);
                    if (loadedChunks.Contains(chunk))
                    {
                        Debug.Log("false loadedChunks Contains " + chunk.position.bytePosition);
                        loadedChunks.Remove(chunk);
                    }
                    dictChunks.Remove(chunkVectorPosition);
                }
            }
        }
    }

    private bool ReadChunkInformation(Vector3Short position)
    {
        Debug.Log("Read " + position.x);
        long bytePosition = -1;
        try
        {
            Debug.Log("Read try " + position.x);
            using (var fileStreamChunk = new FileStream(fileWorld + ".bm", FileMode.Open))
            {
                Debug.Log("Read try FileStream " + position.x);
                int chunkByteSize = chunkSize * chunkSize * chunkSize;
                long fileSize = fileStreamChunk.Length;
                for (long i = 0; i < fileSize; i += 6 + chunkByteSize)
                {
                    Debug.Log("Read try FileStream for " + i);
                    Debug.Log("Read try FileStream for Seek " + SeekOrigin.Begin);
                    fileStreamChunk.Seek(i, SeekOrigin.Begin);
                    var positionX = new byte[2];
                    positionX[0] = (byte)fileStreamChunk.ReadByte();
                    positionX[1] = (byte)fileStreamChunk.ReadByte();
                    var positionY = new byte[2];
                    positionY[0] = (byte)fileStreamChunk.ReadByte();
                    positionY[1] = (byte)fileStreamChunk.ReadByte();
                    var positionZ = new byte[2];
                    positionZ[0] = (byte)fileStreamChunk.ReadByte();
                    positionZ[1] = (byte)fileStreamChunk.ReadByte();
                    Debug.Log("Read try FileStream for read " + BitConverter.ToInt16(positionX, 0) + " " + BitConverter.ToInt16(positionY, 0) + " " + BitConverter.ToInt16(positionZ, 0));
                    if (position.x == BitConverter.ToInt16(positionX, 0) && position.y == BitConverter.ToInt16(positionY, 0) && position.z == BitConverter.ToInt16(positionZ, 0))
                    {
                        Debug.Log("Read try FileStream for find " + i);
                        var bytesChunk = new byte[chunkSize, chunkSize, chunkSize];
                        bytePosition = fileStreamChunk.Position;
                        int chunkSize2D = chunkSize * chunkSize;
                        int x = 0;
                        int y = 0;
                        int z = 0;
                        bytesChunk[0, 0, 0] = (byte)fileStreamChunk.ReadByte();
                        for (int j = 1; j < chunkByteSize; j++)
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
            Debug.Log("Read false");
            return false;
        }
    FindIt:
        Debug.Log("Read true " + bytePosition);
        processedChunks.Enqueue(new QueueActionCube(true, new ChunkPosition(bytePosition, position)));
        return true;
    }

    private IEnumerator CheckPositionPlayer()
    {
        while (true)
        {
            Debug.Log("CheckPositionPlayer");
            if (!ReadChunkInformation(new Vector3Short(Convert.ToInt16(playerTransform.position.x / chunkSize), Convert.ToInt16(playerTransform.position.y / chunkSize), Convert.ToInt16(playerTransform.position.z / chunkSize))))
            {
                Debug.Log("CheckPositionPlayer Read");
                writeQueue.Enqueue(new Vector3Short(Convert.ToInt16(playerTransform.position.x / chunkSize), Convert.ToInt16(playerTransform.position.y / chunkSize), Convert.ToInt16(playerTransform.position.z / chunkSize)));
            }
            yield return new WaitWhile(() => loadedChunks.Count > 0);
        }
    }

    private IEnumerator WriteChunkInformation()
    {
        while (true)
        {
        NoFindIt:
            Debug.Log("WriteChunkInformation");
            while (writeQueue.Count > 0)
            {
                Debug.Log("WriteChunkInformation writeQueue>0");
                var position = writeQueue.Dequeue();
                try
                {
                    Debug.Log("WriteChunkInformation try " + position.x);
                    using (var fileStreamChunk = new FileStream(fileWorld + ".bm", FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        Debug.Log("WriteChunkInformation FileStream " + position.x);
                        int chunkByteSize = chunkSize * chunkSize * chunkSize;
                        if (dictChunks.ContainsKey(position))
                        {
                            Debug.Log("WriteChunkInformation dictChunks Contains " + position.x);
                            Chunk chunk;
                            foreach (var loadedChunk in loadedChunks)
                            {
                                if (loadedChunk.position.position.Equals(position) && loadedChunk.position.bytePosition != -1)
                                {
                                    chunk = loadedChunk;
                                    goto FindIt;
                                }
                            }
                            Debug.Log("WriteChunkInformation dictChunks Contains NoFindIt " + position.x);
                            writeQueue.Enqueue(position);
                            goto NoFindIt;
                        FindIt:
                            Debug.Log("WriteChunkInformation dictChunks Contains FindIt " + chunk.position.bytePosition);
                            var bytesChunk = new byte[chunkByteSize];
                            int x = 0;
                            for (int i = 0; i < chunkSize; i++)
                            {
                                for (int j = 0; j < chunkSize; j++)
                                {
                                    for (int k = 0; k < chunkSize; k++)
                                    {
                                        bytesChunk[x] = chunk.chunk[i, j, k];
                                        x++;
                                    }
                                }
                            }
                            Debug.Log("WriteChunkInformation dictChunks Contains Lock " + chunk.position.bytePosition);
                            fileStreamChunk.Seek(chunk.position.bytePosition, SeekOrigin.Begin);
                            fileStreamChunk.Lock(0, chunkByteSize);
                            Debug.Log("WriteChunkInformation dictChunks Contains Write " + chunk.position.bytePosition);
                            fileStreamChunk.Write(bytesChunk, 0, chunkByteSize);
                            fileStreamChunk.Flush();
                            fileStreamChunk.Unlock(0, chunkByteSize);
                        }
                        else
                        {
                            Debug.Log("WriteChunkInformation dictChunks NoContains " + position.x);
                            var bytesChunkInformation = new byte[6 + chunkByteSize];
                            BitConverter.GetBytes(position.x).CopyTo(bytesChunkInformation, 0);
                            BitConverter.GetBytes(position.y).CopyTo(bytesChunkInformation, 2);
                            BitConverter.GetBytes(position.z).CopyTo(bytesChunkInformation, 4);
                            for (int i = 6; i < bytesChunkInformation.Length; i++)
                            {
                                bytesChunkInformation[i] = 0;
                            }
                            Debug.Log("WriteChunkInformation dictChunks NoContains Lock " + position.x);
                            fileStreamChunk.Seek(fileStreamChunk.Length, SeekOrigin.Begin);
                            fileStreamChunk.Lock(0, bytesChunkInformation.Length);
                            Debug.Log("WriteChunkInformation dictChunks NoContains Write " + fileStreamChunk.Length);
                            fileStreamChunk.Write(bytesChunkInformation, 0, bytesChunkInformation.Length);
                            fileStreamChunk.Flush();
                            fileStreamChunk.Unlock(0, bytesChunkInformation.Length);
                        }
                    }
                }
                catch
                {
                    Debug.Log("Exception caught in process writing chunk information");
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
            int chunkByteSize = chunkSize * chunkSize * chunkSize;
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
                Debug.Log("Exception caught in process writing world information");
            }
            yield return new WaitWhile(() => loadedChunks.Equals(this.loadedChunks));
        }
    }*/
}
