using HarmonyLib;
using flanne;
using System;

using UnityEngine;
using System.Collections.Generic;

namespace LagLess
{
    [HarmonyPatch(typeof(flanne.ObjectPooler))]
    public class ObjectPoolerPatch
    {
        static LLObjectPooler objectPoolReplacement;

        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        static bool Awake(flanne.ObjectPooler __instance)
        {
            LLConstants.Logger.LogDebug("Pooler Awake");
            objectPoolReplacement = new LLObjectPooler(__instance.transform, __instance.itemsToPool);
            flanne.ObjectPooler.SharedInstance = __instance;

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

    public class LLObjectPooler
    {
        Dictionary<string, LLObjectPool> objectPools;
        Transform baseTransform;

        public LLObjectPooler(Transform inBaseTransform, List<ObjectPoolItem> itemsToPool)
        {
            baseTransform = inBaseTransform;
            objectPools = new Dictionary<string, LLObjectPool>();
            foreach (var item in itemsToPool)
            {
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

        public List<GameObject> GetAllPooledObjects(string tag) => objectPools[tag].GetAll();

        public GameObject GetPooledObject(string tag) => objectPools[tag].GetNext();

        private void addNewPool(ObjectPoolItem item)
        {
            LLConstants.Logger.LogDebug($"OPR::addNewPool: {item.tag} | shouldExpand: {item.shouldExpand}");

            // Honestly not even sure these are used? Don't want them spawning outside of pooler though.
            if (item.tag == "LargeXP")
            {
                item.shouldExpand = true;
            }

            LLObjectPool newPool = new LLObjectPool(item, baseTransform);
            objectPools.Add(item.tag, newPool);
        }

    }


    public class LLObjectPool
    {
        List<GameObject> items;
        int currentIndex = 0;
        ObjectPoolItem baseObject;
        Transform baseTransform;

        public LLObjectPool(ObjectPoolItem inBaseObject, Transform inBaseTransform)
        {
            baseTransform = inBaseTransform;
            baseObject = inBaseObject;
            items = createPoolItems(baseObject);
        }

        public List<GameObject> GetAll() => items;

        // TODO: Think about having seperate active/inactive lists/arrays.
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

        private GameObject cloneBaseObject(ObjectPoolItem objectPoolItem)
        {
            GameObject toReturn = UnityEngine.Object.Instantiate(objectPoolItem.objectToPool);
            toReturn.SetActive(value: false);
            toReturn.transform.SetParent(baseTransform);

            if (toReturn.tag == "Pickup")
            {
                flanne.Pickups.XPPickup xpPickup = toReturn.GetComponent<flanne.Pickups.XPPickup>();
                if (xpPickup)
                {
                    toReturn.AddComponent(typeof(LLXPComponent));
                }
            }

            LLLayers.SetPooledObjectLayer(toReturn);

            return toReturn;
        }

    }
}
