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
    public class TerrainSettings : IExposable
    {
        public int workAmount;
        public float failChance;
        public Dictionary<string, float> scavengingChance;
        public Dictionary<string, IntRange> spawnAmount;
        public void ExposeData()
        {
            Scribe_Values.Look(ref workAmount, "workAmount");
            Scribe_Values.Look(ref failChance, "failChance");
            Scribe_Collections.Look(ref scavengingChance, "scavengingChance", LookMode.Value, LookMode.Value, ref stringKeys1, ref floatValues1);
            Scribe_Collections.Look(ref spawnAmount, "spawnAmount", LookMode.Value, LookMode.Value, ref stringKeys2, ref intRangeValue);
        }

        private List<string> stringKeys1;
        private List<float> floatValues1;
        private List<string> stringKeys2;
        private List<IntRange> intRangeValue;
    }
    class ScavengeSettings : ModSettings
    {
        public List<string> disabledTerrains = new List<string>();
        public Dictionary<string, TerrainSettings> terrainSettings;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref disabledTerrains, "disabledTerrains", LookMode.Value);
            Scribe_Collections.Look(ref terrainSettings, "biomeSettings", LookMode.Value, LookMode.Deep, ref stringKeys, ref biomeSettingsValues);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (disabledTerrains is null)
                {
                    disabledTerrains = new List<string>();
                }
                if (terrainSettings is null)
                {
                    terrainSettings = new Dictionary<string, TerrainSettings>();
                }
            }
        }

        private List<string> stringKeys;
        private List<TerrainSettings> biomeSettingsValues;
        public void DoSettingsWindowContents(Rect inRect)
        {
            var terrainDefs = DefDatabase<TerrainDef>.AllDefs.Where(x => x.HasModExtension<TerrainExtension>()).ToList();
            var scrollHeight = GetScrollHeight(terrainDefs);
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect rect2 = new Rect(0f, 0f, rect.width - 60f, scrollHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect2);
            foreach (var terrain in terrainDefs)
            {
                listingStandard.GapLine();
                if (terrainSettings is null)
                {
                    terrainSettings = new Dictionary<string, TerrainSettings>();
                }
                if (!terrainSettings.ContainsKey(terrain.defName))
                {
                    terrainSettings[terrain.defName] = new TerrainSettings();
                }
                var biomeSettings = terrainSettings[terrain.defName];
                if (disabledTerrains is null)
                {
                    disabledTerrains = new List<string>();
                }
                bool disabled = disabledTerrains.Contains(terrain.defName);
                listingStandard.CheckboxLabeled("SC.Disable".Translate() + " " + terrain.label, ref disabled);
                if (!disabled && disabledTerrains.Contains(terrain.defName))
                {
                    disabledTerrains.Remove(terrain.defName);
                }
                else if (disabled && !disabledTerrains.Contains(terrain.defName))
                {
                    disabledTerrains.Add(terrain.defName);
                }
                if (!disabled)
                {
                    var extension = terrain.GetModExtension<TerrainExtension>();
                    biomeSettings.workAmount = extension.workAmount;
                    listingStandard.SliderLabeled("SC.BaseWorkAmount".Translate(), ref biomeSettings.workAmount, biomeSettings.workAmount.ToString(), 1, 10000);
                    biomeSettings.failChance = extension.failChance;
                    listingStandard.SliderLabeled("SC.ScavengingFailChance".Translate(), ref biomeSettings.failChance, (biomeSettings.failChance * 100).ToStringDecimalIfSmall() + "%", 0, 1);
                    foreach (var scavengable in extension.scavengables)
                    {
                        var thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(scavengable.thingDef);
                        if (thingDef != null)
                        {
                            if (biomeSettings.scavengingChance is null)
                            {
                                biomeSettings.scavengingChance = new Dictionary<string, float>();
                            }
                            var scavengeValue = biomeSettings.scavengingChance[thingDef.defName] = scavengable.chance;
                            listingStandard.SliderLabeled("SC.ScavengingChanceFor".Translate(thingDef.label), ref scavengeValue, (scavengeValue * 100).ToStringDecimalIfSmall() + "%", 0, 1);
                            biomeSettings.scavengingChance[thingDef.defName] = scavengeValue;

                            if (biomeSettings.spawnAmount is null)
                            {
                                biomeSettings.spawnAmount = new Dictionary<string, IntRange>();
                            }
                            var spawnAmount = biomeSettings.spawnAmount[thingDef.defName] = scavengable.amountToSpawn;
                            listingStandard.IntRange("SC.SpawnAmount".Translate(thingDef.label), ref spawnAmount, 1, 999);
                            biomeSettings.spawnAmount[thingDef.defName] = spawnAmount;
                        }
                    }
                }
            }
            listingStandard.End();
            Widgets.EndScrollView();
            SettingsApplier.ApplySettings();
            base.Write();
        }

        private float GetScrollHeight(List<TerrainDef> terrainDefs)
        {
            float num = 0;
            foreach (var terrain in terrainDefs)
            {
                var extension = terrain.GetModExtension<TerrainExtension>();
                if (extension != null)
                {
                    num += 24 + 12;
                    if (!disabledTerrains.Contains(terrain.defName))
                    {
                        num += 48;
                        foreach (var scavengable in extension.scavengables)
                        {
                            num += 24 + 24;
                        }
                    }

                }
            }
            return num + 10;
        }

        private static Vector2 scrollPosition = Vector2.zero;
    }
}
