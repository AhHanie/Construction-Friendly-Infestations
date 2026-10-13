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
                Log.Error("[CFInfestations] Missing harmony instance, infestations will not avoid buildings.");
                return;
            }

            // Patch CompSpawnerHives CanSpawnHiveAt
            Patch(
                AccessTools.Method(typeof(CompSpawnerHives), "CanSpawnHiveAt", new[] { typeof(IntVec3), typeof(Map), typeof(IntVec3), typeof(ThingDef), typeof(float), typeof(bool) }),
                AccessTools.Method(typeof(HarmonyPatcher), nameof(CanSpawnHiveAtPostfixPatch)),
                "CompSpawnerHives.CanSpawnHiveAt");

            // Patch InfestationCellFinder CellHasBlockingThings
            Patch(
                AccessTools.Method(typeof(InfestationCellFinder), "CellHasBlockingThings", new[] { typeof(IntVec3), typeof(Map) }),
                AccessTools.Method(typeof(HarmonyPatcher), nameof(CellHasBlockingThingsPostfixPatch)),
                "InfestationCellFinder.CellHasBlockingThings");
        }

        private static void Patch(MethodInfo original, MethodInfo postfix, string targetName)
        {
            if (original == null || postfix == null)
            {
                Log.Error("[CFInfestations] Could not patch " + targetName + ": " + (original == null ? "target method" : "postfix method") + " not found.");
                return;
            }

            instance.Patch(original, postfix: new HarmonyMethod(postfix));
            Logger.WriteToHarmonyFile("Patched " + targetName);
        }

        // Block children hives from spawning at buildings.
        // CanSpawnHiveAt returns true when the cell is allowed, so a building turns it into false.
        public static void CanSpawnHiveAtPostfixPatch(IntVec3 c, Map map, ref bool __result)
        {
            if (!__result)
            {
                return;
            }

            if (HasBuilding(c, map))
            {
                __result = false;
            }
        }

        // Block root hive from spawning at buildings.
        // CellHasBlockingThings returns true when the cell is rejected, so a building turns it into true.
        public static void CellHasBlockingThingsPostfixPatch(IntVec3 cell, Map map, ref bool __result)
        {
            if (__result)
            {
                return;
            }

            if (HasBuilding(cell, map))
            {
                __result = true;
            }
        }

        private static bool HasBuilding(IntVec3 cell, Map map)
        {
            List<Thing> thingList = cell.GetThingList(map);
            for (int k = 0; k < thingList.Count; k++)
            {
                if (thingList[k].def.category == ThingCategory.Building)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
