using BepInEx;
using HarmonyLib;
using UnityEngine;
using flanne;
using System.Collections.Generic;

namespace LagLess
{
    public class LLLayers
    {
        public static readonly int pickerupLayer = 23;
        public static readonly int pickupLayer = 24;
        public static readonly int bulletLayer = 25;
        public static readonly int bulletExplosionLayer = 26;
        public static readonly int summonCollideOnlyBullet = 27;
        public static readonly int enemyLayer = 28;


        public static void setPooledObjectLayer(GameObject objectToPool)
        {
            if (objectToPool.tag == "Pickup")
            {
                objectToPool.layer = LLLayers.pickupLayer;
            }
            else if (objectToPool.tag == "Bullet")
            {
                objectToPool.layer = LLLayers.bulletLayer;
            }
            else if (objectToPool.tag.StartsWith("Enemy"))
            {
                objectToPool.layer = LLLayers.enemyLayer;
            }
            else
            {
                // Explosions 
                HarmfulOnContact contactHarm = objectToPool.GetComponentInChildren<HarmfulOnContact>();
                if (contactHarm && contactHarm.hitTag == "Enemy")
                {
                    objectToPool.layer = LLLayers.bulletExplosionLayer;
                }
            }
        }
    }

    // TODO: Move somewhere else?
    [HarmonyPatch(typeof(PlayerController))]
    public class CollisionLayersPatch : MonoBehaviour
    {

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void Start(PlayerController __instance)
        {
            LLConstants.Logger.LogDebug("PlayerController Started");

            // There is some fog system that doesn't seem to get used but is still exists
            Camera cam = Camera.main;
            cam.cullingMask =
                cam.cullingMask
                    | (1 << LLLayers.enemyLayer)
                    | (1 << LLLayers.summonCollideOnlyBullet)
                    | (1 << LLLayers.bulletLayer)
                    | (1 << LLLayers.bulletExplosionLayer);

            GameObject PickerUpper = GameObject.FindGameObjectWithTag("Pickupper");
            PickerUpper.layer = LLLayers.pickerupLayer;

            setupLayers();
        }

        static void setupLayers()
        {
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

            // TODO: is this worth removing? not sure how much is left
            Physics2D.IgnoreLayerCollision(0, LLLayers.enemyLayer, false);
        }
    }


    [HarmonyPatch(typeof(AIComponent))]
    public class EnemyLayersPatch
    {
        // Needs to be done here because the trees don't get pooled
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
        // TOOD: dragon/ghost? not sure they even have colliders
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void Start(Summon __instance)
        {
            string summonType = __instance.SummonTypeID;
            LLConstants.Logger.LogDebug($"Summoning: {summonType}");

            switch (summonType)
            {
                case "MagicLens":
                    __instance.gameObject.layer = LLLayers.summonCollideOnlyBullet;
                    break;
                case "Knife":
                    __instance.gameObject.layer = LLLayers.bulletLayer;
                    break;
                case "Scythe":
                    __instance.gameObject.layer = LLLayers.bulletLayer;
                    break;
                default:
                    break;
            }
        }
    }

}