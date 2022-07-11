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
    public class LLXP : MonoBehaviour
    {
        float originalXP;
        int numExperience;
        flanne.Pickups.XPPickup thisXPPickup;
        static long globalAge = long.MaxValue;
        public long age;
        public bool hasBeenPickedUp;

        void OnEnable()
        {
            thisXPPickup = this.gameObject.GetComponent<flanne.Pickups.XPPickup>();

            originalXP = thisXPPickup.amount;
            age = globalAge--;
            hasBeenPickedUp = false;
            numExperience = 1;

            Invoke("FindAndJoinAnotherXP", 1.5f);
        }

        void acceptMoreExperience(float amount)
        {
            thisXPPickup.amount += amount;
            numExperience += 1;
            float scaleFactor = Mathf.Min(1, thisXPPickup.amount * .05f) + Mathf.Min(1, thisXPPickup.amount * .01f) + Mathf.Min(1, thisXPPickup.amount * .001f);
            gameObject.transform.localScale = new Vector3(1f + scaleFactor, 1f + scaleFactor, 1f + scaleFactor);
        }

        void FindAndJoinAnotherXP()
        {
            if (hasBeenPickedUp == false)
            {
                LLXP xpToJoin = findXPToJoin();
                if (xpToJoin)
                {
                    hasBeenPickedUp = true;
                    JoinXP(xpToJoin);
                }
            }
        }

        void JoinXP(LLXP target)
        {
            target.acceptMoreExperience(thisXPPickup.amount);
            thisXPPickup.amount = 0;
            LeanTween.move(base.gameObject, target.gameObject.transform.position, 0.3f).setEase(LeanTweenType.easeInBounce).setOnComplete(JoinXPDone);
        }

        void JoinXPDone()
        {
            thisXPPickup.transform.SetParent(ObjectPooler.SharedInstance.transform);
            thisXPPickup.transform.localPosition = Vector3.zero;
            gameObject.SetActive(false);
        }

        LLXP findXPToJoin()
        {
            Vector3 currentPosition = gameObject.transform.position;
            Collider2D[] collisions = Physics2D.OverlapCircleAll(currentPosition, LLConstants.xpSelfPickupRadius, (1 << LLConstants.pickupLayer));

            LLXP clostestTarget = null;
            float clostestTargetDistance = Mathf.Infinity;

            foreach (Collider2D collision in collisions)
            {
                LLXP otherEnableDisable = collision.gameObject.GetComponent<LLXP>();
                if (
                    otherEnableDisable != null &&
                    otherEnableDisable.gameObject.activeInHierarchy &&
                    otherEnableDisable.hasBeenPickedUp == false &&
                    otherEnableDisable.age > this.age
                )
                {
                    float distance = (collision.gameObject.transform.position - currentPosition).sqrMagnitude;
                    if (distance < clostestTargetDistance)
                    {
                        clostestTargetDistance = distance;
                        clostestTarget = otherEnableDisable;
                    }
                }
            }

            return clostestTarget;
        }
        void OnDisable()
        {
            thisXPPickup.amount = originalXP;
            gameObject.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    [HarmonyPatch(typeof(flanne.Pickups.Pickup))]
    public class SelfXPPickupPatch
    {

        [HarmonyPatch("OnTriggerEnter2D")]
        [HarmonyPrefix]
        static bool OnTriggerEnter2D(Collider2D other, flanne.Pickups.Pickup __instance)
        {
            LLConstants.Logger.LogDebug($"PickupCollision: {other.tag}");

            if (other.tag == "Pickupper")
            {
                LLXP llxp = __instance.gameObject.GetComponent<LLXP>();
                if (llxp != null)
                {
                    if (llxp.hasBeenPickedUp) return false;
                    llxp.hasBeenPickedUp = true;
                }
            }

            return true;
        }

    }
}