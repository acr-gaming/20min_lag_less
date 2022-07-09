using BepInEx;
using HarmonyLib;
using flanne;

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using BepInEx.Configuration;


namespace LagLess
{
    [HarmonyPatch(typeof(flanne.ObjectPooler))]
    public class ObjectPooler
    {

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        static void Awake()
        {
            LLConstants.StaticLogger.LogDebug("Pooler Awake");
        }

        [HarmonyPatch("GetPooledObject")]
        [HarmonyPostfix]
        static void GetPooledObject(ref string tag, ref GameObject __result)
        {

            if (__result.tag == "Pickup")
            {
                LLConstants.StaticLogger.LogInfo($"Changing layer for: {tag}");
                __result.layer = LLConstants.pickupLayer;
            }

            if (__result.tag == "Bullet")
            {
                LLConstants.StaticLogger.LogInfo($"Changing layer for: {tag}");
                __result.layer = LLConstants.bulletLayer;
            }

        }

    }
}