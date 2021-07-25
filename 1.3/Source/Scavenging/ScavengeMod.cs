using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Scavenging
{
    class ScavengeMod : Mod
    {
        public static ScavengeSettings settings;
        public ScavengeMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<ScavengeSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            SettingsApplier.ApplySettings();
        }
        public override string SettingsCategory()
        {
            return "Scavenging Mod";
        }
    }

    [StaticConstructorOnStartup]
    static class SettingsApplier
    {
        static SettingsApplier()
        {
            ApplySettings();
        }

        public static void ApplySettings()
        {
            if (ScavengeMod.settings.terrainSettings != null)
            {
                foreach (var terrainSettings in ScavengeMod.settings.terrainSettings)
                {
                    var terrain = DefDatabase<TerrainDef>.GetNamedSilentFail(terrainSettings.Key);
                    if (terrain != null)
                    {
                        var extension = terrain.GetModExtension<TerrainExtension>();
                        if (extension != null)
                        {
                            extension.workAmount = terrainSettings.Value.workAmount;
                            extension.failChance = terrainSettings.Value.failChance;

                            if (terrainSettings.Value.scavengingChance != null)
                            {
                                foreach (var sc in terrainSettings.Value.scavengingChance)
                                {
                                    var entry = extension.scavengables.FirstOrDefault(x => x.thingDef == sc.Key);
                                    if (entry != null)
                                    {
                                        entry.chance = sc.Value;
                                    }
                                }
                            }

                            if (terrainSettings.Value.spawnAmount != null)
                            {
                                foreach (var sc in terrainSettings.Value.spawnAmount)
                                {
                                    var entry = extension.scavengables.FirstOrDefault(x => x.thingDef == sc.Key);
                                    if (entry != null)
                                    {
                                        entry.amountToSpawn = sc.Value;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
