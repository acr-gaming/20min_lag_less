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

    [BepInPlugin("acr.20mintilldawn.lagless", "Lag Minus Minus", "0.0.1")]
    public class MapPlugin : BaseUnityPlugin
    {
        // State
        internal static BepInEx.Logging.ManualLogSource StaticLogger;
        internal static long numberGoBigger;


        private void Awake()
        {
            StaticLogger = Logger;

            Harmony.CreateAndPatchAll(typeof(PlayerPatch));
            Harmony.CreateAndPatchAll(typeof(PickupPatch));
        }

        [HarmonyPatch(typeof(PlayerController))]
        public class PlayerPatch
        {
            [HarmonyPatch("Update")]
            [HarmonyPrefix]
            static void Update(PlayerController __instance)
            {
                // StaticLogger.LogInfo("This is information");
            }



        }

        [HarmonyPatch(typeof(flanne.Pickups.Pickup))]
        public class PickupPatch
        {
            [HarmonyPatch("OnTriggerEnter2D")]
            [HarmonyPrefix]
            static void OnTriggerEnter2D()
            {
                StaticLogger.LogInfo($"Pickup Collider {numberGoBigger}");
                numberGoBigger++;
            }

        }


    }
}
