using BepInEx;
using HarmonyLib;
using UnityEngine;
using flanne;


namespace LagLess
{
    [HarmonyPatch(typeof(PlayerController))]
    public class PlayerPatchJuice
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
                Juicer.UpgradesPlease(__instance, Juicer.littleBitOfEverythingElse);
                Juicer.SetPlayerLevel(__instance);
            }

        }
    }

}