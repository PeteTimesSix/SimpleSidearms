using RimWorld;
using SimpleSidearms.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SimpleSidearms.rimworld
{
    [StaticConstructorOnStartup]
    public class GoldfishModule : IExposable
    {
        internal List<string> weapons = new List<string>();
        internal string primary = NoWeaponString;

        public static string NoWeaponString = "NOWEAPON";

        public Pawn Owner { get; set; }
        public bool NoPrimary { get { return primary.Equals(NoWeaponString); } }

        public GoldfishModule() : this(null, false) { }

        public GoldfishModule(Pawn owner) : this(owner, false) { }

        public GoldfishModule(Pawn owner, bool fillExisting)
        {
            this.Owner = owner;
            if (fillExisting)
            {
                List<Thing> meleeWeapons;
                List<Thing> rangedWeapons;
                GettersFilters.getWeaponLists(out meleeWeapons, out rangedWeapons, Owner.inventory);
                foreach(Thing weapon in meleeWeapons)
                {
                    AddSidearm(weapon.def);
                }
                foreach (Thing weapon in rangedWeapons)
                {
                    AddSidearm(weapon.def);
                }
                if (owner.equipment != null && owner.equipment.Primary != null)
                {
                    AddSidearm(owner.equipment.Primary.def);
                    SetPrimary(owner.equipment.Primary.def, true);
                }
                else
                    SetPrimaryEmpty(true);
            }
        }

        public void ExposeData()
        {
            Scribe_Collections.Look<string>(ref weapons, "weapons", LookMode.Value);
            Scribe_Values.Look<string>(ref primary, "primary", NoWeaponString, true);
        }

        public static GoldfishModule GetGoldfishForPawn(Pawn pawn)
        {
            if (pawn == null)
                return null;
            if (SimpleSidearms.CEOverride)
                return null;
            if (SimpleSidearms.saveData == null)
                return null;
            var pawnId = pawn.thingIDNumber;
            GoldfishModule memory;
            if (!SimpleSidearms.saveData.memories.TryGetValue(pawnId, out memory))
            {
                memory = new GoldfishModule(pawn, true);
                SimpleSidearms.saveData.memories.Add(pawnId, memory);
            }
            return memory;
        }

        internal void SetPrimaryEmpty(bool intentional)
        {
            if (intentional)
                primary = NoWeaponString;
        }

        internal void AddSidearm(ThingDef def)
        {
            if(!weapons.Contains(def.defName))
                weapons.Add(def.defName);
        }

        internal void DropPrimary(bool intentional)
        {
            if (intentional)
            {
                weapons.Remove(primary);
                primary = NoWeaponString;
            }
            LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SimpleSidearmsDropping, OpportunityType.Important);
        }

        internal void DropSidearm(ThingDef def, bool intentional)
        {
            if (intentional)
            {
                if (weapons.Contains(def.defName))
                    weapons.Remove(def.defName);
                if (primary.Equals(def.defName))
                    primary = NoWeaponString;
            }
        }

        internal void PickupPrimary(ThingDef def, bool intentional)
        {
            if (!NoPrimary)
                weapons.Remove(primary);

            AddSidearm(def);
            if (intentional)
                SetPrimary(def, intentional);
        }

        internal void SetPrimary(ThingDef def, bool intentional)
        {
            if(def != null)
            {
                if (intentional)
                    primary = def.defName;
            }
            else
            {
                if (intentional)
                    primary = NoWeaponString;
            }
        }

        internal bool IsCurrentPrimary(string defName)
        {
            return defName.Equals(primary);
        }

        internal void ForgetSidearm(ThingDef def)
        {
            if (weapons.Contains(def.defName))
                weapons.Remove(def.defName);
            if (primary.Equals(def.defName))
                primary = NoWeaponString;
        }
    }
}
