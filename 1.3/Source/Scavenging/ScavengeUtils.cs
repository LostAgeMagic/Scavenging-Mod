using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;
using Verse.AI;

namespace Scavenging
{
    [StaticConstructorOnStartup]
	public static class ScavengeUtils
    {
        static ScavengeUtils()
        {
            foreach (var terrain in DefDatabase<TerrainDef>.AllDefs)
            {
                if (terrain.defName.EndsWith("_Rough"))
                {
                    if (terrain.modExtensions is null)
                    {
                        terrain.modExtensions = new List<DefModExtension>();
                    }
                    terrain.modExtensions.Add(new TerrainExtension
                    {
                        workAmount = 300,
                        failChance = 0.4f,
                        scavengables = new List<ScavengableRecord>
                        {
                            new ScavengableRecord { thingDef = "SC_Snail", chance = 0.7f, amountToSpawn = new IntRange {min=1, max=1 } },
                            new ScavengableRecord { thingDef = "SC_Frog", chance = 0.15f, amountToSpawn = new IntRange {min=1, max=1 } },
                            new ScavengableRecord { thingDef = "SC_Beetle", chance = 0.05f, amountToSpawn = new IntRange {min=1, max=1 } },
                            new ScavengableRecord { thingDef = "SC_Moss", chance = 0.6f, amountToSpawn = new IntRange {min=1, max=1 } },
                        }
                    });
                }
            }
        }

        public static List<IntVec3> GetScavengableCells(IntVec3 root, Map map)
        {
            return GenRadial.RadialCellsAround(root, 20, true).Where(x => x != root && IsGoodCell(x, map) 
            && map.reachability.CanReach(root, x, PathEndMode.OnCell, TraverseMode.PassDoors)).ToList();
        }
        private static bool IsGoodCell(IntVec3 cell, Map map)
        {
            return cell.InBounds(map) && cell.GetEdifice(map) is null && IsNaturalAndHasFood(cell, map);
        }
        private static bool IsNaturalAndHasFood(IntVec3 cell, Map map)
        {
            var terrain = cell.GetTerrain(map);
            if ((ScavengeMod.settings.disabledTerrains != null && !ScavengeMod.settings.disabledTerrains.Contains(terrain.defName) || ScavengeMod.settings.disabledTerrains is null)
                && !terrain.Removable)
            {
                return terrain.GetModExtension<TerrainExtension>() != null;
            }
            return false;
        }
    }
}
