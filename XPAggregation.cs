using BepInEx;
using HarmonyLib;
using flanne;

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using BepInEx.Configuration;
using System.Collections;

namespace LagLess
{


    [HarmonyPatch(typeof(PlayerController))]
    public class PlayerPatchAggregation : MonoBehaviour
    {
        static PlayerController player;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void Start(PlayerController __instance)
        {
            LLConstants.Logger.LogDebug("start");

            player = __instance;

            __instance.StartCoroutine(AggregateXP());
        }


        static IEnumerator AggregateXP()
        {
            while (true)
            {
                LLConstants.Logger.LogDebug("Running experience aggregation");
                if (player)
                {
                    Vector3 playerLocation = player.transform.position;
                    List<flanne.Pickups.XPPickup> xpPickups = getXPInRadius(playerLocation, 20);
                    LLConstants.Logger.LogDebug($"Found: {xpPickups.Count} xpPickups");

                }
                yield return new WaitForSeconds(2.0f);
            }
        }

        static List<flanne.Pickups.XPPickup> getXPInRadius(Vector3 center, int radius)
        {
            int xpLayer = LLConfigs.enableLayerOptimization.Value ? LLConstants.pickupLayer : 0;
            Collider2D[] collisions = Physics2D.OverlapCircleAll(center, radius, (1 << xpLayer));
            LLConstants.Logger.LogDebug($"Found: {collisions.Length} collisons");

            List<flanne.Pickups.XPPickup> toReturn = new List<flanne.Pickups.XPPickup>();

            foreach (Collider2D collision in collisions)
            {
                flanne.Pickups.XPPickup xppickup = collision.GetComponent<flanne.Pickups.XPPickup>();
                if (xppickup)
                {
                    toReturn.Add(xppickup);
                }
            }

            return toReturn;
        }


    }



}