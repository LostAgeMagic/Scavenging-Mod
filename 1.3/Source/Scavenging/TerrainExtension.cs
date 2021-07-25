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

        public override IEnumerable<string> ConfigErrors()
        {
			foreach (var scavengable in scavengables)
            {
				if (DefDatabase<ThingDef>.GetNamedSilentFail(scavengable.thingDef) is null)
                {
					yield return scavengable.thingDef + " is missing";
				}
				if (scavengable.cannotSpawnIn != null)
                {
					foreach (var biome in scavengable.cannotSpawnIn)
                    {
						if (DefDatabase<BiomeDef>.GetNamedSilentFail(biome) is null)
						{
							yield return biome + " is missing";
						}
					}
                }
            }
        }
    }
}
