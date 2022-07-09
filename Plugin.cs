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


        private void Awake()
        {
            StaticLogger = Logger;

            Harmony.CreateAndPatchAll(typeof(PlayerPatch));
            Harmony.CreateAndPatchAll(typeof(PickupPatch));
        }

        [HarmonyPatch(typeof(PlayerController))]
        public class PlayerPatch
        {
            static int experienceLayer = 10;
            static int count = 0;

            [HarmonyPatch("Start")]
            [HarmonyPostfix]
            static void Start()
            {
                Physics2D.IgnoreLayerCollision(0, experienceLayer, true);
                GameObject PickerUpper = GameObject.FindGameObjectWithTag("Pickupper");
                PickerUpper.layer = 10;
            }


            // [HarmonyPatch("Update")]
            // [HarmonyPrefix]
            // static void Update(PlayerController __instance)
            // {
            //     int playerLevel = __instance.GetComponentInChildren<flanne.Player.PlayerXP>().level;


            //     if (playerLevel >= 60)
            //     {


            //         if (count >= howOften)
            //         {
            //             Vector3 playerLocation = __instance.transform.position;
            //             Collider2D[] collisions = Physics2D.OverlapCircleAll(playerLocation, 100);

            //             foreach (Collider2D collision in collisions)
            //             {
            //                 if (collision.tag == "Pickup")
            //                 {
            //                     StaticLogger.LogInfo("pickup found");

            //                     flanne.Pickups.XPPickup xppickup = collision.GetComponent<flanne.Pickups.XPPickup>();
            //                     if (xppickup)
            //                     {
            //                         Destroy(xppickup.transform.gameObject);
            //                     }
            //                 }
            //             }
            //             count = 0;
            //         }
            //         else
            //         {
            //             count++;
            //         }
            //    }
            //}



        }

        // [HarmonyPatch(typeof(flanne.Pickups.XPPickup))]
        // public class PickupPatch
        // {
        //     [HarmonyPatch("SetActive")]
        //     [HarmonyPostfix]
        //     static void OnEnable(flanne.Pickups.XPPickup __instance)
        //     {
        //         StaticLogger.LogInfo($"amnount {__instance.amount}");
        //     }

        //     [HarmonyPatch("UsePickup")]
        //     [HarmonyPostfix]
        //     static void UsePickup(flanne.Pickups.XPPickup __instance)
        //     {
        //         StaticLogger.LogInfo($"usePickup amnount {__instance.amount}");
        //     }

        // }

        [HarmonyPatch(typeof(flanne.PowerupSystem.SummonOnEnemyDeath))]
        public class PickupPatch
        {
            [HarmonyPatch("OnDeath")]
            [HarmonyPostfix]
            static void OnDeath(flanne.PowerupSystem.SummonOnEnemyDeath __instance)
            {
                StaticLogger.LogInfo($"sdsadsa");
            }


        }


    }
}
