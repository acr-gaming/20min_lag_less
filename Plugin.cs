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

    [BepInPlugin("acr.20mintilldawn.lagless", "Lag Minus Minus", "0.0.2")]
    public class MapPlugin : BaseUnityPlugin
    {
        // State
        internal static BepInEx.Logging.ManualLogSource StaticLogger;
        static int pickupLayer = 10;


        private void Awake()
        {
            StaticLogger = Logger;

            Harmony.CreateAndPatchAll(typeof(PlayerPatch));
            Harmony.CreateAndPatchAll(typeof(ObjectPooler));
        }

        [HarmonyPatch(typeof(PlayerController))]
        public class PlayerPatch
        {

            [HarmonyPatch("Start")]
            [HarmonyPostfix]
            static void Start()
            {
                Physics2D.IgnoreLayerCollision(0, pickupLayer, true);
                GameObject PickerUpper = GameObject.FindGameObjectWithTag("Pickupper");
                PickerUpper.layer = 10;
            }


        }

        [HarmonyPatch(typeof(flanne.ObjectPooler))]
        public class ObjectPooler
        {

            [HarmonyPatch("Awake")]
            [HarmonyPostfix]
            static void Awake()
            {
                StaticLogger.LogInfo("Pooler Awake");
            }

            [HarmonyPatch("GetPooledObject")]
            [HarmonyPostfix]
            static void GetPooledObject(ref string tag, ref GameObject __result)
            {

                if (__result.tag == "Pickup")
                {
                    StaticLogger.LogInfo($"Changing layer for: {tag}");
                    __result.layer = pickupLayer;
                }

            }

        }




    }
}
