using RimWorld;
using SimpleSidearms.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using static SimpleSidearms.Globals;

namespace SimpleSidearms.rimworld
{
    [StaticConstructorOnStartup]
    public class GoldfishModule : IExposable
    {
        public List<ThingDefStuffDefPair> rememberedWeapons = new List<ThingDefStuffDefPair>();
        public List<ThingDefStuffDefPair> RememberedWeapons { get
            {
                if (rememberedWeapons == null)
                    generateRememberedWeaponsFromEquipped();
                List<ThingDefStuffDefPair> fakery = new List<ThingDefStuffDefPair>();
                foreach (var wep in rememberedWeapons)
                    fakery.Add(wep);
                return fakery;
            } }

        public bool forcedUnarmedEx = false;
        public ThingDefStuffDefPair? forcedWeaponEx = null;
        public bool forcedUnarmedWhileDraftedEx = false;
        public ThingDefStuffDefPair? forcedWeaponWhileDraftedEx = null;

        public bool preferredUnarmedEx = false;
        public ThingDefStuffDefPair? defaultRangedWeaponEx = null;
        public ThingDefStuffDefPair? preferredMeleeWeaponEx = null;

        public bool currentJobWeaponReequipDelayed;
        public JobDef autotoolJob = null;
        public Toil autotoolToil = null;
        public int delayIdleSwitchTimestamp = -60;

        public bool ForcedUnarmed
        {
            get
            {
                return forcedUnarmedEx;
            }
            set
            {
                if (value == true)
                    ForcedWeapon = null;
                forcedUnarmedEx = value;
            }
        }
        public ThingDefStuffDefPair? ForcedWeapon
        {
            get
            {
                if (forcedWeaponEx == null)
                    return null;
                return forcedWeaponEx.Value;
            }
            set
            {
                if (value == null)
                    forcedWeaponEx = null;
                else
                    forcedWeaponEx = value;
            }
        }

        public bool ForcedUnarmedWhileDrafted
        {
            get
            {
                return forcedUnarmedWhileDraftedEx;
            }
            set
            {
                if (value == true)
                    ForcedWeaponWhileDrafted = null;
                forcedUnarmedWhileDraftedEx = value;
            }
        }
        public ThingDefStuffDefPair? ForcedWeaponWhileDrafted
        {
            get
            {
                if (forcedWeaponWhileDraftedEx != null)
                    return forcedWeaponWhileDraftedEx.Value;
                else
                    return null;
            }
            set
            {
                if (value == null)
                    forcedWeaponWhileDraftedEx = null;
                else
                    forcedWeaponWhileDraftedEx = value;
            }
        }


        public bool PreferredUnarmed
        {
            get
            {
                return preferredUnarmedEx;
            }
            private set
            {
                if (value == true)
                    PreferredMeleeWeapon = null;
                preferredUnarmedEx = value;
            }
        }
        public ThingDefStuffDefPair? DefaultRangedWeapon {
            get
            {
                if (defaultRangedWeaponEx == null)
                    return null;
                return defaultRangedWeaponEx.Value;
            }
            private set
            {
                if (value == null)
                    defaultRangedWeaponEx = null;
                else
                    defaultRangedWeaponEx = value;
            }
        }
        public ThingDefStuffDefPair? PreferredMeleeWeapon
        {
            get
            {
                if (preferredMeleeWeaponEx == null)
                    return null;
                return preferredMeleeWeaponEx.Value;
            }
            private set
            {
                if (value == null)
                    preferredMeleeWeaponEx = null;
                else
                    preferredMeleeWeaponEx = value;
            }
        }

        public PrimaryWeaponMode primaryWeaponMode = PrimaryWeaponMode.BySkill;

        public Pawn ownerEx;
        public Pawn Owner { get { return ownerEx; } set { ownerEx = value; } }

        public GoldfishModule() { }

