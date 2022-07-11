using BepInEx;
using HarmonyLib;
using UnityEngine;
using flanne;
using System.Collections.Generic;

namespace LagLess
{
    public class LLLayers
    {
        public static int pickerupLayer = 23;
        public static int pickupLayer = 24;
        public static int bulletLayer = 25;
        public static int summonCollideOnlyBullet = 26;
        public static int enemyLayer = 27;
        public static int bulletExplosionLayer = 28;

        public static readonly Dictionary<string, int> ghostPowerUps = new Dictionary<string, int>
            {
                {"Pickup", pickupLayer},
                {"Bullet", bulletLayer},
            };

    }

    [HarmonyPatch(typeof(PlayerController))]
    public class PlayerPatchCollisionLayers : MonoBehaviour
    {

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void Start(PlayerController __instance)
        {
            LLConstants.Logger.LogDebug("PlayerController Started");

            // There is some weird fog system that doesnt' really seem to get used but is still around
            Camera cam = Camera.main;
            cam.cullingMask = cam.cullingMask | (1 << LLLayers.enemyLayer) | (1 << LLLayers.summonCollideOnlyBullet) | (1 << LLLayers.bulletLayer) | (1 << LLLayers.bulletExplosionLayer);

            // Clear collisions
            for (int i = 0; i < 32; i++)
            {
                Physics2D.IgnoreLayerCollision(i, LLLayers.pickupLayer, true);
                Physics2D.IgnoreLayerCollision(i, LLLayers.pickerupLayer, true);
                Physics2D.IgnoreLayerCollision(i, LLLayers.summonCollideOnlyBullet, true);
                Physics2D.IgnoreLayerCollision(i, LLLayers.bulletLayer, true);
                Physics2D.IgnoreLayerCollision(i, LLLayers.bulletExplosionLayer, true);
                Physics2D.IgnoreLayerCollision(i, LLLayers.enemyLayer, true);
            }

            // Pickups collide with pickeruppers
            Physics2D.IgnoreLayerCollision(LLLayers.pickupLayer, LLLayers.pickerupLayer, false);

            // For lens so they collide with bullets only
            Physics2D.IgnoreLayerCollision(LLLayers.summonCollideOnlyBullet, LLLayers.bulletLayer, false);

            // Bullets must collide with enemies
            Physics2D.IgnoreLayerCollision(LLLayers.bulletLayer, LLLayers.enemyLayer, false);

            // Explosions need to collider with player and enemies  
            Physics2D.IgnoreLayerCollision(LLLayers.bulletExplosionLayer, LLLayers.enemyLayer, false);
            Physics2D.IgnoreLayerCollision(LLLayers.bulletExplosionLayer, 0, false);

            // I think enemys need to collide?
            Physics2D.IgnoreLayerCollision(LLLayers.enemyLayer, LLLayers.enemyLayer, false);

            // TODO: remove this - but I think too much stuff for now - maybe enough other stuff moved out it doesn't matter
            Physics2D.IgnoreLayerCollision(0, LLLayers.enemyLayer, false);

            GameObject PickerUpper = GameObject.FindGameObjectWithTag("Pickupper");
            PickerUpper.layer = LLLayers.pickerupLayer;
        }
    }


    [HarmonyPatch(typeof(AIComponent))]
    public class EnemyLayersPatch
    {

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void Start(Summon __instance)
        {
            if (__instance.gameObject.tag.StartsWith("Enemy"))
            {
                __instance.gameObject.layer = LLLayers.enemyLayer;
            }
        }
    }

    [HarmonyPatch(typeof(Summon))]
    public class SummonLayersPatch
    {

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void Start(Summon __instance)
        {
            LLConstants.Logger.LogDebug($"Summoning: {__instance.SummonTypeID}");
            if (__instance.SummonTypeID == "MagicLens")
            {
                __instance.gameObject.layer = LLLayers.summonCollideOnlyBullet;
            }
        }
    }

}