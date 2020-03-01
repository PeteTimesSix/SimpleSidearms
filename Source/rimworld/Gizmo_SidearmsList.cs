using RimWorld;
using SimpleSidearms.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using static SimpleSidearms.Globals;

namespace SimpleSidearms.rimworld
{
    public class Gizmo_SidearmsList : Command
    {
        public const float ContentPadding = 2f;
        public const float MinGizmoSize = 75f;
        public const float IconSize = 32f;
        public const float IconGap = 1f;
        public const float SelectorPanelWidth = 32f + ContentPadding * 2;
        public const float PreferenceIconHeight = 21f;
        public const float PreferenceIconWidth = 32f;


        public static readonly Color iconBaseColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        public static readonly Color iconMouseOverColor = new Color(0.6f, 0.6f, 0.4f, 1f);

        public static readonly Color preferenceBase = new Color(0.5f, 0.5f, 0.5f, 1f);
        public static readonly Color preferenceSet = new Color(0.5f, 1.0f, 0.5f, 1f);
        public static readonly Color preferenceOfSkill = new Color(1.0f, 0.75f, 0.5f, 1f);
        public static readonly Color preferenceHighlight = new Color(0.7f, 0.7f, 0.4f, 1f);
        public static readonly Color preferenceHighlightSet = new Color(0.7f, 1.0f, 0.4f, 1f);

        //public Texture2D[] iconTextures;
        public Action hotkeyAction;

        public Pawn parent;
        public IEnumerable<ThingWithComps> carriedWeapons;
        public IEnumerable<ThingWithComps> carriedRangedWeapons { get { return carriedWeapons.Where(w => w.def.IsRangedWeapon); } }
        public IEnumerable<ThingWithComps> carriedMeleeWeapons { get { return carriedWeapons.Where(w => w.def.IsMeleeWeapon); } }

        public IEnumerable<ThingStuffPair> weaponMemories;
        public IEnumerable<ThingStuffPair> rangedWeaponMemories { get { return weaponMemories.Where(w => w.thing.IsRangedWeapon); } }
        public IEnumerable<ThingStuffPair> meleeWeaponMemories { get { return weaponMemories.Where(w => w.thing.IsMeleeWeapon); } }

        public enum SidearmsListInteraction
        {
            None,
            SelectorRanged,
            SelectorSkill,
            SelectorMelee,
            Weapon,
            WeaponMemory,
            Unarmed
        }
        public SidearmsListInteraction interactedWith = SidearmsListInteraction.None;
        public ThingWithComps interactionWeapon;
        public ThingStuffPair? interactionWeaponType;
        public bool interactionWeaponIsDuplicate;

        public override float GetWidth(float maxWidth)
        {
            GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(parent);
            //if (pawnMemory == null)
            //    return 75;
            int biggerCount = Math.Max(
                carriedRangedWeapons.Count() + countMissingRangedWeapons(pawnMemory, parent),
                carriedMeleeWeapons.Count() + countMissingMeleeWeapons(pawnMemory, parent) + 1
                );
            float width = SelectorPanelWidth + ContentPadding + (IconSize * biggerCount) + IconGap * (biggerCount - 1) + ContentPadding;
            return Math.Min(Math.Max(width, MinGizmoSize), maxWidth);
        }

        public Gizmo_SidearmsList(Pawn parent, IEnumerable<ThingWithComps> carriedWeapons, IEnumerable<ThingStuffPair> weaponMemories)
        {
            this.parent = parent;

            this.carriedWeapons = carriedWeapons;
            this.weaponMemories = weaponMemories;
            tutorTag = "SidearmsList";
            this.defaultLabel = "DrawSidearm_gizmoTitle".Translate();
            this.defaultDesc = "DrawSidearm_gizmoTooltip".Translate();
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            interactionWeaponIsDuplicate = false;
            interactionWeapon = null;
            interactionWeaponType = null;

            var gizmoRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), MinGizmoSize);

