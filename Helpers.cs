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

    public static class Identity<T>
    {
        public static Func<T, T> func = (x) => x;
    }

    public class Helpers
    {
        void disableAll(GameObject go)
        {

        }

        void enableAll(GameObject go)
        {
            go.SetActive(true);
            foreach (Transform child in go.transform)
                child.gameObject.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(flanne.Pickups.XPPickup))]
    public class PickupAmountPatch
    {
        [HarmonyPatch("SetActive")]
        [HarmonyPostfix]
        static void OnEnable(flanne.Pickups.XPPickup __instance)
        {
            LLConstants.Logger.LogInfo($"amnount {__instance.amount}");
        }

        [HarmonyPatch("UsePickup")]
        [HarmonyPostfix]
        static void UsePickup(flanne.Pickups.XPPickup __instance)
        {
            LLConstants.Logger.LogInfo($"usePickup amnount {__instance.amount}");
        }

    }

    [HarmonyPatch(typeof(flanne.Pickups.Pickup))]
    public class PickupCountPatch
    {
        static int numberGoBigger = 0;
        [HarmonyPatch("OnTriggerEnter2D")]
        [HarmonyPrefix]
        static void OnTriggerEnter2D()
        {
            LLConstants.Logger.LogInfo($"Pickup Collision Count: {numberGoBigger}");
            numberGoBigger++;
        }

    }
}