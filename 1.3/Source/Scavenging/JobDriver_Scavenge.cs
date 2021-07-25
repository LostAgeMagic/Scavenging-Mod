using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Scavenging
{
    public class JobDriver_Scavenge : JobDriver
    {
        public Thing ScavengeSpot => this.TargetA.Thing;
        public IntVec3 Cell => this.TargetB.Cell;
        public const int InitialStandingPeriod = 360;
        private int curWorkTick;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job);
        }

        private int nextTurnTick;
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            Toil doWork = new Toil();
            doWork.tickAction = () =>
            {
                curWorkTick++;
                if (curWorkTick >= nextTurnTick)
                {
                    pawn.Rotation = Rot4.Random;
                    nextTurnTick = curWorkTick + Rand.RangeInclusive(60, 120);
                }
                if (curWorkTick >= InitialStandingPeriod)
                {
                    var cell = ScavengeUtils.GetScavengableCells(pawn.Position, this.Map).RandomElement();
                    this.job.SetTarget(TargetIndex.B, cell);
                    this.ReadyForNextToil();
                }
            };
            doWork.handlingFacing = true;
            doWork.defaultCompleteMode = ToilCompleteMode.Never;
            doWork.WithProgressBar(TargetIndex.A, () => curWorkTick / (float)InitialStandingPeriod, false, -0.5f);
            doWork.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return doWork;
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
            var doScavenging = new Toil();
            doScavenging.initAction = () => curWorkTick = 0;
            doScavenging.tickAction = () =>
            {
                curWorkTick++;
                if (curWorkTick >= GetScavengingPeriod(Cell))
                {
                    var extension = Cell.GetTerrain(this.Map).GetModExtension<TerrainExtension>();
                    if (!Rand.Chance(extension.failChance) && extension.ScavengablesFor(this.Map.Biome).TryRandomElementByWeight(x => x.chance, out var scavengableRecord))
                    {
                        SC_DefOf.Harvest_Standard_Finish.PlayOneShot(pawn);
                        var thingDef = DefDatabase<ThingDef>.GetNamed(scavengableRecord.thingDef);
                        Thing thing = ThingMaker.MakeThing(thingDef);
                        thing.stackCount = scavengableRecord.amountToSpawn.RandomInRange;
                        GenSpawn.Spawn(thing, pawn.Position, base.Map);
                    }
                    this.ReadyForNextToil();
                }
            };
            doScavenging.WithProgressBar(TargetIndex.B, () => curWorkTick / (float)GetScavengingPeriod(Cell), false, -0.5f);
            doScavenging.PlaySustainerOrSound(() => SC_DefOf.Harvest_Standard);

            doScavenging.defaultCompleteMode = ToilCompleteMode.Never;
            doScavenging.handlingFacing = true;
            yield return doScavenging;
        }

        private int GetScavengingPeriod(IntVec3 cell)
        {
            var extension = cell.GetTerrain(this.Map).GetModExtension<TerrainExtension>();
            var plantWorkbonus = pawn.GetStatValue(StatDefOf.PlantWorkSpeed);
            var huntingWorkBonus = pawn.GetStatValue(StatDefOf.HuntingStealth);
            var total = 1 + plantWorkbonus + huntingWorkBonus;
            return (int)(extension.workAmount / total);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref curWorkTick, "curWorkTick");
            Scribe_Values.Look(ref nextTurnTick, "nextTurnTick");
        }
    }
}