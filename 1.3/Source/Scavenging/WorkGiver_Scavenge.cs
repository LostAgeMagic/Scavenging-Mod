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
    public class WorkGiver_Scavenge : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.OnCell;
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(SC_DefOf.SC_ScavengingSpot);
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!pawn.CanReserveAndReach(t, PathEndMode, Danger.Deadly))
            {
                return false;
            }
            if (!ScavengeUtils.GetScavengableCells(t.Position, pawn.Map).Any())
            {
                JobFailReason.Is("SC.CannotScavengeNoUsableTiles".Translate());
            }
            return true;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(SC_DefOf.SC_Scavenging, t);
        }
    }
}