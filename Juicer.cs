using HarmonyLib;
using flanne;

using UnityEngine;
using System.Collections.Generic;

namespace LagLess
{

    class Juicer
    {
        public static void SpawnExperience(int num, Vector2 location, int radius)
        {
            for (int i = 0; i < num; i++)
            {
                Vector2 randomLocation = Random.insideUnitCircle * radius + location;
                var newXP = flanne.ObjectPooler.SharedInstance.GetPooledObject("SmallXP");
                newXP.transform.position = randomLocation;
                newXP.SetActive(value: true);
            }

        }

        public static void UpgradesPlease(PlayerController player)
        {
            flanne.Player.PlayerXP playerXP = player.GetComponentInChildren<flanne.Player.PlayerXP>();
            playerXP.level = 100;

            PowerupGenerator powerupGenerator = flanne.PowerupGenerator.Instance;

            Dictionary<string, int> wantedPowerUps = new Dictionary<string, int>()
            {
                {"GhostFriend", 5},
                {"InSync", 5},
                {"VengefulGhost", 5},
                {"EnergeticFriends", 5},

                {"MagicLens", 5},
                {"FocalPoint", 1},

                {"SummonMastery", 5},
            };

            List<PowerupPoolItem> PowerupPoolItems = powerupGenerator.powerupPool;

            foreach (var powerupPoolItem in PowerupPoolItems)
            {
                var powerup = powerupPoolItem.powerup;
                LLConstants.StaticLogger.LogDebug($"Powerup name:{powerup.name}");

                if (wantedPowerUps.ContainsKey(powerup.name))
                {
                    int numUpgrades = wantedPowerUps[powerup.name];
                    for (int i = 0; i < numUpgrades; i++)
                    {
                        powerup.ApplyAndNotify(player.gameObject);
                    }
                }
            }
        }
    }

}