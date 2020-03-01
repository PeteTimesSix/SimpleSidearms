using RimWorld;
using SimpleSidearms.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace SimpleSidearms.rimworld
{
    [StaticConstructorOnStartup]
    public class GoldfishModule : IExposable
    {
        public enum PrimaryWeaponMode
        {
            Ranged,
            Melee, 
            BySkill,
            ByGenerated
        }

        public List<ThingStuffPairExposable> rememberedWeapons = new List<ThingStuffPairExposable>();
        public List<ThingStuffPair> RememberedWeapons { get
            {
                if (rememberedWeapons == null)
                    generateRememberedWeaponsFromEquipped();
                List<ThingStuffPair> fakery = new List<ThingStuffPair>();
                foreach (var wep in rememberedWeapons)
                    fakery.Add(wep.Val);
                return fakery;
            } }

        public bool forcedUnarmedEx = false;
        public ThingStuffPairExposable? forcedWeaponEx = null;
        public bool forcedUnarmedWhileDraftedEx = false;
        public ThingStuffPairExposable? forcedWeaponWhileDraftedEx = null;

        public bool preferredUnarmedEx = false;
        public ThingStuffPairExposable? defaultRangedWeaponEx = null;
        public ThingStuffPairExposable? preferredMeleeWeaponEx = null;

        public Toil autotoolToil = null;
        public int delayIdleSwitchTimestamp = int.MinValue;

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
        public ThingStuffPair? ForcedWeapon
        {
            get
            {
                if (forcedWeaponEx == null)
                    return null;
                return forcedWeaponEx.Value.Val;
            }
            set
            {
                if (value == null)
                    forcedWeaponEx = null;
                else
                    forcedWeaponEx = new ThingStuffPairExposable(value.Value);
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
        public ThingStuffPair? ForcedWeaponWhileDrafted
        {
            get
            {
                if (forcedWeaponWhileDraftedEx != null)
                    return forcedWeaponWhileDraftedEx.Value.Val;
                else
                    return null;
            }
            set
            {
                if (value == null)
                    forcedWeaponWhileDraftedEx = null;
                else
                    forcedWeaponWhileDraftedEx = new ThingStuffPairExposable(value.Value);
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
        public ThingStuffPair? DefaultRangedWeapon {
            get
            {
                if (defaultRangedWeaponEx == null)
                    return null;
                return defaultRangedWeaponEx.Value.Val;
            }
            private set
            {
                if (value == null)
                    defaultRangedWeaponEx = null;
                else
                    defaultRangedWeaponEx = new ThingStuffPairExposable(value.Value);
            }
        }
        public ThingStuffPair? PreferredMeleeWeapon
        {
            get
            {
                if (preferredMeleeWeaponEx == null)
                    return null;
                return preferredMeleeWeaponEx.Value.Val;
            }
            private set
            {
                if (value == null)
                    preferredMeleeWeaponEx = null;
                else
                    preferredMeleeWeaponEx = new ThingStuffPairExposable(value.Value);
            }
        }

        public PrimaryWeaponMode primaryWeaponMode = PrimaryWeaponMode.BySkill;

        public Pawn ownerEx;
        public Pawn Owner { get { return ownerEx; } set { ownerEx = value; } }

        public GoldfishModule() : this(null, false) { }

        public GoldfishModule(Pawn owner) : this(owner, false) { }

        public GoldfishModule(Pawn owner, bool fillExisting)
        {
            this.rememberedWeapons = new List<ThingStuffPairExposable>();
            this.Owner = owner;
            if (fillExisting)
            {
                generateRememberedWeaponsFromEquipped();
            }
            if (owner != null) //null owner should only come up when loading from savegames
            {
                if (owner.IsColonist)
                    primaryWeaponMode = SimpleSidearms.ColonistDefaultWeaponMode.Value;
                else
                    primaryWeaponMode = SimpleSidearms.NPCDefaultWeaponMode.Value;

                if (primaryWeaponMode == PrimaryWeaponMode.ByGenerated)
                {
                    if (Owner == null || Owner.equipment == null || owner.equipment.Primary == null)
                        primaryWeaponMode = PrimaryWeaponMode.BySkill;
                    else if (owner.equipment.Primary.def.IsRangedWeapon)
                        primaryWeaponMode = PrimaryWeaponMode.Ranged;
                    else if (owner.equipment.Primary.def.IsMeleeWeapon)
                        primaryWeaponMode = PrimaryWeaponMode.Melee;
                    else
                        primaryWeaponMode = PrimaryWeaponMode.BySkill;
                }
            }
        }

        public void generateRememberedWeaponsFromEquipped()
        {
            this.rememberedWeapons = new List<ThingStuffPairExposable>();
            IEnumerable<ThingWithComps> carriedWeapons = Owner.getCarriedWeapons(includeTools: true);
            foreach (ThingWithComps weapon in carriedWeapons)
            {
                ThingStuffPairExposable pair = new ThingStuffPairExposable(new ThingStuffPair(weapon.def, weapon.Stuff));
                rememberedWeapons.Add(pair);
            }
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref ownerEx, "owner");

            Scribe_Collections.Look<ThingStuffPairExposable>(ref rememberedWeapons, "rememberedWeapons", LookMode.Deep);

            Scribe_Deep.Look<ThingStuffPairExposable?>(ref forcedWeaponEx, "forcedWeapon");
            Scribe_Values.Look<bool>(ref forcedUnarmedEx, "forcedUnarmed");
            Scribe_Deep.Look<ThingStuffPairExposable?>(ref forcedWeaponWhileDraftedEx, "forcedWeaponWhileDrafted");
            Scribe_Values.Look<bool>(ref forcedUnarmedWhileDraftedEx, "forcedUnarmedWhileDrafted");

            Scribe_Values.Look<bool>(ref preferredUnarmedEx, "preferredUnarmed");
            Scribe_Deep.Look<ThingStuffPairExposable?>(ref defaultRangedWeaponEx, "prefferedRangedWeapon");
            Scribe_Deep.Look<ThingStuffPairExposable?>(ref preferredMeleeWeaponEx, "prefferedMeleeWeapon");
            Scribe_Values.Look<PrimaryWeaponMode>(ref primaryWeaponMode, "primaryWeaponMode");
        }

        public static GoldfishModule GetGoldfishForPawn(Pawn pawn, bool fillExistingIfCreating = true)
        {
            if (pawn == null)
                return null;
            if (SimpleSidearms.CEOverride)
                return null;
            if (SimpleSidearms.configData == null)
                return null;
            var pawnId = pawn.thingIDNumber;
            GoldfishModule memory;
            if (!SimpleSidearms.configData.memories.TryGetValue(pawnId, out memory))
            {
                memory = new GoldfishModule(pawn, fillExistingIfCreating);
                SimpleSidearms.configData.memories.Add(pawnId, memory);
            }
            else
            {
                memory.NullChecks(pawn);
            }
            return memory;
        }

        public bool IsCurrentWeaponForced(bool alsoCountPreferredOrDefault)
        {
            if (Owner == null || Owner.Dead || Owner.equipment == null)
                return false;
            ThingStuffPair? currentWeaponN = Owner.equipment.Primary?.toThingStuffPair();
            if (currentWeaponN == null)
            {
                if (Owner.Drafted && ForcedUnarmedWhileDrafted)
                    return true;
                else if (ForcedUnarmed)
                    return true;
                else if (alsoCountPreferredOrDefault && PreferredUnarmed)
                    return true;
                else
                    return false;
            }
            else
            {
                ThingStuffPair currentWeapon = currentWeaponN.Value;
                if (Owner.Drafted && ForcedWeaponWhileDrafted == currentWeapon)
                    return true;
                else if (ForcedWeapon == currentWeapon)
                    return true;
                else if (alsoCountPreferredOrDefault)
                {
                    if (currentWeapon.thing.IsMeleeWeapon && PreferredMeleeWeapon == currentWeapon)
                        return true;
                    else if (currentWeapon.thing.IsRangedWeapon && DefaultRangedWeapon == currentWeapon)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
        }

        public void SetUnarmedAsForced(bool drafted)
        {
            if (drafted)
            {
                ForcedUnarmedWhileDrafted = true;
                ForcedWeaponWhileDrafted = null;
            }
            else
            {
                ForcedUnarmed = true;
                ForcedWeapon = null;
            }
        }

        public void SetWeaponAsForced(ThingStuffPair weapon, bool drafted)
        {
            if (drafted)
            {
                ForcedUnarmedWhileDrafted = false;
                ForcedWeaponWhileDrafted = weapon;
            }
            else
            {
                ForcedUnarmed = false;
                ForcedWeapon = weapon;
            }
        }


        public void UnsetUnarmedAsForced(bool drafted)
        {
            if (drafted)
            {
                ForcedUnarmedWhileDrafted = false;
                ForcedWeaponWhileDrafted = null;
            }
            else
            {
                ForcedUnarmed = false;
                ForcedWeapon = null;
            }
        }

        public void UnsetForcedWeapon(bool drafted)
        {
            if(drafted)
            {
                ForcedUnarmedWhileDrafted = false;
                ForcedWeaponWhileDrafted = null;
            }
            else
            {
                ForcedUnarmed = false;
                ForcedWeapon = null;
            }
        }

        public void SetRangedWeaponTypeAsDefault(ThingStuffPair rangedWeapon)
        {
            this.DefaultRangedWeapon = rangedWeapon;
            if (this.ForcedWeapon != null && this.ForcedWeapon != rangedWeapon && this.ForcedWeapon.Value.thing.IsRangedWeapon)
                UnsetForcedWeapon(false);
        }
        public void SetMeleeWeaponTypeAsPreferred(ThingStuffPair meleeWeapon)
        {
            this.preferredUnarmedEx = false;
            this.PreferredMeleeWeapon = meleeWeapon;
            if (this.ForcedWeapon != null && this.ForcedWeapon != meleeWeapon && this.ForcedWeapon.Value.thing.IsMeleeWeapon)
                UnsetForcedWeapon(false);
            if (ForcedUnarmed)
                UnsetUnarmedAsForced(false);
        }
        public void SetUnarmedAsPreferredMelee()
        {
            PreferredUnarmed = true;
            PreferredMeleeWeapon = null;
            if (this.ForcedWeapon != null && this.ForcedWeapon.Value.thing.IsMeleeWeapon)
                UnsetForcedWeapon(false);
        }

        public void UnsetRangedWeaponDefault()
        {
            DefaultRangedWeapon = null;
        }
        public void UnsetMeleeWeaponPreference()
        {
            PreferredMeleeWeapon = null;
            PreferredUnarmed = false;
        }

        public void InformOfUndraft()
        {
            ForcedWeaponWhileDrafted = null;
            ForcedUnarmedWhileDrafted = false;
        }

        public void InformOfAddedPrimary(Thing weapon)
        {
            InformOfAddedSidearm(weapon);

            if (weapon.def.IsRangedWeapon)
                SetRangedWeaponTypeAsDefault(weapon.toThingStuffPair());
            else
                SetMeleeWeaponTypeAsPreferred(weapon.toThingStuffPair());
        }
        public void InformOfAddedSidearm(Thing weapon)
        {
            ThingStuffPair weaponType = weapon.toThingStuffPair();
            var carriedOfType = Owner.getCarriedWeapons(includeTools: true).Where(w => w.toThingStuffPair() == weaponType);
            var rememberedOfType = rememberedWeapons.Where(w => w.Val == weaponType);

            if(rememberedOfType.Count() < carriedOfType.Count())
                rememberedWeapons.Add(weapon.toThingStuffPair().toExposable());
        }

        public void InformOfDroppedSidearm(Thing weapon, bool intentional)
        {
            if (intentional)
                ForgetSidearmMemory(weapon.toThingStuffPair());
        }

        public void ForgetSidearmMemory(ThingStuffPair weaponMemory)
        {
            if (rememberedWeapons.Contains(weaponMemory.toExposable()))
                rememberedWeapons.Remove(weaponMemory.toExposable());

            if (!rememberedWeapons.Contains(weaponMemory.toExposable())) //only remove if this was the last instance
            {
                if (weaponMemory == ForcedWeapon)
                    ForcedWeapon = null;
                if (weaponMemory == PreferredMeleeWeapon)
                    PreferredMeleeWeapon = null;
                if (weaponMemory == DefaultRangedWeapon)
                    PreferredMeleeWeapon = null;
            }
        }



        public bool nullchecked = false;

        public void NullChecks(Pawn owner)
        {
            if (nullchecked)
                return;
            if(Owner == null)
            {
                Log.Warning("goldfish module didnt know what pawn it belongs to!");
                this.Owner = owner;
            }
            if(rememberedWeapons == null)
            {
                Log.Warning("Remembered weapons list of " + this.Owner.LabelCap + " was missing, regenerating...");
                generateRememberedWeaponsFromEquipped();
            }
            for (int i = rememberedWeapons.Count() - 1; i >= 0; i--)
            {
                try
                {
                    var pair = rememberedWeapons[i];
                    var disposed = pair.Val;
                }
                catch (Exception ex)
                {
                    Log.Warning("A memorised weapon of " + this.Owner.LabelCap + " had a missing def or malformed data, removing...");
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
                    Log.Warning("Melee weapon preference of " + this.Owner.LabelCap + " had a missing def or malformed data, removing...");
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
                    Log.Warning("Ranged weapon preference of " + this.Owner.LabelCap + " had a missing def or malformed data, removing...");
                    PreferredMeleeWeapon = null;
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
                    Log.Warning("Forced weapon of " + this.Owner.LabelCap + " had a missing def or malformed data, removing...");
                    PreferredMeleeWeapon = null;
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
                    Log.Warning("Forced drafted weapon of " + this.Owner.LabelCap + " had a missing def or malformed data, removing...");
                    PreferredMeleeWeapon = null;
                }
            }
            nullchecked = true;
        }
    }
}
