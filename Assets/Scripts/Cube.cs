using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    protected Transform _transform;
    private Transform playerTransform;
    private byte id;

    private void Awake()
    {
        playerTransform = Player.instance.GetComponent<Transform>();
        _transform = GetComponent<Transform>();
    }

    private void FixedUpdate()
    {
        if ((playerTransform.position - _transform.position).magnitude >= Player.instance.despawnDistance)
        {
            QueueActionPlayer action = new QueueActionPlayer(false, new CubeStruct(new Vector3Int((int)_transform.position.x, (int)_transform.position.y, (int)_transform.position.z), id));
            Player.instance.actionsQueue.Enqueue(action);
        }
    }

    public void SetID(byte id)
    {
        //Debug.Log(GetComponent<MeshRenderer>().sharedMaterials[0]);
        this.id = id;
    }
}
