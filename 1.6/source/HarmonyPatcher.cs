using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;

namespace SK.CFInfestations
{
    public class HarmonyPatcher
    {
        public static Harmony instance;
        public static void PatchVanillaMethods()
        {
            if (instance == null)
            {
                Logger.WriteToHarmonyFile("Missing harmony instance");
                return;
            }

            // Patch CompSpawnerHives CanSpawnHiveAt
            MethodInfo canSpawnHiveAtMethod = AccessTools.Method(typeof(CompSpawnerHives), "CanSpawnHiveAt");
            HarmonyMethod canSpawnHiveAtPostfixPatch = new HarmonyMethod(typeof(HarmonyPatcher).GetMethod("CanSpawnHiveAtPostfixPatch"));
            instance.Patch(canSpawnHiveAtMethod, null, canSpawnHiveAtPostfixPatch);

            // Patch InfestationCellFinder CellHasBlockingThings
            MethodInfo cellHasBlockingThingsMethod = AccessTools.Method(typeof(InfestationCellFinder), "CellHasBlockingThings");
            HarmonyMethod cellHasBlockingThingsPostfixPatch = new HarmonyMethod(typeof(HarmonyPatcher).GetMethod("CellHasBlockingThingsPostfixPatch"));
            instance.Patch(cellHasBlockingThingsMethod, null, cellHasBlockingThingsPostfixPatch);
        }

        // Block children hives from spawning at buildings
        public static void CanSpawnHiveAtPostfixPatch(IntVec3 c, Map map, ref bool __result)
        {
            List<Thing> thingList = c.GetThingList(map);
            for (int k = 0; k < thingList.Count; k++)
            {
                Thing thing = thingList[k];
                if (thing.def.category == ThingCategory.Building)
                {
                    __result = false;
                    return;
                }
            }
        }

        // Block root hive from spawning at buildings
        public static void CellHasBlockingThingsPostfixPatch(IntVec3 cell, Map map, ref bool __result)
        {
            List<Thing> thingList = cell.GetThingList(map);
            for (int k = 0; k < thingList.Count; k++)
            {
                Thing thing = thingList[k];
                if (thing.def.category == ThingCategory.Building)
                {
                    __result = false;
                    return;
                }
            }
        }
    }
}
