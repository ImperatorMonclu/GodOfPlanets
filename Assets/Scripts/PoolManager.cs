using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<string, List<GameObject>> pool;
    private Transform poolParent;


    private void Awake()
    {
        poolParent = new GameObject("Pool").transform;
        pool = new Dictionary<string, List<GameObject>>();
    }

    public void Load(GameObject prefab, uint amount = 1)
    {

        if (!pool.ContainsKey(prefab.name))
        {
            pool[prefab.name] = new List<GameObject>();
        }

        //Bad practices
        if (pool[prefab.name].Count < 32)
        {
            for (int i = 0; i < amount; i++)
            {
                var go = Instantiate(prefab);
                go.name = prefab.name;
                go.SetActive(false);
                go.transform.parent = poolParent;
                pool[prefab.name].Add(go);
            }
        }
    }

    public bool Spawn(GameObject prefab)
    {
        return (Spawn(prefab, prefab.transform.position, prefab.transform.rotation) != null);

    }

    public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (!pool.ContainsKey(prefab.name) || pool[prefab.name].Count == 0)
        {
            Load(prefab);
        }
        var go = pool[prefab.name][0];
        pool[prefab.name].RemoveAt(0);
        go.transform.parent = null;
        var t = go.GetComponent<Transform>();
        t.position = pos;
        t.rotation = rot;
        go.SetActive(true);
        return go;
    }

    public void Despawn(GameObject go)
    {
        if (go != null)
        {
            //Bad practices
            if (pool[go.name].Count < 32)
            {
                go.SetActive(false);
                go.transform.parent = poolParent;
                pool[go.name].Add(go);
            }
            else
            {
                Destroy(go);
            }
        }
    }
}
