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

        public override float Width
        {
            get {
                int biggerCount = Math.Max(rangedWeapons.Count, meleeWeapons.Count);
                if (biggerCount < 2)
                    return MinGizmoSize + LockPanelWidth;
                else
                    return ContentPadding * 2 + (IconSize * biggerCount) + IconGap * (biggerCount - 1) + LockPanelWidth;
            }
        }

        //public Texture2D[] iconTextures;
        public Action hotkeyAction;

        private Pawn parent;
        private List<Thing> rangedWeapons;
        private List<Thing> meleeWeapons;

        public Gizmo_SidearmsList(Pawn parent, List<Thing> rangedWeapons, List<Thing> meleeWeapons)
        {
            this.parent = parent;
            this.rangedWeapons = rangedWeapons;
            this.meleeWeapons = meleeWeapons; 
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
            MouseoverSounds.DoRegion(rect, SoundDefOf.MouseoverCommand);

            if (Mouse.IsOver(rect))
                GUI.color = iconMouseOverColor;
            else
                GUI.color = iconBaseColor;

            GUI.DrawTexture(rect, lockTex);
            GUI.color = Color.white;

            if (Widgets.ButtonInvisible(rect, true))
            {
                handler.currentWeaponLocked = !handler.currentWeaponLocked;
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
            MouseoverSounds.DoRegion(rect, SoundDefOf.MouseoverCommand);

            if (Mouse.IsOver(rect))
                GUI.color = iconMouseOverColor;
            else
                GUI.color = iconBaseColor;

            GUI.DrawTexture(rect, lockTex);
            GUI.color = Color.white;

            if (Widgets.ButtonInvisible(rect, true))
            {
                handler.autoLockOnManualSwap = !handler.autoLockOnManualSwap;
                return true;
            }
            else
                return false;
        }

        private bool DrawIconForWeapon(Thing weapon, Rect contentRect, Vector2 iconOffset, int buttonID)
        {
            var iconTex = weapon.def.uiIcon;
            Color color = weapon.DrawColor;

            var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, IconSize, IconSize);
            //var iconColor = iconBaseColor;

            TooltipHandler.TipRegion(iconRect, string.Format(defaultDesc, weapon.LabelShort));
            MouseoverSounds.DoRegion(iconRect, SoundDefOf.MouseoverCommand);
            if (Mouse.IsOver(iconRect))
            { 
                GUI.color = iconMouseOverColor;
                GUI.DrawTexture(iconRect, TextureResources.drawPocket);
                //Graphics.DrawTexture(iconRect, TextureResources.drawPocket, new Rect(0, 0, 1f, 1f), 0, 0, 0, 0, iconMouseOverColor);
            }
            else
            {
                GUI.color = iconBaseColor;
                GUI.DrawTexture(iconRect, TextureResources.drawPocket);
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

            if (Widgets.ButtonInvisible(iconRect, true))
            {
                Event.current.button = buttonID;
                return true;
            }
            else
                return false;
        }

        

        public override GizmoResult GizmoOnGUI(Vector2 topLeft)
        {
            var gizmoRect = new Rect(topLeft.x, topLeft.y, Width, MinGizmoSize);
            var contentRect = gizmoRect.ContractedBy(ContentPadding);
            Widgets.DrawWindowBackground(gizmoRect);
            var interacted = false;
            int buttonID = 0;

            for (int i = 0; i < rangedWeapons.Count; i++)
            {
                var iconOffset = new Vector2((IconSize * i) + IconGap * (i - 1) + LockPanelWidth, 0);
                interacted |= DrawIconForWeapon(rangedWeapons[i],contentRect, iconOffset, buttonID);
                buttonID++;
            }

            for (int i = 0; i < meleeWeapons.Count; i++)
            {
                var iconOffset = new Vector2((IconSize * i) + IconGap * (i - 1) + LockPanelWidth, IconSize + IconGap);
                interacted |= DrawIconForWeapon(meleeWeapons[i], contentRect, iconOffset, buttonID);
                buttonID++;
            }

            Rect locksPanel = new Rect(gizmoRect.x + ContentPadding, gizmoRect.y, LockPanelWidth - ContentPadding, MinGizmoSize);
            //locksPanel = locksPanel.ContractedBy(LockPanelPadding);

            SwapControlsHandler handler = SwapControlsHandler.GetHandlerForPawn(parent);

            Rect lockPanel = new Rect(locksPanel.x, locksPanel.y + (locksPanel.height / 2f) -locksPanel.width - LockIconsOffset, locksPanel.width, locksPanel.width);
            Rect locklockPanel = new Rect(locksPanel.x, locksPanel.y + (locksPanel.height / 2f) + LockIconsOffset, locksPanel.width, locksPanel.width);

            DrawLock(handler, lockPanel);
            DrawLocklock(handler, locklockPanel);

            DrawGizmoLabel(defaultLabel, gizmoRect);
            return interacted ? new GizmoResult(GizmoState.Interacted, Event.current) : new GizmoResult(GizmoState.Clear);
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
            Thing toSwapTo;

            if (buttonID >= rangedWeapons.Count)
            {
                toSwapTo = meleeWeapons[buttonID - rangedWeapons.Count];
                WeaponAssingment.weaponSwapSpecific(parent, toSwapTo, MiscUtils.shouldDrop(DroppingModeEnum.UserForced));
            }
            else
            {
                toSwapTo = rangedWeapons[buttonID];
                WeaponAssingment.weaponSwapSpecific(parent, toSwapTo, MiscUtils.shouldDrop(DroppingModeEnum.UserForced));
                SwapControlsHandler handler = SwapControlsHandler.GetHandlerForPawn(parent);
                if (handler.autoLockOnManualSwap)
                    handler.currentWeaponLocked = true;
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
