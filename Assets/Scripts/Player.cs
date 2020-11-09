using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public struct QueueActionPlayer
{
    public bool add;
    public CubeStruct cube;
    public QueueActionPlayer(bool add, CubeStruct cube)
    {
        this.add = add;
        this.cube = cube;
    }
}
public struct CubeStruct
{
    public Vector3Int position;
    public byte id;
    public CubeStruct(Vector3Int position, byte id)
    {
        this.position = position;
        this.id = id;
    }
}

public class Player : Singleton<Player>
{
    [Range(0.1f, 4.0f)]
    public float sensitivity = 2;
    [Range(0.0f, 120.0f)]
    public float maxYAngle = 90;
    [Range(16, 512)]
    public int despawnDistance = 128;
    [Range(0.1f, 4.0f)]
    public float movementSpeed = 2;
    public string filePlayer = "Saves/Player";

    public Dictionary<bool, List<CubeStruct>> grid;
    public Queue<QueueActionPlayer> actionsQueue;
    protected Transform _transform;
    private Vector2 rotation;
    private GameObject cube;
    private Dictionary<Vector3Int, GameObject> dictObjects;

    private void Awake()
    {
        grid = new Dictionary<bool, List<CubeStruct>>();
        grid[true] = new List<CubeStruct>();
        grid[false] = new List<CubeStruct>();
        actionsQueue = new Queue<QueueActionPlayer>();
        this._transform = GetComponent<Transform>();
        cube = GameManager.instance.cubeType;
        dictObjects = new Dictionary<Vector3Int, GameObject>();
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
                _transform.position = new Vector3(BitConverter.ToSingle(positionX, 0), BitConverter.ToSingle(positionY, 0), BitConverter.ToSingle(positionZ, 0));
            }
        }
        catch
        {
            Debug.Log("Exception caught in process reading player information");
        }
        StartCoroutine(UpdatePlayerInformation());
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            _transform.position += _transform.right * movementSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            _transform.position -= _transform.right * movementSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            _transform.position += _transform.up * movementSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            _transform.position -= _transform.up * movementSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.W))
        {
            _transform.position += _transform.forward * movementSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            _transform.position -= _transform.forward * movementSpeed * Time.deltaTime;
        }

        var y = Input.GetAxis("Mouse Y");
        var x = Input.GetAxis("Mouse X");
        if (y != 0.0f && x != 0.0f)
        {
            rotation = new Vector2(Mathf.Clamp(rotation.x - y * sensitivity, -maxYAngle, maxYAngle), Mathf.Repeat(rotation.y + x * sensitivity, 360));
            _transform.rotation = Quaternion.Euler(rotation.x, rotation.y, 0);
        }
    }

    private void FixedUpdate()
    {
        foreach (var cubeStruct in grid[false])
        {
            if ((_transform.position - cubeStruct.position).magnitude < despawnDistance)
            {
                var action = new QueueActionPlayer(true, cubeStruct);
                actionsQueue.Enqueue(action);
            }
        }
    NoContains:
        while (actionsQueue.Count > 0)
        {
            var queueAction = actionsQueue.Dequeue();
            if (queueAction.add)
            {
                foreach (var cubeStruct in grid[false])
                {
                    if (queueAction.cube.position == cubeStruct.position)
                    {
                        goto Contains;
                    }
                }
                goto NoContains;
            Contains:
                grid[false].Remove(queueAction.cube);
                grid[true].Add(queueAction.cube);
                cube.GetComponent<Cube>().SetID(queueAction.cube.id);
                dictObjects[queueAction.cube.position] = PoolManager.instance.Spawn(cube, queueAction.cube.position, Quaternion.identity);

            }
            else
            {
                foreach (var cubeStruct in grid[true])
                {
                    if (queueAction.cube.position == cubeStruct.position)
                    {
                        goto Contains;
                    }
                }
                goto NoContains;
            Contains:
                grid[true].Remove(queueAction.cube);
                grid[false].Add(queueAction.cube);
                PoolManager.instance.Despawn(dictObjects[queueAction.cube.position]);
                dictObjects[queueAction.cube.position] = null;
            }
        }
    }

    private IEnumerator UpdatePlayerInformation()
    {
        while (true)
        {
            var playerPosition = _transform.position;
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
            yield return new WaitWhile(() => _transform.position.Equals(playerPosition));
        }
    }
}
