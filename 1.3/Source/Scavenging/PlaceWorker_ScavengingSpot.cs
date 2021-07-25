using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Scavenging
{
    public class PlaceWorker_ScavengingSpot : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            var scavengingSpots = map.listerThings.ThingsOfDef(SC_DefOf.SC_ScavengingSpot);
            if (scavengingSpots.Any(x => x.Position.DistanceTo(loc) <= 40))
            {
                return new AcceptanceReport("SC.CannotPlaceSpotsNearToEachOther".Translate());
            }
            return base.AllowsPlacing(checkingDef, loc, rot, map, thingToIgnore, thing);
        }
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            Map currentMap = Find.CurrentMap;
            var cells = ScavengeUtils.GetScavengableCells(center, currentMap);
            if (cells.Any())
            {
                GenDraw.DrawFieldEdges(cells.ToList(), Color.green);
            }
        }

    }
}