            if (Mouse.IsOver(gizmoRect))
            {
                LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SimpleSidearmsBasic, OpportunityType.Important);
            }

            var contentRect = gizmoRect.ContractedBy(ContentPadding);
            Widgets.DrawWindowBackground(gizmoRect);

            GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(parent);

            int i = 0;
            {
                Dictionary<ThingStuffPair, int> dupeCounters = new Dictionary<ThingStuffPair, int>();
                var rangedWeapons = carriedRangedWeapons.ToList();
                rangedWeapons.SortStable((a, b) => { return (int)((b.MarketValue - a.MarketValue) * 1000); });
                for (i = 0; i < rangedWeapons.Count(); i++)
                {
                    ThingStuffPair weaponMemory = rangedWeapons[i].toThingStuffPair();
                    if (!dupeCounters.ContainsKey(weaponMemory))
                        dupeCounters[weaponMemory] = 0;

                    bool isDupe = dupeCounters[weaponMemory] > 0;
                    var iconOffset = new Vector2((IconSize * i) + IconGap * (i - 1) + SelectorPanelWidth, 0);
                    DrawIconForWeapon(parent, pawnMemory, rangedWeapons[i], isDupe, contentRect, iconOffset);

                    dupeCounters[weaponMemory]++;
                }
            }

            int j = 0;
            if (pawnMemory != null)
            {
                Dictionary<ThingStuffPair, int> dupeCounters = new Dictionary<ThingStuffPair, int>();
                var rangedWeaponMemoriesSorted = rangedWeaponMemories.ToList();
                rangedWeaponMemoriesSorted.SortStable((a, b) => { return (int)((b.Price - a.Price) * 1000); });
                foreach (ThingStuffPair weaponMemory in rangedWeaponMemoriesSorted)
                {
                    if (!dupeCounters.ContainsKey(weaponMemory))
                        dupeCounters[weaponMemory] = 0;

                    if (!parent.hasWeaponSomewhere(weaponMemory, dupeCounters[weaponMemory]))
                    {
                        bool isDupe = dupeCounters[weaponMemory] > 0;
                        var iconOffset = new Vector2((IconSize * (i + j)) + IconGap * ((i + j) - 1) + SelectorPanelWidth, 0);
                        DrawIconForWeaponMemory(parent, pawnMemory, weaponMemory, isDupe, contentRect, iconOffset);
                        j++;
                    }

                    dupeCounters[weaponMemory]++;
                }
            }

            {
                Dictionary<ThingStuffPair, int> dupeCounters = new Dictionary<ThingStuffPair, int>();
                var meleeWeapons = carriedMeleeWeapons.ToList();
                meleeWeapons.SortStable((a, b) => { return (int)((b.MarketValue - a.MarketValue) * 1000); });
                for (i = 0; i < meleeWeapons.Count(); i++)
                {
                    ThingStuffPair weaponMemory = meleeWeapons[i].toThingStuffPair();
                    if (!dupeCounters.ContainsKey(weaponMemory))
                        dupeCounters[weaponMemory] = 0;

                    bool isDupe = dupeCounters[weaponMemory] > 0;
                    var iconOffset = new Vector2((IconSize * i) + IconGap * (i - 1) + SelectorPanelWidth, IconSize + IconGap);

                    DrawIconForWeapon(parent, pawnMemory, meleeWeapons[i], isDupe, contentRect, iconOffset);

                    dupeCounters[weaponMemory]++;
                }
            }
            

            j = 0;
            if (pawnMemory != null)
            {
                Dictionary<ThingStuffPair, int> dupeCounters = new Dictionary<ThingStuffPair, int>();
                var meleeWeaponMemoriesSorted = meleeWeaponMemories.ToList();
                meleeWeaponMemoriesSorted.SortStable((a, b) => { return (int)((b.Price - a.Price) * 1000); });
                foreach (ThingStuffPair weaponMemory in meleeWeaponMemoriesSorted)
                {
                    if (!dupeCounters.ContainsKey(weaponMemory))
                        dupeCounters[weaponMemory] = 0;

                    if (!parent.hasWeaponSomewhere(weaponMemory, dupeCounters[weaponMemory]))
                    {
                        bool isDupe = dupeCounters[weaponMemory] > 0;
                        var iconOffset = new Vector2((IconSize * (i + j)) + IconGap * ((i + j) - 1) + SelectorPanelWidth, IconSize + IconGap);

                        DrawIconForWeaponMemory(parent, pawnMemory, weaponMemory, isDupe, contentRect, iconOffset);

                        dupeCounters[weaponMemory]++;
                        j++;
                    }
                }
            }

            var unarmedIconOffset = new Vector2((IconSize * (i + j)) + IconGap * ((i + j) - 1) + SelectorPanelWidth, IconSize + IconGap);
            DrawIconForUnarmed(parent, pawnMemory, contentRect, unarmedIconOffset);

            Rect selectorPanel = new Rect(gizmoRect.x + ContentPadding, gizmoRect.y + ContentPadding, SelectorPanelWidth - ContentPadding * 2, MinGizmoSize - ContentPadding * 2);

            DrawPreferenceSelector(parent, pawnMemory, selectorPanel);

            UIHighlighter.HighlightOpportunity(gizmoRect, "SidearmList");

            if(parent.IsColonistPlayerControlled && ((parent.CombinedDisabledWorkTags & WorkTags.Violent) == 0))
                DrawGizmoLabel(defaultLabel, gizmoRect);
            else
                DrawGizmoLabel(defaultLabel+" (godmode)", gizmoRect);
            return interactedWith != SidearmsListInteraction.None ? new GizmoResult(GizmoState.Interacted, Event.current) : new GizmoResult(GizmoState.Clear);
        }


        public void DrawPreferenceSelector(Pawn pawn, GoldfishModule pawnMemory, Rect contentRect)
        {
            var rangedIconRect = new Rect(contentRect.x, contentRect.y, PreferenceIconWidth, PreferenceIconHeight);
            var skillIconRect = new Rect(contentRect.x, contentRect.y+ PreferenceIconHeight + IconGap, PreferenceIconWidth, PreferenceIconHeight);
            var meleeIconRect = new Rect(contentRect.x, contentRect.y+ (PreferenceIconHeight + IconGap) * 2, PreferenceIconWidth, PreferenceIconHeight);

            var skillPref = pawn.getSkillWeaponPreference();

            if (Mouse.IsOver(contentRect))
            {
                LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SimpleSidearmsBasic, OpportunityType.Important);
                LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SimpleSidearmsPreference, OpportunityType.Important);
            }

            if (Mouse.IsOver(rangedIconRect))
            {
                if (pawnMemory.primaryWeaponMode == GoldfishModule.PrimaryWeaponMode.Ranged)
                    GUI.color = preferenceHighlightSet;
                else
                    GUI.color = preferenceHighlight;
                GUI.DrawTexture(rangedIconRect, TextureResources.preferRanged);
                TooltipHandler.TipRegion(rangedIconRect, string.Format("SidearmPreference_Ranged".Translate()));
                MouseoverSounds.DoRegion(rangedIconRect, SoundDefOf.Mouseover_Command);
            }
            else
            {
                if (pawnMemory.primaryWeaponMode == GoldfishModule.PrimaryWeaponMode.Ranged)
                    GUI.color = preferenceSet;
                else if (pawnMemory.primaryWeaponMode == GoldfishModule.PrimaryWeaponMode.BySkill && skillPref == GoldfishModule.PrimaryWeaponMode.Ranged)
                    GUI.color = preferenceOfSkill;
                else
                    GUI.color = preferenceBase;
                GUI.DrawTexture(rangedIconRect, TextureResources.preferRanged);
            }

            if (Mouse.IsOver(skillIconRect))
            {
                if (pawnMemory.primaryWeaponMode == GoldfishModule.PrimaryWeaponMode.BySkill)
                    GUI.color = preferenceHighlightSet;
                else
                    GUI.color = preferenceHighlight;
                GUI.DrawTexture(skillIconRect, TextureResources.preferSkilled);
                TooltipHandler.TipRegion(skillIconRect, string.Format("SidearmPreference_Skill".Translate()));
                MouseoverSounds.DoRegion(skillIconRect, SoundDefOf.Mouseover_Command);
            }
            else
            {
                if (pawn.skills != null)
                {
                    if (pawnMemory.primaryWeaponMode == GoldfishModule.PrimaryWeaponMode.BySkill)
                        GUI.color = preferenceSet;
                    else
                        GUI.color = preferenceBase;
                    GUI.DrawTexture(skillIconRect, TextureResources.preferSkilled);
                }
            }

            if (Mouse.IsOver(meleeIconRect))
            {
                if (pawnMemory.primaryWeaponMode == GoldfishModule.PrimaryWeaponMode.Melee)
                    GUI.color = preferenceHighlightSet;
                else
                    GUI.color = preferenceHighlight;
                GUI.DrawTexture(meleeIconRect, TextureResources.preferMelee);
                TooltipHandler.TipRegion(meleeIconRect, string.Format("SidearmPreference_Melee".Translate()));
                MouseoverSounds.DoRegion(meleeIconRect, SoundDefOf.Mouseover_Command);
            }
            else
            {
                if (pawnMemory.primaryWeaponMode == GoldfishModule.PrimaryWeaponMode.Melee)
                    GUI.color = preferenceSet;
                else if (pawnMemory.primaryWeaponMode == GoldfishModule.PrimaryWeaponMode.BySkill && skillPref == GoldfishModule.PrimaryWeaponMode.Melee)
                    GUI.color = preferenceOfSkill;
                else
                    GUI.color = preferenceBase;
                GUI.DrawTexture(meleeIconRect, TextureResources.preferMelee);
            }

            UIHighlighter.HighlightOpportunity(rangedIconRect, "SidearmPreferenceButton");
            UIHighlighter.HighlightOpportunity(skillIconRect, "SidearmPreferenceButton");
            UIHighlighter.HighlightOpportunity(meleeIconRect, "SidearmPreferenceButton");

            if (Widgets.ButtonInvisible(rangedIconRect, true))
            {
                interactedWith = SidearmsListInteraction.SelectorRanged;
            }
            if (Widgets.ButtonInvisible(skillIconRect, true))
            {
                interactedWith = SidearmsListInteraction.SelectorSkill;
            }
            if (Widgets.ButtonInvisible(meleeIconRect, true))
            {
                interactedWith = SidearmsListInteraction.SelectorMelee;
            }
        }

        public void DrawIconForWeaponMemory(Pawn pawn, GoldfishModule pawnMemory, ThingStuffPair weaponType, bool isDuplicate, Rect contentRect, Vector2 iconOffset)
        {
            Graphic g = weaponType.thing.graphicData.Graphic;

            var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, IconSize, IconSize);

            Texture2D drawPocket;
            drawPocket = TextureResources.drawPocketMemory;

            if (pawn.Drafted)
            {
                TooltipHandler.TipRegion(iconRect, string.Format("DrawSidearm_gizmoTooltipMemoryWhileDrafted".Translate(), weaponType.getLabel()));
            }
            else
            {
                TooltipHandler.TipRegion(iconRect, string.Format("DrawSidearm_gizmoTooltipMemory".Translate(), weaponType.getLabel()));
            }
            MouseoverSounds.DoRegion(iconRect, SoundDefOf.Mouseover_Command);
            if (Mouse.IsOver(iconRect))
            {
                LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SidearmsMissing, OpportunityType.GoodToKnow);

                GUI.color = iconMouseOverColor;
                GUI.DrawTexture(iconRect, drawPocket);
            }
            else
            {
                GUI.color = iconBaseColor;
                GUI.DrawTexture(iconRect, drawPocket);
            }

            Texture resolvedIcon = weaponType.thing.uiIcon;

            GUI.color = weaponType.getDrawColor();
            GUI.DrawTexture(iconRect, resolvedIcon);
            GUI.color = Color.white;

            if (!isDuplicate)
            {
                GUI.color = Color.white;

                if (pawnMemory.ForcedWeapon == weaponType)
                    GUI.DrawTexture(iconRect, TextureResources.forcedAlways);
                
                if(weaponType.thing.IsRangedWeapon & pawnMemory.DefaultRangedWeapon == weaponType)
                    GUI.DrawTexture(iconRect, TextureResources.defaultRanged);
                else if (pawnMemory.PreferredMeleeWeapon == weaponType)
                    GUI.DrawTexture(iconRect, TextureResources.preferredMelee);

                GUI.color = Color.white;
            }

            UIHighlighter.HighlightOpportunity(iconRect, "SidearmMissing");

            if (Widgets.ButtonInvisible(iconRect, true))
            {
                interactedWith = SidearmsListInteraction.WeaponMemory;
                interactionWeaponType = weaponType;
                interactionWeaponIsDuplicate = isDuplicate;
            }
        }

        public void DrawIconForWeapon(Pawn pawn, GoldfishModule pawnMemory, ThingWithComps weapon, bool isDuplicate, Rect contentRect, Vector2 iconOffset)
        {
            if (weapon is null || weapon.def is null || weapon.def.uiIcon is null)
                return;

            ThingStuffPair weaponType = weapon.toThingStuffPair();

            var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, IconSize, IconSize);
            //var iconColor = iconBaseColor;

            string hoverText;
            if (pawn.Drafted)
            {
                if (pawnMemory.ForcedWeaponWhileDrafted == weapon.toThingStuffPair())
                    hoverText = "DrawSidearm_gizmoTooltipForcedWhileDrafted";
                else
                    hoverText = "DrawSidearm_gizmoTooltipWhileDrafted";
            }
            else
            {
                if (pawnMemory.ForcedWeapon == weapon.toThingStuffPair())
                    hoverText = "DrawSidearm_gizmoTooltipForced";
                else 
                {
                    if (weapon.def.IsRangedWeapon)
                    {
                        if (pawnMemory.DefaultRangedWeapon == weaponType)
                            hoverText = "DrawSidearm_gizmoTooltipRangedDefault";
                        else
                            hoverText = "DrawSidearm_gizmoTooltipRanged";
                    }
                    else 
                    {
                        if (pawnMemory.PreferredMeleeWeapon == weaponType)
                            hoverText = "DrawSidearm_gizmoTooltipMeleePreferred";
                        else
                            hoverText = "DrawSidearm_gizmoTooltipMelee";
                    }
                }
            }

            TooltipHandler.TipRegion(iconRect, string.Format(hoverText.Translate(), weapon.toThingStuffPair().getLabel()));
            MouseoverSounds.DoRegion(iconRect, SoundDefOf.Mouseover_Command);

            Texture2D drawPocket;
            if (pawnMemory.RememberedWeapons.Contains(weapon.toThingStuffPair()))
            {
                drawPocket = TextureResources.drawPocket;
            }
            else
            {
                drawPocket = TextureResources.drawPocketTemp;
            }

            if (Mouse.IsOver(iconRect))
            {
                LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SidearmsDropping, OpportunityType.GoodToKnow);

                if (pawn.Drafted)
                {
                    LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SimpleSidearmsAdvancedDrafted, OpportunityType.GoodToKnow);
                }
                else 
                {
                    if (weapon.def.IsRangedWeapon)
                        LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SimpleSidearmsAdvancedRanged, OpportunityType.GoodToKnow);
                    else
                        LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, OpportunityType.GoodToKnow);
                }

                GUI.color = iconMouseOverColor;
                GUI.DrawTexture(iconRect, drawPocket);
                //Graphics.DrawTexture(iconRect, TextureResources.drawPocket, new Rect(0, 0, 1f, 1f), 0, 0, 0, 0, iconMouseOverColor);
            }
            else
            {
                GUI.color = iconBaseColor;
                GUI.DrawTexture(iconRect, drawPocket);
                //Graphics.DrawTexture(iconRect, TextureResources.drawPocket, new Rect(0, 0, 1f, 1f), 0, 0, 0, 0, iconBaseColor);
            }

            Texture resolvedIcon;
            if (!weapon.def.uiIconPath.NullOrEmpty())
            {
                resolvedIcon = weapon.def.uiIcon;
            }
            else
            {
                resolvedIcon = weapon.Graphic.ExtractInnerGraphicFor(weapon).MatSingle.mainTexture;
            }
            GUI.color = weapon.DrawColor;
            GUI.DrawTexture(iconRect, resolvedIcon);
            GUI.color = Color.white;


            if (!isDuplicate)
            {
                GUI.color = Color.white;


                if (pawnMemory.ForcedWeaponWhileDrafted == weaponType)
                    GUI.DrawTexture(iconRect, TextureResources.forcedDrafted);

                if (pawnMemory.ForcedWeapon == weaponType)
                    GUI.DrawTexture(iconRect, TextureResources.forcedAlways);

                if (weaponType.thing.IsRangedWeapon & pawnMemory.DefaultRangedWeapon == weaponType)
                    GUI.DrawTexture(iconRect, TextureResources.defaultRanged);
                else if (pawnMemory.PreferredMeleeWeapon == weaponType)
                    GUI.DrawTexture(iconRect, TextureResources.preferredMelee);

                GUI.color = Color.white;
            }

            UIHighlighter.HighlightOpportunity(iconRect, "SidearmInInventory");
            if (weapon.def.IsRangedWeapon)
                UIHighlighter.HighlightOpportunity(iconRect, "SidearmInInventoryRanged");
            else
                UIHighlighter.HighlightOpportunity(iconRect, "SidearmInInventoryMelee");

            if (Widgets.ButtonInvisible(iconRect, true))
            {
                interactedWith = SidearmsListInteraction.Weapon;
                interactionWeapon = weapon;
                interactionWeaponIsDuplicate = isDuplicate;
            }
        }

        public void DrawIconForUnarmed(Pawn pawn, GoldfishModule pawnMemory, Rect contentRect, Vector2 iconOffset)
        {
            var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, IconSize, IconSize);
            //var iconColor = iconBaseColor;

            string hoverText;
            if (pawn.Drafted)
            {
                if (pawnMemory.ForcedUnarmedWhileDrafted)
                    hoverText = "DrawSidearm_gizmoTooltipUnarmedForcedWhileDrafted";
                else
                    hoverText = "DrawSidearm_gizmoTooltipUnarmedWhileDrafted";
            }
            else
            {
                if (pawnMemory.ForcedUnarmed)
                    hoverText = "DrawSidearm_gizmoTooltipUnarmedForced";
                else if (pawnMemory.PreferredUnarmed)
                    hoverText = "DrawSidearm_gizmoTooltipUnarmedPreferred";
                else
                    hoverText = "DrawSidearm_gizmoTooltipUnarmed";

            }

            TooltipHandler.TipRegion(iconRect, hoverText.Translate());
            MouseoverSounds.DoRegion(iconRect, SoundDefOf.Mouseover_Command);

            Texture2D drawPocket = TextureResources.drawPocket;

            if (Mouse.IsOver(iconRect))
            {
                GUI.color = iconMouseOverColor;
                GUI.DrawTexture(iconRect, drawPocket);
                //Graphics.DrawTexture(iconRect, TextureResources.drawPocket, new Rect(0, 0, 1f, 1f), 0, 0, 0, 0, iconMouseOverColor);
            }
            else
            {
                GUI.color = iconBaseColor;
                GUI.DrawTexture(iconRect, drawPocket);
                //Graphics.DrawTexture(iconRect, TextureResources.drawPocket, new Rect(0, 0, 1f, 1f), 0, 0, 0, 0, iconBaseColor);
            }

            Texture resolvedIcon = TexCommand.AttackMelee;
            GUI.color = Color.white;
            GUI.DrawTexture(iconRect, resolvedIcon);
            GUI.color = Color.white;

            GUI.color = Color.white; 
            
            if (pawnMemory.ForcedUnarmedWhileDrafted)
                GUI.DrawTexture(iconRect, TextureResources.forcedDrafted);
            
            if (pawnMemory.ForcedUnarmed)
                GUI.DrawTexture(iconRect, TextureResources.forcedAlways);
            
            if (pawnMemory.PreferredUnarmed)
                GUI.DrawTexture(iconRect, TextureResources.preferredMelee);
            else 
            GUI.color = Color.white;

            if (Widgets.ButtonInvisible(iconRect, true))
            {
                interactedWith = SidearmsListInteraction.Unarmed;
            }
        }

        public override void ProcessInput(Event ev)
        {
            if (activateSound != null)
            {
                activateSound.PlayOneShotOnCamera();
            }
            if (ev.button < 0)
            {
                if (hotkeyAction != null)
                    hotkeyAction();
            }
            else {
                handleInteraction(interactedWith, ev);
                //iconClickAction(ev.button);
            }

        }

        //Ive rewritten this twice now and its still an ugly monster.
        public const int LEFT_CLICK = 0;
        public const int RIGHT_CLICK = 1;
        public void handleInteraction(SidearmsListInteraction interaction, Event ev)
        {
            GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(parent);
            if (pawnMemory == null)
                return;

            var dropMode = parent.Drafted ? DroppingModeEnum.Combat : DroppingModeEnum.Calm;


            if (ev.button == LEFT_CLICK)
            {
                switch (interaction)
                {
                    case SidearmsListInteraction.SelectorRanged:
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsPreference, KnowledgeAmount.SpecificInteraction);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                        pawnMemory.primaryWeaponMode = GoldfishModule.PrimaryWeaponMode.Ranged;
                        break;
                    case SidearmsListInteraction.SelectorSkill:
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsPreference, KnowledgeAmount.SpecificInteraction);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                        pawnMemory.primaryWeaponMode = GoldfishModule.PrimaryWeaponMode.BySkill;
                        break;
                    case SidearmsListInteraction.SelectorMelee:
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsPreference, KnowledgeAmount.SpecificInteraction);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                        pawnMemory.primaryWeaponMode = GoldfishModule.PrimaryWeaponMode.Melee;
                        break;
                    case SidearmsListInteraction.Weapon:
                        Thing weapon = interactionWeapon;
                        ThingStuffPair weaponType = weapon.toThingStuffPair();
                        if (parent.Drafted)
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedDrafted, KnowledgeAmount.SpecificInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            pawnMemory.SetWeaponAsForced(weaponType, true);
                            if (parent.equipment.Primary != weapon && weapon is ThingWithComps)
                                WeaponAssingment.equipSpecificWeaponTypeFromInventory(parent, weaponType, MiscUtils.shouldDrop(dropMode), false);
                        }
                        else if (pawnMemory.DefaultRangedWeapon == weaponType || pawnMemory.PreferredMeleeWeapon == weaponType || weaponType.isToolNotWeapon())
                        {
                            if(weaponType.thing.IsRangedWeapon)
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedRanged, KnowledgeAmount.SpecificInteraction);
                            else
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            pawnMemory.SetWeaponAsForced(weaponType, false);
                            if (parent.equipment.Primary != weapon && weapon is ThingWithComps)
                                WeaponAssingment.equipSpecificWeaponTypeFromInventory(parent, weaponType, MiscUtils.shouldDrop(dropMode), false);
                        }
                        else
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);
                            if (weaponType.thing.IsRangedWeapon)
                            {
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedRanged, KnowledgeAmount.SpecificInteraction);
                                pawnMemory.SetRangedWeaponTypeAsDefault(weaponType);
                            }
                            else
                            {
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                                pawnMemory.SetMeleeWeaponTypeAsPreferred(weaponType);
                            }
                        }
                        break;
                    case SidearmsListInteraction.WeaponMemory:

                        ThingStuffPair weaponMemory = interactionWeaponType.Value;
                        if (parent.Drafted)
                        {
                            //allow nothing
                        }
                        else
                        {
                            if (weaponMemory.thing.IsRangedWeapon)
                            {
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedRanged, KnowledgeAmount.SpecificInteraction);
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                pawnMemory.SetRangedWeaponTypeAsDefault(weaponMemory);
                            }
                            else
                            {
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                pawnMemory.SetMeleeWeaponTypeAsPreferred(weaponMemory);
                            }
                        }
                        break;
                    case SidearmsListInteraction.Unarmed:
                        if (parent.Drafted)
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedDrafted, KnowledgeAmount.SpecificInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            pawnMemory.SetUnarmedAsForced(true);
                            if (parent.equipment.Primary != null)
                                WeaponAssingment.equipSpecificWeapon(parent, null, MiscUtils.shouldDrop(dropMode), false);
                        }
                        else if (pawnMemory.PreferredUnarmed)
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            pawnMemory.SetUnarmedAsForced(false);
                            if (parent.equipment.Primary != null)
                                WeaponAssingment.equipSpecificWeapon(parent, null, MiscUtils.shouldDrop(dropMode), false);
                        }
                        else 
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            pawnMemory.SetUnarmedAsPreferredMelee();
                        }
                        break;
                    case SidearmsListInteraction.None:
                    default:
                        return;
                        break;
                }
            }
            else if(ev.button == RIGHT_CLICK)
            {
                switch (interaction)
                {
                    case SidearmsListInteraction.SelectorRanged:
                    case SidearmsListInteraction.SelectorSkill:
                    case SidearmsListInteraction.SelectorMelee:
                        break;
                    case SidearmsListInteraction.Weapon:
                        Thing weapon = interactionWeapon;
                        ThingStuffPair weaponType = weapon.toThingStuffPair();

                        if (interactionWeaponIsDuplicate)
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SidearmsDropping, KnowledgeAmount.SpecificInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            WeaponAssingment.dropSidearm(parent, weapon, true);
                        }
                        else
                        {
                            if (parent.Drafted)
                            {
                                if (pawnMemory.ForcedWeaponWhileDrafted == weaponType)
                                {
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedDrafted, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    pawnMemory.UnsetForcedWeapon(true);
                                }
                            }
                            else
                            {
                                if (pawnMemory.ForcedWeapon == weaponType)
                                {
                                    if (weaponType.thing.IsRangedWeapon)
                                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedRanged, KnowledgeAmount.SpecificInteraction);
                                    else
                                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    pawnMemory.UnsetForcedWeapon(false);
                                }
                                else if (weaponType.thing.IsRangedWeapon & pawnMemory.DefaultRangedWeapon == weaponType)
                                {
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedRanged, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    pawnMemory.UnsetRangedWeaponDefault();
                                }
                                else if (pawnMemory.PreferredMeleeWeapon == weaponType)
                                {
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    pawnMemory.UnsetMeleeWeaponPreference();
                                }
                                else
                                {
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SidearmsDropping, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    WeaponAssingment.dropSidearm(parent, weapon, true);
                                }
                            }
                        }

                        break;
                    case SidearmsListInteraction.WeaponMemory:
                        ThingStuffPair weaponMemory = interactionWeaponType.Value;

                        if (interactionWeaponIsDuplicate)
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SidearmsMissing, KnowledgeAmount.SmallInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            pawnMemory.ForgetSidearmMemory(weaponMemory);
                        }
                        else
                        {
                            if (parent.Drafted)
                            {
                                if (pawnMemory.ForcedWeaponWhileDrafted == weaponMemory)
                                {
                                    if (weaponMemory.thing.IsRangedWeapon)
                                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedRanged, KnowledgeAmount.SpecificInteraction);
                                    else
                                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    pawnMemory.UnsetForcedWeapon(true);
                                }
                            }
                            else
                            {
                                if (pawnMemory.ForcedWeapon == weaponMemory)
                                {
                                    if (weaponMemory.thing.IsRangedWeapon)
                                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedRanged, KnowledgeAmount.SpecificInteraction);
                                    else
                                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    pawnMemory.UnsetForcedWeapon(false);
                                }
                                else if (weaponMemory.thing.IsRangedWeapon & pawnMemory.DefaultRangedWeapon == weaponMemory)
                                {
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedRanged, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    pawnMemory.UnsetRangedWeaponDefault();
                                }
                                else if (pawnMemory.PreferredMeleeWeapon == weaponMemory)
                                {
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    pawnMemory.UnsetMeleeWeaponPreference();
                                }
                                else
                                {
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SidearmsMissing, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    pawnMemory.ForgetSidearmMemory(weaponMemory);
                                }
                            }
                        }

                        break;
                    case SidearmsListInteraction.Unarmed:
                        if (parent.Drafted && pawnMemory.ForcedUnarmedWhileDrafted)
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedDrafted, KnowledgeAmount.SpecificInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            pawnMemory.UnsetUnarmedAsForced(true);
                        }
                        else if (pawnMemory.ForcedUnarmed)
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            pawnMemory.UnsetUnarmedAsForced(false);
                        }
                        else if (pawnMemory.PreferredUnarmed)
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            pawnMemory.UnsetMeleeWeaponPreference();
                        }
                        break;
                    case SidearmsListInteraction.None:
                    default:
                        return;
                        break;
                }
            }
        }


        public void DrawGizmoLabel(string labelText, Rect gizmoRect)
        {
            var labelHeight = Text.CalcHeight(labelText, gizmoRect.width);
            labelHeight -= 2f;
            var labelRect = new Rect(gizmoRect.x, gizmoRect.yMax - labelHeight + 12f, gizmoRect.width, labelHeight);
            GUI.DrawTexture(labelRect, TexUI.GrayTextBG);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(labelRect, labelText);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }


        public int countMissingMeleeWeapons(GoldfishModule pawnMemory, Pawn pawn)
        {
            if (pawnMemory == null)
                return 0;

            int count = 0;

            Dictionary<ThingStuffPair, int> dupeCounters = new Dictionary<ThingStuffPair, int>();
            foreach (ThingStuffPair weapon in pawnMemory.RememberedWeapons)
            {
                if (!dupeCounters.ContainsKey(weapon))
                    dupeCounters[weapon] = 0;

                if (!weapon.thing.IsMeleeWeapon)
                    continue;
                if (!pawn.hasWeaponSomewhere(weapon, dupeCounters[weapon]))
                    count++;

                dupeCounters[weapon]++;
            }
            return count;
        }

        public int countMissingRangedWeapons(GoldfishModule pawnMemory, Pawn pawn)
        {
            if (pawnMemory == null)
                return 0;

            int count = 0;

            Dictionary<ThingStuffPair, int> dupeCounters = new Dictionary<ThingStuffPair, int>();
            foreach (ThingStuffPair weapon in pawnMemory.RememberedWeapons)
            {
                if (!dupeCounters.ContainsKey(weapon))
                    dupeCounters[weapon] = 0;

                if (!weapon.thing.IsRangedWeapon)
                    continue;
                if (!pawn.hasWeaponSomewhere(weapon, dupeCounters[weapon]))
                    count++;

                dupeCounters[weapon]++;
            }
            return count;
        }

    }
}
