using BepInEx;
using HarmonyLib;
using UnityEngine;
using flanne;


namespace LagLess
{

    [HarmonyPatch(typeof(PlayerController))]
    public class PlayerPatch
    {

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void Start()
        {
            LLConstants.StaticLogger.LogDebug("PlayerController Started");

            // Pickups only collide with pickeruper
            Physics2D.IgnoreLayerCollision(0, LLConstants.pickupLayer, true);
            Physics2D.IgnoreLayerCollision(0, LLConstants.pickerupLayer, true);
            Physics2D.IgnoreLayerCollision(LLConstants.pickupLayer, LLConstants.pickupLayer, true);

            // Bullets don't collide with themselves or pickups
            Physics2D.IgnoreLayerCollision(LLConstants.bulletLayer, LLConstants.bulletLayer, true);
            Physics2D.IgnoreLayerCollision(LLConstants.bulletLayer, LLConstants.pickupLayer, true);
            Physics2D.IgnoreLayerCollision(LLConstants.bulletLayer, LLConstants.pickerupLayer, true);

            GameObject PickerUpper = GameObject.FindGameObjectWithTag("Pickupper");
            PickerUpper.layer = LLConstants.pickerupLayer;

        }
    }

    [HarmonyPatch(typeof(PlayerController))]
    public class PlayerPatchDev
    {
        static bool needsUpgrades = true;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void Start()
        {
            needsUpgrades = true;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void Update(PlayerController __instance)
        {

            if (UnityEngine.InputSystem.Keyboard.current.kKey.isPressed)
            {
                Juicer.SpawnExperience(2, __instance.transform.position, 15);
            }

            if (needsUpgrades && UnityEngine.InputSystem.Keyboard.current.jKey.isPressed)
            {
                needsUpgrades = false;
                Juicer.UpgradesPlease(__instance);
            }

        }
    }

}