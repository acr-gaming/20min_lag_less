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
    public class LLXPComponent : MonoBehaviour
    {
        float defaultExperienceAmount;
        public int extraExperienceCollected;
        flanne.Pickups.XPPickup thisXPPickup;
        public bool hasBeenPickedUp = false;

        void Awake()
        {
            thisXPPickup = this.gameObject.GetComponent<flanne.Pickups.XPPickup>();
            defaultExperienceAmount = thisXPPickup.amount;
        }

        void OnEnable()
        {
            hasBeenPickedUp = false;
            extraExperienceCollected = 0;
            thisXPPickup.amount = defaultExperienceAmount;
            gameObject.transform.localScale = new Vector3(1, 1, 1);

            StartCoroutine(FindAndJoinAnotherXP());
        }

        void acceptMoreExperience(float amount, int extraExperienceCollected)
        {
            thisXPPickup.amount += amount;
            this.extraExperienceCollected += extraExperienceCollected + 1;
            float scaleFactor = Mathf.Min(1, thisXPPickup.amount * .05f) + Mathf.Min(1, thisXPPickup.amount * .02f) + Mathf.Min(1, thisXPPickup.amount * .005f);
            gameObject.transform.localScale = new Vector3(1f + scaleFactor, 1f + scaleFactor, 1f + scaleFactor);
        }

        IEnumerator FindAndJoinAnotherXP()
        {
            yield return new WaitForSeconds(1.5f);
            if (hasBeenPickedUp) yield break;

            LLXPComponent xpToJoin = findXPToJoin();
            if (xpToJoin)
            {
                hasBeenPickedUp = true;
                xpToJoin.acceptMoreExperience(thisXPPickup.amount, extraExperienceCollected);
                LeanTween.move(gameObject, xpToJoin.transform.position, 0.3f).setEase(LeanTweenType.easeInBounce).setOnComplete(JoinXPDone);
            }

        }

        void JoinXPDone()
        {
            gameObject.transform.SetParent(ObjectPooler.SharedInstance.transform);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.SetActive(false);
        }

        LLXPComponent findXPToJoin()
        {
            Vector3 currentPosition = gameObject.transform.position;
            float pickupRadius = LLConstants.xpSelfPickupRadius + LLConstants.xpSelfPickupRadius * UnityEngine.Random.value;
            Collider2D[] collisions = Physics2D.OverlapCircleAll(currentPosition, pickupRadius, (1 << LLLayers.pickupLayer));

            LLXPComponent clostestTarget = null;
            float clostestTargetDistance = Mathf.Infinity;

            foreach (Collider2D collision in collisions)
            {
                LLXPComponent otherLLXP = collision.gameObject.GetComponent<LLXPComponent>();
                if (
                    otherLLXP &&
                    otherLLXP != this &&
                    otherLLXP.hasBeenPickedUp == false
                )
                {
                    float distance = (otherLLXP.gameObject.transform.position - currentPosition).sqrMagnitude;
                    if (distance < clostestTargetDistance)
                    {
                        clostestTargetDistance = distance;
                        clostestTarget = otherLLXP;
                    }
                }
            }

            return clostestTarget;
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }
    }

    [HarmonyPatch(typeof(flanne.Pickups.Pickup))]
    public class SelfXPPickupPatch
    {
        [HarmonyPatch("OnTriggerEnter2D")]
        [HarmonyPrefix]
        static bool OnTriggerEnter2D(Collider2D other, flanne.Pickups.Pickup __instance)
        {

            if (other.tag == "Pickupper")
            {
                LLXPComponent llxp = __instance.gameObject.GetComponent<LLXPComponent>();
                if (llxp)
                {
                    if (llxp.hasBeenPickedUp) return false;
                    llxp.hasBeenPickedUp = true;
                }
            }
            else
            {
                LLConstants.Logger.LogError($"Pickup collided with something other than pickerupper with tag: {other.tag}");
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(flanne.Pickups.XPPickup))]
    public class XPPickupPatch
    {
        [HarmonyPatch("UsePickup")]
        [HarmonyPostfix]
        static void UsePickup(flanne.Pickups.XPPickup __instance)
        {
            LLXPComponent llxp = __instance.gameObject.GetComponent<LLXPComponent>();

            for (int i = 0; i < llxp.extraExperienceCollected; i++)
            {
                __instance.PostNotification(flanne.Pickups.XPPickup.XPPickupEvent, null);
            }
        }
    }
}