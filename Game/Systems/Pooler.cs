using UnityEngine.Pool;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Pool
{
    public string tag;
    public GameObject prefab;
    public int size;
    public int maxSize;
}

public class Pooler : MonoBehaviour
{
    public static Pooler Instance;
    public List<Pool> pools;
    public static Dictionary<string, ObjectPool<GameObject>> poolDictionary;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        poolDictionary = new();

        foreach (Pool pool in pools)
        {
            ObjectPool<GameObject> objectPool = new(() => 
                {
                    return Instantiate(pool.prefab);
                }, obj => 
                {
                    obj.SetActive(true);
                }, obj => 
                {
                    obj.SetActive(false);
                }, obj => 
                {
                    Destroy(obj);
                }, false, pool.size, pool.maxSize
            );

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        GameObject gameObject = poolDictionary[tag].Get();
        gameObject.transform.SetPositionAndRotation(position, rotation);

        return gameObject;
    }

    public void DespawnToPool(string tag, GameObject objectToDespawn)
    {
        poolDictionary[tag].Release(objectToDespawn);
    }
}
