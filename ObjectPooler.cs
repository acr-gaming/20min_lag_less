using BepInEx;
using HarmonyLib;
using flanne;

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using BepInEx.Configuration;


namespace LagLess
{
    [HarmonyPatch(typeof(flanne.ObjectPooler))]
    public class ObjectPoolerPatch
    {
        static ObjectPoolerReplacement objectPoolReplacement;

        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        static bool Awake(flanne.ObjectPooler __instance)
        {
            flanne.ObjectPooler.SharedInstance = __instance;

            LLConstants.StaticLogger.LogDebug("Pooler Awake");
            objectPoolReplacement = new ObjectPoolerReplacement(__instance.transform, __instance.itemsToPool);
            return false;
        }


        [HarmonyPatch("GetPooledObject")]
        [HarmonyPrefix]
        static bool GetPooledObject(ref string tag, ref GameObject __result)
        {
            __result = objectPoolReplacement.GetPooledObject(tag);
            return false;
        }

        [HarmonyPatch("GetAllPooledObjects")]
        [HarmonyPrefix]
        static bool GetAllPooledObjects(ref string tag, ref List<GameObject> __result)
        {
            __result = objectPoolReplacement.GetAllPooledObjects(tag);
            return false;
        }

        [HarmonyPatch("AddObject")]
        [HarmonyPrefix]
        static bool AddObject(ref string tag, ref GameObject GO, ref int amt, ref bool exp)
        {
            objectPoolReplacement.AddObject(tag, GO, amt, exp);
            return false;
        }

    }

    public class LLObjectPool
    {
        List<GameObject> items;
        int currentIndex;
        ObjectPoolItem baseObject;
        Transform baseTransform;


        public LLObjectPool(ObjectPoolItem inBaseObject, Transform inBaseTransform)
        {
            baseTransform = inBaseTransform;
            baseObject = inBaseObject;
            items = createPoolItems(baseObject);
        }

        public List<GameObject> GetAll()
        {
            return items;
        }

        public GameObject GetNext()
        {
            for (int i = currentIndex; i < items.Count; i++)
            {
                if (!items[i].activeInHierarchy)
                {
                    currentIndex = i + 1;
                    return items[i];
                }
            }

            for (int i = 0; i < currentIndex; i++)
            {
                if (!items[i].activeInHierarchy)
                {
                    currentIndex = i + 1;
                    return items[i];
                }
            }

            if (baseObject.shouldExpand)
            {
                GameObject newItem = cloneBaseObject(baseObject);
                items.Add(newItem);
                currentIndex = 0;
                return newItem;
            }

            return null;
        }

        private List<GameObject> createPoolItems(ObjectPoolItem baseObject)
        {
            var toReturn = new List<GameObject>();

            for (int i = 0; i < baseObject.amountToPool; i++)
            {
                toReturn.Add(cloneBaseObject(baseObject));
            }

            return toReturn;
        }

        private GameObject cloneBaseObject(ObjectPoolItem baseObject)
        {
            GameObject toReturn = UnityEngine.Object.Instantiate(baseObject.objectToPool);
            toReturn.SetActive(value: false);
            toReturn.transform.SetParent(baseTransform);

            if (toReturn.tag == "Pickup")
            {
                toReturn.layer = LLConstants.pickupLayer;
            }
            else if (toReturn.tag == "Bullet")
            {
                toReturn.layer = LLConstants.bulletLayer;
            }

            return toReturn;
        }

    }

    public class ObjectPoolerReplacement
    {
        Dictionary<string, LLObjectPool> objectPools;
        Transform baseTransform;
        private Dictionary<string, int> lastPoolIndex;


        public ObjectPoolerReplacement(Transform inBaseTransform, List<ObjectPoolItem> itemsToPool)
        {
            baseTransform = inBaseTransform;
            objectPools = new Dictionary<string, LLObjectPool>();
            foreach (var item in itemsToPool)
            {
                LLConstants.StaticLogger.LogDebug($"ObjectPoolerReplacement:: Adding item to: {item.tag}");
                addNewPool(item);
            }
        }

        public void AddObject(string tag, GameObject GO, int amt = 3, bool exp = true)
        {
            if (!objectPools.ContainsKey(tag))
            {

                ObjectPoolItem item = new ObjectPoolItem(tag, GO, amt, exp);
                addNewPool(item);
            }
        }

        public List<GameObject> GetAllPooledObjects(string tag)
        {
            return objectPools[tag].GetAll();
        }

        public GameObject GetPooledObject(string tag)
        {
            return objectPools[tag].GetNext();
        }

        private void addNewPool(ObjectPoolItem item)
        {
            LLObjectPool newPool = new LLObjectPool(item, baseTransform);
            objectPools.Add(item.tag, newPool);
        }

    }
}