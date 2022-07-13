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
        public flanne.Pickups.XPPickup flanneXP;

        void Awake()
        {
            flanneXP = this.gameObject.GetComponent<flanne.Pickups.XPPickup>();
            defaultExperienceAmount = flanneXP.amount;
        }

        void OnEnable()
        {
            extraExperienceCollected = 0;
            flanneXP.amount = defaultExperienceAmount;
            gameObject.transform.localScale = new Vector3(1, 1, 1);

            StartCoroutine(FindAndJoinAnotherXP());
        }

        void acceptMoreExperience(float amount, int extraExperienceCollected)
        {
            flanneXP.amount += amount;
            this.extraExperienceCollected += extraExperienceCollected + 1;
            float scaleFactor = Mathf.Min(1, flanneXP.amount * .05f) + Mathf.Min(1, flanneXP.amount * .02f) + Mathf.Min(1, flanneXP.amount * .005f);
            gameObject.transform.localScale = new Vector3(1f + scaleFactor, 1f + scaleFactor, 1f + scaleFactor);
        }

        IEnumerator FindAndJoinAnotherXP()
        {
            yield return new WaitForSeconds(1.5f);
            if (flanneXP.pickUpCoroutine != null) yield break;

            LLXPComponent xpToJoin = findXPToJoin();
            if (xpToJoin)
            {
                flanneXP.pickUpCoroutine = JoinXP(xpToJoin);
                StartCoroutine(flanneXP.pickUpCoroutine);
            }
        }

        IEnumerator JoinXP(LLXPComponent xpToJoin)
        {
            xpToJoin.acceptMoreExperience(flanneXP.amount, extraExperienceCollected);
            int tweenID = LeanTween.move(gameObject, xpToJoin.transform.position, 0.3f).setEase(LeanTweenType.easeInBounce).id;

            while (LeanTween.isTweening(tweenID))
            {
                yield return null;
            }

            flanneXP.pickUpCoroutine = null;
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
                    otherLLXP.flanneXP.pickUpCoroutine == null
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
