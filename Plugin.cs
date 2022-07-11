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

        public static float xpSelfPickupRadius = 1f;
        internal static BepInEx.Logging.ManualLogSource Logger;
    }

    public class LLConfigs
    {
        static public ConfigEntry<bool> enableLayerOptimization;
        static public ConfigEntry<bool> enableXPAggregation;
        static public ConfigEntry<bool> enableJuice;


        static public void initConfig(ConfigFile config)
        {
            enableLayerOptimization = config.Bind("General", "Enable Layer Optimization", true, "Moves bullets/xp into their own layers");
            enableXPAggregation = config.Bind("General", "Enable XP Aggregation", true, "Aggregates XP pickups.");
            enableJuice = config.Bind("General", "Enable Juice", false, "For dev - spawns experience with k and specific laggy upgrades with j.");
        }
    }


    [BepInPlugin("acr.20mintilldawn.lagless", "Lag Minus Minus", "0.0.3")]
    public class LagLessPlugin : BaseUnityPlugin
    {

        private void Awake()
        {
            LLConstants.Logger = Logger;
            LLConfigs.initConfig(Config);

            if (LLConfigs.enableLayerOptimization.Value)
            {
                Harmony.CreateAndPatchAll(typeof(ObjectPoolerPatch));
                Harmony.CreateAndPatchAll(typeof(PlayerPatchCollisionLayers));
            }

            if (LLConfigs.enableJuice.Value)
            {
                Harmony.CreateAndPatchAll(typeof(PlayerPatchJuice));
            }

            Harmony.CreateAndPatchAll(typeof(SummonLayersPatch));
            Harmony.CreateAndPatchAll(typeof(SelfXPPickupPatch));
            Harmony.CreateAndPatchAll(typeof(EnemyLayersPatch));
        }

    }
}
