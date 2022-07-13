using HarmonyLib;
using flanne;

using UnityEngine;
using System.Collections.Generic;

namespace LagLess
{
    class Juicer
    {
        public static readonly Dictionary<string, int> ghostPowerUps = new Dictionary<string, int>
            {
                {"GhostFriend", 6},
                {"InSync", 5},
                {"VengefulGhost", 6},
                {"EnergeticFriends", 5},

                {"MagicLens", 5},
                {"FocalPoint", 1},

                {"SummonMastery", 10},

                {"Frostbite", 6},
                {"FrostMagic", 2},
                {"Shatter", 2},

                {"Vitality", 5},
                {"Regeneration", 5},
            };

        public static readonly Dictionary<string, int> summonTest = new Dictionary<string, int>
            {
                {"DragonEgg", 1},

                {"InSync", 1},
                {"GhostFriend", 1},

                {"MagicLens", 2},

                {"LightWeaponry", 1},
                {"HeavyWeaponry", 1},
                {"DualWield", 1},

                {"SummonMastery", 2},
                {"buddy_name", 2},
                {"buddy_two_name", 1}
            };

        public static readonly Dictionary<string, int> littleBitOfEverythingElse = new Dictionary<string, int>
            {
                {"BigShot", 1},
                {"Splinter", 1},
                {"GhostFriend", 1},
                {"RubberBullets", 1},
                {"HolyShield", 1},
                {"StalwartShield", 1},
                {"ElectroMage", 1},
                {"ElectroBug", 1},
                {"ElectroMastery", 1},
                {"Energized", 1},
                {"IntenseBurn", 1},
                {"PyroMage", 1},
                {"Shatter", 1},
                {"FrostMagic", 1},
                {"MagicLens", 1},
                {"FrostFire", 1},
                {"Overload", 1},
            };


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

        public static void SetPlayerLevel(PlayerController player, int level = 100)
        {
            flanne.Player.PlayerXP playerXP = player.GetComponentInChildren<flanne.Player.PlayerXP>();
            playerXP.level = level;
        }

        public static void UpgradesPlease(PlayerController player, Dictionary<string, int> wantedPowerUps)
        {
            PowerupGenerator powerupGenerator = flanne.PowerupGenerator.Instance;
            List<PowerupPoolItem> PowerupPoolItems = powerupGenerator.powerupPool;

            foreach (var powerupPoolItem in PowerupPoolItems)
            {
                var powerup = powerupPoolItem.powerup;

                LLConstants.Logger.LogDebug($"{powerup.name}");
                LLConstants.Logger.LogDebug($"nameStringID: {powerup.nameStringID.key}");


                if (wantedPowerUps.ContainsKey(powerup.name) || wantedPowerUps.ContainsKey(powerup.nameStringID.key))
                {
                    string powerUpKey = powerup.name.Length > 0 ? powerup.name : powerup.nameStringID.key;
                    int numUpgrades = wantedPowerUps[powerUpKey];
                    LLConstants.Logger.LogDebug($"Applying: {powerUpKey} - {numUpgrades} times.");
                    for (int i = 0; i < numUpgrades; i++)
                    {
                        powerup.ApplyAndNotify(player.gameObject);
                    }
                }
            }
        }

    }
}
