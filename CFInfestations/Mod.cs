using Verse;
using HarmonyLib;

namespace SK.CFInfestations
{
    public class Mod : Verse.Mod
    {
        public static bool SHOULD_PRINT_LOG = false;
        public Mod(ModContentPack content) : base(content)
        {
            Harmony instance = new Harmony("rimworld.sk.cfinfestations");
            HarmonyPatcher.instance = instance;

            LongEventHandler.ExecuteWhenFinished(Init);
        }

        public void Init()
        {
            HarmonyPatcher.PatchVanillaMethods();
        }
    }
}
