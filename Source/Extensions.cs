﻿using RimWorld;
using SimpleSidearms.rimworld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace SimpleSidearms
{
    public static class Extensions
    {
        public static string getLabelCap(this ThingStuffPair pair)
        {
            return pair.getLabel().CapitalizeFirst();
        }

        public static string getLabel(this ThingStuffPair pair)
        {
            if (pair.stuff != null)
                return pair.stuff.LabelAsStuff + " " + pair.thing.label;
            else
                return pair.thing.label;
        }

        public static Color getDrawColor(this ThingStuffPair pair)
        {
            if (pair.stuff != null)
            {
                return pair.stuff.stuffProps.color;
            }
            if (pair.thing.graphicData != null)
            {
                return pair.thing.graphicData.color;
            }
            return Color.white;
        }

        public static Color getDrawColorTwo(this ThingStuffPair pair)
        {
            if (pair.thing.graphicData != null)
            {
                return pair.thing.graphicData.colorTwo;
            }
            return Color.white;
        }

        public static ThingStuffPair toThingStuffPair(this Thing thing)
        {
            return new ThingStuffPair(thing.def, thing.Stuff);
        }

        public static ThingStuffPairExposable toExposable(this ThingStuffPair pair)
        {
            return new ThingStuffPairExposable(pair);
        }

        public static float getBestStatBoost(this ThingStuffPair tool, List<StatDef> stats, out bool found)
        {
            if (tool.thing.equippedStatOffsets == null || tool.thing.equippedStatOffsets.Count == 0)
            {
                found = false;
                return 0;
            }

            //this is not great because not all stats are boosted equally
            //but its something

            float best = 0;
            found = false;
            foreach (StatModifier modifier in tool.thing.equippedStatOffsets)
            {
                if (stats.Contains(modifier.stat)) 
                {
                    found = true;
                    if(best < modifier.value)
                        best = modifier.value;
                }
            }
            return best;
        }

        public static bool isToolNotWeapon(this ThingStuffPair pair)
        {
            if (
                pair.thing.defName == "Gun_Fire_Ext" ||
                pair.thing.defName == "VWE_Gun_FireExtinguisher"
                )
            {
                return true;
            }
            return false;
        }

        public static bool matchesThingStuffPair(this Thing thing, ThingStuffPair pair, bool allowPartialMatch = false)
        {
            bool retVal = false;
            var thisPair = thing.toThingStuffPair();
            if(thisPair.thing == pair.thing && thisPair.stuff == pair.stuff)
                retVal = true;
            else if(allowPartialMatch && thisPair.thing == pair.thing)
                retVal = true;

            return retVal;
        }

        public static GoldfishModule.PrimaryWeaponMode getSkillWeaponPreference(this Pawn pawn)
        {
            if (pawn.skills == null)
                return GoldfishModule.PrimaryWeaponMode.Ranged;

            SkillRecord rangedSkill = pawn.skills.GetSkill(SkillDefOf.Shooting);
            SkillRecord meleeSkill = pawn.skills.GetSkill(SkillDefOf.Melee);

            if (rangedSkill.passion > meleeSkill.passion)
                return GoldfishModule.PrimaryWeaponMode.Ranged;
            else if (meleeSkill.passion > rangedSkill.passion)
                return GoldfishModule.PrimaryWeaponMode.Melee;
            else if (meleeSkill.Level > rangedSkill.Level)
                return GoldfishModule.PrimaryWeaponMode.Melee;
            else
                return GoldfishModule.PrimaryWeaponMode.Ranged; //slight bias towards ranged but *shrug*
        }

        public static IEnumerable<ThingWithComps> getCarriedWeapons(this Pawn pawn, bool includeEquipped = true, bool includeTools = false)
        {
            List<ThingWithComps> weapons = new List<ThingWithComps>();

            if (pawn == null || pawn.equipment == null || pawn.inventory == null)
                return weapons;

            if (includeEquipped)
            {
                if (pawn.equipment.Primary != null && (!pawn.equipment.Primary.toThingStuffPair().isToolNotWeapon() || includeTools))
                    weapons.Add(pawn.equipment.Primary);
            }

            foreach (Thing item in pawn.inventory.innerContainer)
            {
                if (
                    item is ThingWithComps &&
                    (!item.toThingStuffPair().isToolNotWeapon() || includeTools) &&
                    (item.def.IsRangedWeapon || item.def.IsMeleeWeapon))
                {
                    weapons.Add(item as ThingWithComps);
                }
            }
            return weapons;
        }

        public static bool hasWeaponSomewhere(this Pawn pawn, ThingStuffPair weapon, int duplicatesToSkip = 0)
        {
            if (pawn == null)
            {
                Log.Warning("hasWeaponSomewhere got handed null pawn");
                return false;
            }

            int dupesSoFar = 0;

            if (pawn.equipment != null)
                if (pawn.equipment.Primary != null)
                    if (pawn.equipment.Primary.matchesThingStuffPair(weapon))
                        dupesSoFar++;

            if (duplicatesToSkip < dupesSoFar)
                return true;

            if (pawn.inventory != null)
            {
                if (pawn.inventory.innerContainer != null)
                {
                    foreach (Thing thing in pawn.inventory.innerContainer)
                    {
                        if (thing.matchesThingStuffPair(weapon))
                        {
                            dupesSoFar++;
                            if (duplicatesToSkip < dupesSoFar)
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool Contains(this Rect rect, Rect otherRect)
        {
            if (!rect.Contains(new Vector2(otherRect.xMin, otherRect.yMin)))
                return false;
            if (!rect.Contains(new Vector2(otherRect.xMin, otherRect.yMax)))
                return false;
            if (!rect.Contains(new Vector2(otherRect.xMax, otherRect.yMax)))
                return false;
            if (!rect.Contains(new Vector2(otherRect.xMax, otherRect.yMin)))
                return false;
            return true;
        }

        public static bool ButtonText(Rect rect, string label, Color color, bool drawBackground = true, bool doMouseoverSound = false, bool active = true)
        {
            return Widgets.ButtonText(rect, label, drawBackground, doMouseoverSound, color, active);
        } 
    }
}