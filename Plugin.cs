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

    public class LLConstants
    {
        public static int pickupLayer = 24;
        public static int pickerupLayer = 23;
        public static int bulletLayer = 25;
        internal static BepInEx.Logging.ManualLogSource StaticLogger;
        public static bool dev = true;
        public static bool enableOptimization = true;
    }


    [BepInPlugin("acr.20mintilldawn.lagless", "Lag Minus Minus", "0.0.2")]
    public class LagLessPlugin : BaseUnityPlugin
    {

        private void Awake()
        {
            LLConstants.StaticLogger = Logger;

            if (LLConstants.enableOptimization)
            {
                Harmony.CreateAndPatchAll(typeof(ObjectPoolerPatch));
                Harmony.CreateAndPatchAll(typeof(PlayerPatch));
            }

            if (LLConstants.dev)
            {
                Harmony.CreateAndPatchAll(typeof(PlayerPatchDev));
            }
        }

    }
}
