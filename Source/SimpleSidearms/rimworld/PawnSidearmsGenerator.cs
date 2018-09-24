using RimWorld;
using SimpleSidearms.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SimpleSidearms.rimworld
{
    public static class PawnSidearmsGenerator
    {
        private static List<ThingStuffPair> allWeaponPairs;

        private static List<ThingStuffPair> workingWeapons = new List<ThingStuffPair>();

        private static bool hasBeenReset = false;


        internal static List<ThingStuffPair> getWeaponsList()
        {
            if (!hasBeenReset)
                Reset();

            return allWeaponPairs;
        }

        public static void Reset()
        {
            //Log.Message("reset called");
            hasBeenReset = true;
            Predicate<ThingDef> isWeapon = (ThingDef td) => td.equipmentType == EquipmentType.Primary && !td.weaponTags.NullOrEmpty<string>();
            allWeaponPairs = ThingStuffPair.AllWith(isWeapon);
            foreach (ThingDef thingDef in from td in DefDatabase<ThingDef>.AllDefs
                                          where isWeapon(td)
                                          select td)
            {
                float num = allWeaponPairs.Where((ThingStuffPair pa) => pa.thing == thingDef).Sum((ThingStuffPair pa) => pa.Commonality);
                float num2 = thingDef.generateCommonality / num;
                if (num2 != 1f)
                {
                    for (int i = 0; i < allWeaponPairs.Count; i++)
                    {
                        ThingStuffPair thingStuffPair = allWeaponPairs[i];
                        if (thingStuffPair.thing == thingDef)
                        {
                            allWeaponPairs[i] = new ThingStuffPair(thingStuffPair.thing, thingStuffPair.stuff, thingStuffPair.commonalityMultiplier * num2);
                        }
                    }
                }
            }
        }

        public static void TryGenerateSidearmsFor(Pawn pawn)
        {
            if (SimpleSidearms.SidearmSpawnChance.Value < 0.01f)
                return;

            if (!hasBeenReset)
                Reset();


            if (Rand.ValueSeeded(pawn.thingIDNumber ^ 28554824) >= SimpleSidearms.SidearmSpawnChance.Value)
            {
                return;
            }
            if (pawn.kindDef.weaponTags == null || pawn.kindDef.weaponTags.Count == 0)
            {
                return;
            }
            if (!pawn.RaceProps.ToolUser)
            {
                return;
            }
            if (!pawn.RaceProps.Humanlike)
            {
                return;
            }
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                return;
            }
            if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
            {
                return;
            }
            if (pawn.equipment.Primary == null)
            {
                return;
            }

            bool meleeOnly = pawn.equipment.Primary.def.IsMeleeWeapon || (pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler));
            bool neolithic = true;
            foreach(string wepTag in pawn.kindDef.weaponTags)
            {
                if(wepTag.Contains("Neolithic"))
                {
                    neolithic = false;
                    break;
                }
            }

            List<string> sidearmTags = GettersFilters.weaponTagsToSidearmTags(pawn.kindDef.weaponTags);

            float money = pawn.kindDef.weaponMoney.min;
            money /= 5f;

            for (int i = 0; i < allWeaponPairs.Count; i++)
            {
                ThingStuffPair w = allWeaponPairs[i];

                if (!StatCalculator.canCarrySidearm(w.thing, pawn))
                    continue;
                if (w.Price > money)
                    continue;
                if (meleeOnly && w.thing.IsRangedWeapon)
                    continue;
                if (neolithic)
                {
                    bool isNeolithic = true;
                    #region nestedMonstrosity
                    foreach (string wepTag in pawn.kindDef.weaponTags)
                    {
                        if (!wepTag.Contains("Neolithic"))
                        {
                            //check if the weapon is allowed despite not being neolithic
                            bool getsAPass = false;
                            foreach (string weapon in SimpleSidearms.SidearmsNeolithicExtension.Value.InnerList)
                            {
                                if (weapon.Equals(w.thing.defName))
                                {
                                    getsAPass = true;
                                    break;
                                }
                            }
                            if (!getsAPass)
                                isNeolithic = false;
                        }
                        if (!isNeolithic)
                            break;
                    }
                    #endregion
                    if (!isNeolithic)
                        continue;
                }
                
                if (sidearmTags.Any((string tag) => w.thing.weaponTags.Contains(tag)))
                {
                    if (w.thing.generateAllowChance >= 1f || Rand.ValueSeeded(pawn.thingIDNumber ^ 28554824) <= w.thing.generateAllowChance)
                    {
                        workingWeapons.Add(w);
                    }
                }

            }
            if (workingWeapons.Count == 0)
            {
                return;
            }
            ThingStuffPair thingStuffPair;
            if (workingWeapons.TryRandomElementByWeight((ThingStuffPair w) => w.Commonality * w.Price / w.thing.BaseMass, out thingStuffPair))
            {
                ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(thingStuffPair.thing, thingStuffPair.stuff);
                PawnGenerator.PostProcessGeneratedGear(thingWithComps, pawn);
                pawn.inventory.innerContainer.TryAdd(thingWithComps);
                //pawn.equipment.AddEquipment(thingWithComps);
            }
            workingWeapons.Clear();
        }
    }
}