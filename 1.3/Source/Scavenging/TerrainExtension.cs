using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace Scavenging
{
	public class ScavengableRecord
	{
		public string thingDef;

		public float chance;

		public IntRange amountToSpawn;

		public List<string> cannotSpawnIn;
	}
	public class TerrainExtension : DefModExtension
    {
		public List<ScavengableRecord> scavengables;
		public List<ScavengableRecord> ScavengablesFor(BiomeDef biomeDef)
        {
			return scavengables.Where(x => DefDatabase<ThingDef>.GetNamedSilentFail(x.thingDef) != null && (x.cannotSpawnIn is null || !x.cannotSpawnIn.Contains(biomeDef.defName))).ToList();
		}
		public int workAmount;
		public float failChance;
    }
}
