using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;


namespace LagLess
{

    public class LLConstants
    {

        public static float xpSelfPickupRadius = 1f;
        internal static BepInEx.Logging.ManualLogSource Logger;
    }

    public class LLConfigs
    {
        static public ConfigEntry<bool> enableJuice;

        static public void initConfig(ConfigFile config)
        {
            enableJuice = config.Bind("General", "Enable Juice", false, "For dev - spawns experience with k and specific laggy upgrades with j.");
        }
    }


    [BepInPlugin("acr.20mintilldawn.lagless", "Lag Minus Minus", "0.1.2")]
    public class LagLessPlugin : BaseUnityPlugin
    {

        private void Awake()
        {
            LLConstants.Logger = Logger;
            LLConfigs.initConfig(Config);

            Harmony.CreateAndPatchAll(typeof(ObjectPoolerPatch));
            Harmony.CreateAndPatchAll(typeof(CollisionLayersPatch));
            Harmony.CreateAndPatchAll(typeof(SummonLayersPatch));
            Harmony.CreateAndPatchAll(typeof(EnemyLayersPatch));
            Harmony.CreateAndPatchAll(typeof(XPPickupPatch));

            Harmony.CreateAndPatchAll(typeof(ModPowerUpFixPatch));
            Harmony.CreateAndPatchAll(typeof(ModStartFixPatch));

            if (LLConfigs.enableJuice.Value)
            {
                Harmony.CreateAndPatchAll(typeof(PlayerPatchJuice));
            }
        }

    }
}
