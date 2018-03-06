using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{

    public static ObjectPooler SharedInstance;
    public List<ObjectPoolItem> itemsToPool;
    public List<GameObject> pooledObjects;
    [HideInInspector] public string[] bloodDecapitationEffects;

    void Awake()
    {
        SharedInstance = this;
    }

    void Start()
    {
        bloodDecapitationEffects = new string[] { "BloodEffect4", "BloodEffect5" };
        pooledObjects = new List<GameObject>();
        foreach (ObjectPoolItem item in itemsToPool)
        {
            for (int i = 0; i < item.amountToPool; i++)
            {
                GameObject obj = (GameObject)Instantiate(item.objectToPool);
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
                if(name != null && pooledObjects[i].name.Replace("(Clone)", "") == name)
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
