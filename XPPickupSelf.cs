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

    // [HarmonyPatch(typeof(flanne.Pickups.XPPickup))]
    // public class SelfPickupPatch
    // {
    //     [HarmonyPatch("SetActive")]
    //     [HarmonyPostfix]
    //     static void OnEnable(flanne.Pickups.XPPickup __instance)
    //     {
    //         LLConstants.StaticLogger.LogInfo($"amnount {__instance.amount}");
    //     }

    //     [HarmonyPatch("UsePickup")]
    //     [HarmonyPostfix]
    //     static void UsePickup(flanne.Pickups.XPPickup __instance)
    //     {
    //         LLConstants.StaticLogger.LogInfo($"usePickup amnount {__instance.amount}");
    //     }

    // }

    public class LLXP : MonoBehaviour
    {
        public float lastTimeActivated = 0;
        public float originalXP;

        private void OnTriggerStay2D(Collider2D other)
        {
            LLConstants.Logger.LogDebug($"other xp collision: {other.tag}");

            GameObject otherGO = other.gameObject;
            LLXP otherLLXP = otherGO.GetComponentInChildren<LLXP>();
            flanne.Pickups.XPPickup otherXPPickup = otherGO.GetComponent<flanne.Pickups.XPPickup>();

            GameObject thisGO = this.transform.parent.gameObject;
            LLXP thisLLXP = thisGO.GetComponentInChildren<LLXP>();
            flanne.Pickups.XPPickup thisXPPickup = thisGO.GetComponent<flanne.Pickups.XPPickup>();

            // LLConstants.Logger.LogDebug($"other llxp: {otherLLXP}");
            // LLConstants.Logger.LogDebug($"other otherxppick coroutine is null: {otherXPPickup.pickUpCoroutine == null}");
            // LLConstants.Logger.LogDebug($"other active: {other.gameObject.activeInHierarchy}");
            // LLConstants.Logger.LogDebug($"thiss active: {this.gameObject.activeInHierarchy}");

            // LLConstants.Logger.LogDebug($"other active time: {otherLLXP.lastTimeActivated}");
            // LLConstants.Logger.LogDebug($"this active time: {lastTimeActivated}");

            if (otherXPPickup.pickUpCoroutine != null || thisXPPickup.pickUpCoroutine != null)
            {
                return;
            }

            // Unsure with multiple collisions if they will still fire?
            if (!otherGO.activeInHierarchy || !thisGO.activeInHierarchy)
            {
                return;
            }

            // I have no idea why but for some reason they aren't getting bidirectionally collided??
            if (otherLLXP.lastTimeActivated >= lastTimeActivated)
            {
                consumeOtherXP(otherGO);
            }
            else
            {
                otherLLXP.consumeOtherXP(thisGO);
            }

        }

        public void consumeOtherXP(GameObject toConsume)
        {
            LLConstants.Logger.LogDebug("consuming other xp");
            flanne.Pickups.XPPickup thisXPPickup = this.gameObject.GetComponentInParent<flanne.Pickups.XPPickup>();
            flanne.Pickups.XPPickup otherXPPickup = toConsume.GetComponent<flanne.Pickups.XPPickup>();

            thisXPPickup.amount += otherXPPickup.amount;

            toConsume.SetActive(false);
        }
    }

    public class LLXPUtils
    {
        static public GameObject CreateLLXPGameObject(flanne.Pickups.XPPickup xpPickup)
        {
            GameObject toReturn = new GameObject("LLXP", typeof(LLXP), typeof(CircleCollider2D), typeof(Rigidbody2D));
            CircleCollider2D collider = toReturn.GetComponent<CircleCollider2D>();
            LLXP llxp = toReturn.GetComponent<LLXP>();

            llxp.originalXP = xpPickup.amount;
            toReturn.layer = LLConstants.pickerupLayer;
            collider.radius = LLConstants.xpSelfPickupRadius;

            CircleCollider2D xpCollider = xpPickup.gameObject.GetComponent<CircleCollider2D>();

            Physics2D.IgnoreCollision(collider, xpCollider, true);

            return toReturn;
        }

        public static GameObject resetXPPickup(GameObject go)
        {

            return go;
        }

    }

    public class EnableDisableXP : MonoBehaviour
    {
        void OnEnable()
        {
            GameObject go = this.gameObject;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }

            LLXP llxp = go.GetComponentInChildren<LLXP>();
            go.GetComponent<CircleCollider2D>().enabled = true;
            go.transform.Find("LLXP").gameObject.GetComponent<CircleCollider2D>().enabled = true;
            flanne.Pickups.XPPickup xpPickup = go.GetComponent<flanne.Pickups.XPPickup>();
            xpPickup.amount = llxp.originalXP;
            llxp.lastTimeActivated = Time.time;
        }
        void OnDisable()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    public class EnableDisableXP2 : MonoBehaviour
    {
        float originalXP;
        flanne.Pickups.XPPickup thisXPPickup;
        static long globalAge = long.MaxValue;
        public long age;
        bool alreadyMerging;


        void Awake()
        {
            thisXPPickup = gameObject.GetComponent<flanne.Pickups.XPPickup>();
        }
        void OnEnable()
        {

            originalXP = thisXPPickup.amount;
            age = globalAge--;
            gameObject.GetComponent<CircleCollider2D>().enabled = true;
            alreadyMerging = false;

            Invoke("FindAndJoinAnotherXP", .2f);
        }

        void FindAndJoinAnotherXP()
        {

            EnableDisableXP2 xpToJoin = findXPToJoin();
            if (xpToJoin)
            {
                alreadyMerging = true;
                JoinXP(xpToJoin);
            }
        }

        void JoinXP(EnableDisableXP2 target)
        {
            gameObject.GetComponent<CircleCollider2D>().enabled = false;
            flanne.Pickups.XPPickup targetXPPickup = target.gameObject.GetComponent<flanne.Pickups.XPPickup>();
            targetXPPickup.amount += thisXPPickup.amount;
            LeanTween.move(gameObject, targetXPPickup.transform, 0.3f).setEase(LeanTweenType.easeInBack).setOnComplete(JoinXPDone);
        }

        void JoinXPDone()
        {
            gameObject.SetActive(false);
        }

        EnableDisableXP2 findXPToJoin()
        {
            Vector2 currentPosition = gameObject.transform.position;
            Collider2D[] collisions = Physics2D.OverlapCircleAll(currentPosition, LLConstants.xpSelfPickupRadius, (1 << LLConstants.pickupLayer));
            LLConstants.Logger.LogDebug($"numcollisions:{collisions.Length}");

            EnableDisableXP2 oldestInRange = this;
            foreach (Collider2D collision in collisions)
            {
                EnableDisableXP2 otherEnableDisable = collision.gameObject.GetComponent<EnableDisableXP2>();
                if (otherEnableDisable != null && otherEnableDisable.alreadyMerging == false)
                {
                    if (otherEnableDisable.age > oldestInRange.age)
                    {
                        oldestInRange = otherEnableDisable;
                    }
                }
            }

            if (oldestInRange == this)
            {
                return null;
            }
            else
            {
                return oldestInRange;
            }
        }
        void OnDisable()
        {
            thisXPPickup.amount = originalXP;
        }
    }

    [HarmonyPatch(typeof(flanne.Pickups.Pickup))]
    public class SelfXPPickupPatch
    {

        [HarmonyPatch("OnTriggerEnter2D")]
        [HarmonyPrefix]
        static bool OnTriggerEnter2D(Collider2D other, flanne.Pickups.Pickup __instance)
        {

            // disable colliders when being picked up does this actually guarentee no more collisions??
            if (other.tag == "Pickupper")
            {
                LLConstants.Logger.LogDebug($"disabling collider");
                __instance.gameObject.GetComponent<CircleCollider2D>().enabled = false;
            }

            // dunno if needed?
            if (!__instance.gameObject.activeInHierarchy) return false;

            return true;
        }

    }
}