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
    class Gizmo_SidearmsList : Command
    {
        private const float ContentPadding = 5f;
        private const float MinGizmoSize = 75f;
        private const float IconSize = 32f;
        private const float IconGap = 1f;
        private const float LockPanelWidth = 24f + ContentPadding;
        private const float LockIconsOffset = 6f;
        
        private static readonly Color iconBaseColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color iconMouseOverColor = new Color(0.6f, 0.6f, 0.4f, 1f);

        public override float GetWidth(float maxWidth)
        {
            GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(parent);
            //if (pawnMemory == null)
            //    return 75;
            int biggerCount = Math.Max(rangedWeapons.Count + calcUnmatchedRangedWeapons(pawnMemory, parent), meleeWeapons.Count + calcUnmatchedMeleeWeapons(pawnMemory, parent) + 1);
            float width = ContentPadding * 2 + (IconSize * biggerCount) + IconGap * (biggerCount - 1) + LockPanelWidth;
            return Math.Min(width, maxWidth);
        }
        
        private int calcUnmatchedMeleeWeapons(GoldfishModule pawnMemory, Pawn pawn)
        {
            if (pawnMemory == null)
                return 0;

            int count = 0;

            foreach (string weapon in pawnMemory.weapons)
            {
                ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(weapon);
                if (def == null)
                    continue;
                if (!def.IsMeleeWeapon)
                    continue;
                if (!pawn.hasWeaponSomewhere(def))
                    count++;
            }
            return count;
        }

        private int calcUnmatchedRangedWeapons(GoldfishModule pawnMemory, Pawn pawn)
        {
            if (pawnMemory == null)
                return 0;

            int count = 0;    

            foreach (string weapon in pawnMemory.weapons)
            {
                ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(weapon);
                if (def == null)
                    continue;
                if (!def.IsRangedWeapon)
                    continue;
                if (!pawn.hasWeaponSomewhere(def))
                    count++;
            }
            return count;
        }

        //public Texture2D[] iconTextures;
        public Action hotkeyAction;

        private Pawn parent;
        private List<Thing> rangedWeapons;
        private List<Thing> meleeWeapons;

        private List<ThingDef> rangedWeaponMemories;
        private List<ThingDef> meleeWeaponMemories;

        private Thing interactedWeapon = null;
        private ThingDef interactedWeaponMemory = null;
        private bool interactedRanged = false;
        private bool interactedUnarmed = false;

        public Gizmo_SidearmsList(Pawn parent, List<Thing> rangedWeapons, List<Thing> meleeWeapons, List<ThingDef> rangedWeaponMemories, List<ThingDef> meleeWeaponMemories)
        {
            this.parent = parent;
            this.rangedWeapons = rangedWeapons;
            this.meleeWeapons = meleeWeapons;
            this.rangedWeaponMemories = rangedWeaponMemories;
            this.meleeWeaponMemories = meleeWeaponMemories;
            tutorTag = "SidearmsList";
        }

        private bool DrawLock(SwapControlsHandler handler, Rect rect)
        {
            Texture2D lockTex;
            if (handler.currentWeaponLocked)
            {
                lockTex = TextureResources.lockClosed;
                TooltipHandler.TipRegion(rect, "LockedWeaponSwitch".Translate());
            }
            else
            {
                lockTex = TextureResources.lockOpen;
                TooltipHandler.TipRegion(rect, "UnlockedWeaponSwitch".Translate());
            }           
            MouseoverSounds.DoRegion(rect, SoundDefOf.Mouseover_Command);

            if (Mouse.IsOver(rect))
            {
                LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SimpleSidearmsAdvanced, OpportunityType.GoodToKnow);
                GUI.color = iconMouseOverColor;
            }
            else
                GUI.color = iconBaseColor;

            GUI.DrawTexture(rect, lockTex);
            GUI.color = Color.white;

            if (Widgets.ButtonInvisible(rect, true))
            {
                handler.currentWeaponLocked = !handler.currentWeaponLocked;
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvanced, KnowledgeAmount.SpecificInteraction);
                return true;
            }
            else
                return false;
        }

        private bool DrawLocklock(SwapControlsHandler handler, Rect rect)
        {
            Texture2D lockTex;
            if (handler.autoLockOnManualSwap)
            {
                lockTex = TextureResources.autolockOn;
                TooltipHandler.TipRegion(rect, "WeaponSwitchAutolockOn".Translate());
            }
            else
            {
                lockTex = TextureResources.autolockOff;
                TooltipHandler.TipRegion(rect, "WeaponSwitchAutolockOff".Translate()); 
            }
            MouseoverSounds.DoRegion(rect, SoundDefOf.Mouseover_Command);

            if (Mouse.IsOver(rect))
            {
                LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SimpleSidearmsAdvanced, OpportunityType.GoodToKnow);
                GUI.color = iconMouseOverColor;
            }
            else
                GUI.color = iconBaseColor;

            GUI.DrawTexture(rect, lockTex);
            GUI.color = Color.white;

            if (Widgets.ButtonInvisible(rect, true))
            {
                handler.autoLockOnManualSwap = !handler.autoLockOnManualSwap;
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvanced, KnowledgeAmount.SpecificInteraction);
                return true; 
            }
            else
                return false;  
        }

        private static bool DrawIconForWeaponMemory(GoldfishModule pawnMemory, ThingDef weapon, Rect contentRect, Vector2 iconOffset)
        {
            var iconTex = weapon.uiIcon;
            Graphic g = weapon.graphicData.Graphic;
            Color color = getColor(weapon);
            Color colorTwo = getColor(weapon);
            Graphic g2 = weapon.graphicData.Graphic.GetColoredVersion(g.Shader, color, colorTwo);

            var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, IconSize, IconSize);
            
            string label = weapon.label;
            
            Texture2D drawPocket;
            if (pawnMemory.IsCurrentPrimary(weapon.defName))
                drawPocket = TextureResources.drawPocketMemoryPrimary;
            else
                drawPocket = TextureResources.drawPocketMemory;

            TooltipHandler.TipRegion(iconRect, string.Format("DrawSidearm_gizmoTooltipMemory".Translate(), weapon.label));
            MouseoverSounds.DoRegion(iconRect, SoundDefOf.Mouseover_Command);
            if (Mouse.IsOver(iconRect))
            {
                LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SidearmsMissing, OpportunityType.GoodToKnow);
                if (pawnMemory.IsCurrentPrimary(weapon.defName))
                    LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SidearmsPrimary, OpportunityType.GoodToKnow);
                GUI.color = iconMouseOverColor;
                GUI.DrawTexture(iconRect, drawPocket);
            }
            else
            {
                GUI.color = iconBaseColor;
                GUI.DrawTexture(iconRect, drawPocket);
            }

            Texture resolvedIcon;
            if (!weapon.uiIconPath.NullOrEmpty())
            {
                resolvedIcon = weapon.uiIcon;
            }
            else
            {
                resolvedIcon = g2.MatSingle.mainTexture;
            }
            GUI.color = color;
            GUI.DrawTexture(iconRect, resolvedIcon);
            GUI.color = Color.white;

            UIHighlighter.HighlightOpportunity(iconRect, "SidearmMissing");
            if (pawnMemory.IsCurrentPrimary(weapon.defName))
                UIHighlighter.HighlightOpportunity(iconRect, "SidearmPrimary");
            
            if (Widgets.ButtonInvisible(iconRect, true))
            {
                return true;
            }
            else
                return false;
        }

        private static Color getColor(ThingDef weapon)
        {
            if (weapon.graphicData != null)
            {
                return weapon.graphicData.color;
            }
            return Color.white;
        }

        private static Color getColorTwo(ThingDef weapon)
        {
            if (weapon.graphicData != null)
            {
                return weapon.graphicData.colorTwo;
            }
            return Color.white;
        }

        private bool DrawIconForWeapon(GoldfishModule pawnMemory, Thing weapon, Rect contentRect, Vector2 iconOffset)
        {
            var iconTex = weapon.def.uiIcon;
            Color color = weapon.DrawColor;

            var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, IconSize, IconSize);
            //var iconColor = iconBaseColor;

            TooltipHandler.TipRegion(iconRect, string.Format("DrawSidearm_gizmoTooltip".Translate(), weapon.LabelShort));
            MouseoverSounds.DoRegion(iconRect, SoundDefOf.Mouseover_Command);

            Texture2D drawPocket = TextureResources.drawPocketTemp;
            if (pawnMemory != null && pawnMemory.IsCurrentPrimary(weapon.def.defName))
                drawPocket = TextureResources.drawPocketPrimary;
            else
            {
                if (pawnMemory != null)
                {
                    foreach(string weaponName in pawnMemory.weapons)
                    {
                        if (weaponName.Equals(weapon.def.defName))
                        {
                            drawPocket = TextureResources.drawPocket;
                            break;
                        }
                    }
                }
            }

            if (Mouse.IsOver(iconRect))
            {
                LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SidearmsInInventory, OpportunityType.GoodToKnow);
                if (pawnMemory != null && pawnMemory.IsCurrentPrimary(weapon.def.defName))
                    LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SidearmsPrimary, OpportunityType.GoodToKnow);
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

            UIHighlighter.HighlightOpportunity(iconRect, "SidearmInInventory");
            if (pawnMemory != null && pawnMemory.IsCurrentPrimary(weapon.def.defName))
                UIHighlighter.HighlightOpportunity(iconRect, "SidearmPrimary");

            if (Widgets.ButtonInvisible(iconRect, true))
            {
                return true;
            }
            else
                return false;
        }

        private bool DrawIconForUnarmed(Pawn pawn, Rect contentRect, Vector2 iconOffset)
        {
            var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, IconSize, IconSize);
            //var iconColor = iconBaseColor;

            TooltipHandler.TipRegion(iconRect, "DrawSidearm_gizmoTooltipUnarmed".Translate());
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

            Texture resolvedIcon = TextureResources.unarmedIcon;
            GUI.color = Color.white;
            GUI.DrawTexture(iconRect, resolvedIcon);
            GUI.color = Color.white;

            if (Widgets.ButtonInvisible(iconRect, true))
            {
                return true;
            }
            else
                return false;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            var gizmoRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), MinGizmoSize);

            if (Mouse.IsOver(gizmoRect))
            {
                LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SimpleSidearmsBasic, OpportunityType.Important);
            }

            var contentRect = gizmoRect.ContractedBy(ContentPadding);
            Widgets.DrawWindowBackground(gizmoRect);

            var globalInteracted = false;
            interactedWeapon = null;
            interactedWeaponMemory = null;
            interactedRanged = false;
            interactedUnarmed = false;

            GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(parent);
            //if (pawnMemory == null)
            //    return new GizmoResult(GizmoState.Clear);

            int i = 0;
            for (i = 0; i < rangedWeapons.Count; i++)
            {
                var iconOffset = new Vector2((IconSize * i) + IconGap * (i - 1) + LockPanelWidth, 0);
                bool interacted = DrawIconForWeapon(pawnMemory, rangedWeapons[i], contentRect, iconOffset);
                if (interacted) interactedWeapon = rangedWeapons[i];
                if (interacted) interactedRanged = true;
                globalInteracted |= interacted;
            }
             
            int j = 0;

            if (pawnMemory != null)
            {
                foreach (ThingDef def in rangedWeaponMemories)
                {
                    if (!parent.hasWeaponSomewhere(def))
                    {
                        var iconOffset = new Vector2((IconSize * (i + j)) + IconGap * ((i + j) - 1) + LockPanelWidth, 0);
                        bool interacted = DrawIconForWeaponMemory(pawnMemory, def, contentRect, iconOffset);
                        if (interacted) interactedWeaponMemory = def;
                        if (interacted) interactedRanged = true;
                        globalInteracted |= interacted;
                        j++;
                    }
                }
            }

            for (i = 0; i < meleeWeapons.Count; i++)
            {
                var iconOffset = new Vector2((IconSize * i) + IconGap * (i - 1) + LockPanelWidth, IconSize + IconGap);
                bool interacted = DrawIconForWeapon(pawnMemory, meleeWeapons[i], contentRect, iconOffset);
                if (interacted) interactedWeapon = meleeWeapons[i];
                if (interacted) interactedRanged = false;
                globalInteracted |= interacted;
            }

            j = 0;
            if (pawnMemory != null)
            {
                foreach (ThingDef def in meleeWeaponMemories)
                {
                    if (!parent.hasWeaponSomewhere(def))
                    {
                        var iconOffset = new Vector2((IconSize * (i + j)) + IconGap * ((i + j) - 1) + LockPanelWidth, IconSize + IconGap);
                        bool interacted = DrawIconForWeaponMemory(pawnMemory, def, contentRect, iconOffset);
                        if (interacted) interactedWeaponMemory = def;
                        if (interacted) interactedRanged = false;
                        globalInteracted |= interacted;
                        j++;
                    }
                }
            }

            var unarmedIconOffset = new Vector2((IconSize * (i + j)) + IconGap * ((i + j) - 1) + LockPanelWidth, IconSize + IconGap);
            interactedUnarmed = DrawIconForUnarmed(parent, contentRect, unarmedIconOffset);
            globalInteracted |= interactedUnarmed;

            Rect locksPanel = new Rect(gizmoRect.x + ContentPadding, gizmoRect.y, LockPanelWidth - ContentPadding, MinGizmoSize);
            //locksPanel = locksPanel.ContractedBy(LockPanelPadding);

            SwapControlsHandler handler = SwapControlsHandler.GetHandlerForPawn(parent);

            Rect lockPanel = new Rect(locksPanel.x, locksPanel.y + (locksPanel.height / 2f) -locksPanel.width - LockIconsOffset, locksPanel.width, locksPanel.width);
            Rect locklockPanel = new Rect(locksPanel.x, locksPanel.y + (locksPanel.height / 2f) + LockIconsOffset, locksPanel.width, locksPanel.width);

            DrawLock(handler, lockPanel);
            UIHighlighter.HighlightOpportunity(lockPanel, "SidearmListButton");
            DrawLocklock(handler, locklockPanel);
            UIHighlighter.HighlightOpportunity(locklockPanel, "SidearmListButton");

            UIHighlighter.HighlightOpportunity(gizmoRect, "SidearmList");

            DrawGizmoLabel(defaultLabel, gizmoRect);
            return globalInteracted ? new GizmoResult(GizmoState.Interacted, Event.current) : new GizmoResult(GizmoState.Clear);
        }

        public override void ProcessInput(Event ev)
        {
            if (activateSound != null)
            {
                activateSound.PlayOneShotOnCamera();
            }
            if (ev.button < 0)
            {
                if (hotkeyAction != null) hotkeyAction();
            }
            else {
                iconClickAction(ev.button);
            }

        }

        private void iconClickAction(int buttonID)
        {
            if(interactedWeapon != null)
            {
                Thing toSwapTo;
                if (interactedRanged)
                {
                    if(buttonID == 0)
                    {
                        toSwapTo = interactedWeapon;

                        if (GoldfishModule.GetGoldfishForPawn(parent) != null && toSwapTo.def.defName.Equals(GoldfishModule.GetGoldfishForPawn(parent).primary))
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SidearmsPrimary, KnowledgeAmount.Total);

                        WeaponAssingment.weaponSwapSpecific(parent, toSwapTo, true, MiscUtils.shouldDrop(DroppingModeEnum.UserForced), false);
                        SwapControlsHandler handler = SwapControlsHandler.GetHandlerForPawn(parent);
                        if (handler.autoLockOnManualSwap)
                            handler.currentWeaponLocked = true;

                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SidearmsInInventory, KnowledgeAmount.SpecificInteraction);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);
                    }
                    else if(buttonID == 1)
                    {
                        WeaponAssingment.dropSidearm(parent, interactedWeapon);

                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SidearmsInInventory, KnowledgeAmount.SpecificInteraction);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);
                    }

                    
                }
                else
                {
                    if (buttonID == 0)
                    {
                        toSwapTo = interactedWeapon;
                        
                        if (GoldfishModule.GetGoldfishForPawn(parent) != null && toSwapTo.def.defName.Equals(GoldfishModule.GetGoldfishForPawn(parent).primary))
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SidearmsPrimary, KnowledgeAmount.Total);

                        WeaponAssingment.weaponSwapSpecific(parent, toSwapTo, true, MiscUtils.shouldDrop(DroppingModeEnum.UserForced), false);

                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SidearmsInInventory, KnowledgeAmount.SpecificInteraction);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);
                    }
                    else if (buttonID == 1)
                    {
                        WeaponAssingment.dropSidearm(parent, interactedWeapon);

                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SidearmsInInventory, KnowledgeAmount.SpecificInteraction);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);
                    }
                }
            }
            else if(interactedWeaponMemory != null)
            {
                if (interactedRanged)
                {
                    if (buttonID == 0)
                    {

                    }
                    else if (buttonID == 1)
                    {
                        WeaponAssingment.forgetSidearmMemory(parent, interactedWeaponMemory);

                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SidearmsMissing, KnowledgeAmount.SpecificInteraction);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);
                    }
                }
                else
                {
                    if (buttonID == 0)
                    {

                    }
                    else if (buttonID == 1)
                    {
                        WeaponAssingment.forgetSidearmMemory(parent, interactedWeaponMemory);

                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SidearmsMissing, KnowledgeAmount.SpecificInteraction);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);
                    }
                }
            }
            else if(interactedUnarmed == true)
            {
                if (buttonID == 0)
                {
                    WeaponAssingment.weaponSwapSpecific(parent, null, true, MiscUtils.shouldDrop(DroppingModeEnum.UserForced), false);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);
                }
            }
        }

        /*
        private void DrawHotKeyLabel(Rect gizmoRect)
        {
            var labelRect = new Rect(gizmoRect.x + ContentPadding, gizmoRect.y + ContentPadding, gizmoRect.width - 10f, 18f);
            var keyCode = hotKey.MainKey;
            Widgets.Label(labelRect, keyCode.ToStringReadable());
            GizmoGridDrawer.drawnHotKeys.Add(keyCode);
        }*/

        private void DrawGizmoLabel(string labelText, Rect gizmoRect)
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

    }
}