        public void ExposeData()
        {
            Scribe_References.Look(ref ownerEx, "owner");

            Scribe_Collections.Look<ThingDefStuffDefPair>(ref rememberedWeapons, "rememberedWeapons", LookMode.Deep);

            Scribe_Deep.Look<ThingDefStuffDefPair?>(ref forcedWeaponEx, "forcedWeapon");
            Scribe_Values.Look<bool>(ref forcedUnarmedEx, "forcedUnarmed");
            Scribe_Deep.Look<ThingDefStuffDefPair?>(ref forcedWeaponWhileDraftedEx, "forcedWeaponWhileDrafted");
            Scribe_Values.Look<bool>(ref forcedUnarmedWhileDraftedEx, "forcedUnarmedWhileDrafted");

            Scribe_Values.Look<bool>(ref preferredUnarmedEx, "preferredUnarmed");
            Scribe_Deep.Look<ThingDefStuffDefPair?>(ref defaultRangedWeaponEx, "defaultRangedWeapon");
            Scribe_Deep.Look<ThingDefStuffDefPair?>(ref preferredMeleeWeaponEx, "preferredMeleeWeapon");
            Scribe_Values.Look<PrimaryWeaponMode>(ref primaryWeaponMode, "primaryWeaponMode");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                NullChecks(this.Owner);
            }
        }

        public static bool warnedOfMissingReference = false;
        public bool nullchecked = false;

        public void NullChecks(Pawn owner)
        {
            if (nullchecked)
                return;
            if (Owner == null)
            {
                if (!warnedOfMissingReference)
                {
                    //Log.Warning("(SimpleSidearms) Found a GoldfishModule with a missing owner reference, regenerating... (additional occurences will be silent)");
                    warnedOfMissingReference = true;
                }
                this.Owner = owner;
            }
            if (rememberedWeapons == null)
            {
                //Log.Warning("Remembered weapons list of " + this.Owner.LabelCap + " was missing, regenerating...");
                generateRememberedWeaponsFromEquipped();
            }
            for (int i = rememberedWeapons.Count() - 1; i >= 0; i--)
            {
                try
                {
                    var disposed = rememberedWeapons[i];
                }
                catch (Exception ex)
                {
                    //Log.Warning("A memorised weapon of " + this.Owner.LabelCap + " had a missing def or malformed data, removing. Exception:" + ex.Message);
                    rememberedWeapons.RemoveAt(i);
                }
            }
            if (PreferredMeleeWeapon != null)
            {
                try
                {
                    var disposed = PreferredMeleeWeapon.Value;
                }
                catch (Exception ex)
                {
                    //Log.Warning("Melee weapon preference of " + this.Owner.LabelCap + " had a missing def or malformed data, removing. Exception:" + ex.Message);
                    PreferredMeleeWeapon = null;
                }
            }
            if (DefaultRangedWeapon != null)
            {
                try
                {
                    var disposed = DefaultRangedWeapon.Value;
                }
                catch (Exception ex)
                {
                    //Log.Warning("Ranged weapon preference of " + this.Owner.LabelCap + " had a missing def or malformed data, removing. Exception:" + ex.Message);
                    DefaultRangedWeapon = null;
                }
            }
            if (ForcedWeapon != null)
            {
                try
                {
                    var disposed = ForcedWeapon.Value;
                }
                catch (Exception ex)
                {
                    //Log.Warning("Forced weapon of " + this.Owner.LabelCap + " had a missing def or malformed data, removing. Exception:" + ex.Message);
                    ForcedWeapon = null;
                }
            }
            if (ForcedWeaponWhileDrafted != null)
            {
                try
                {
                    var disposed = ForcedWeaponWhileDrafted.Value;
                }
                catch (Exception ex)
                {
                    //Log.Warning("Forced drafted weapon of " + this.Owner.LabelCap + " had a missing def or malformed data, removing. Exception:" + ex.Message);
                    ForcedWeaponWhileDrafted = null;
                }
            }
            nullchecked = true;
        }


        public void generateRememberedWeaponsFromEquipped()
        {
            this.rememberedWeapons = new List<ThingDefStuffDefPair>();
            IEnumerable<ThingWithComps> carriedWeapons = Owner.getCarriedWeapons(includeTools: true);
            foreach (ThingWithComps weapon in carriedWeapons)
            {
                ThingDefStuffDefPair pair = new ThingDefStuffDefPair(weapon.def, weapon.Stuff);
                rememberedWeapons.Add(pair);
            }
        }
    }

}