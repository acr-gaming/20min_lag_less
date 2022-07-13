using HarmonyLib;
using flanne;

using UnityEngine;
using System.Collections.Generic;


namespace LagLess
{

    // Fixes any mod that creates new pickerupers from a powerup
    [HarmonyPatch(typeof(Powerup))]
    public abstract class ModPowerUpFixPatch
    {
        [HarmonyPatch("ApplyAndNotify")]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPostfix]
        static void ApplyAndNotify(Powerup __instance)
        {
            LLLayers.SetAllPickerUppersLayers();
        }
    }

    [HarmonyPatch(typeof(PlayerController))]
    public abstract class ModStartFixPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPostfix]
        static void Start()
        {
            LLLayers.SetAllPickerUppersLayers();
        }
    }

}
