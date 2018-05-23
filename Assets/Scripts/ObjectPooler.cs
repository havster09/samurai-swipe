using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{

    public static ObjectPooler Instance;
    public List<ObjectPoolItem> itemsToPool;
    public List<GameObject> pooledObjects;
    [HideInInspector] public string[] BloodDecapitationEffects;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BloodDecapitationEffects = new string[] { "BloodEffect4", "BloodEffect5" };
        pooledObjects = new List<GameObject>();
        foreach (ObjectPoolItem item in itemsToPool)
        {
            for (int i = 0; i < item.amountToPool; i++)
            {
                GameObject obj = Instantiate(item.objectToPool);
                obj.SetActive(false);
                pooledObjects.Add(obj);
            }
        }
    }

    public GameObject GetPooledObject(string tag, string name = null)
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                if(name != null && Utilities.ReplaceClone(pooledObjects[i].name) == name)
                {
                    return pooledObjects[i];
                }
                else if (name == null && pooledObjects[i].tag == tag)
                {
                    return pooledObjects[i];
                }
            }
        }
        foreach (ObjectPoolItem item in itemsToPool)
        {
            if (item.objectToPool.tag == tag)
            {
                if (item.shouldExpand)
                {
                    GameObject obj = (GameObject)Instantiate(item.objectToPool);
                    obj.SetActive(false);
                    pooledObjects.Add(obj);
                    return obj;
                }
            }
        }
        return null;
    }

    public void InitializeAllEnemies(string tag)
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy && pooledObjects[i].tag == tag)
            {
                pooledObjects[i].transform.position = new Vector3(pooledObjects[i].transform.position.x - i, 0, 0);
                pooledObjects[i].SetActive(true);
            }
        }
    }

    [System.Serializable]
    public class ObjectPoolItem
    {
        public int amountToPool;
        public GameObject objectToPool;
        public bool shouldExpand;
    }
}
