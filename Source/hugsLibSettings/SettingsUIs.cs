using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HugsLib.Settings;
using UnityEngine;
using RimWorld;
using SimpleSidearms.rimworld;
using SimpleSidearms.utilities;
using static SimpleSidearms.Globals;
using Verse.Sound;
using Verse;

namespace SimpleSidearms.hugsLibSettings
{
    internal static class SettingsUIs
    {
        internal enum WeaponListKind { Both, Melee, Ranged }

        private const float ContentPadding = 5f;
        private const float IconSize = 32f;
        private const float IconGap = 1f;
        private const float TextMargin = 20f;
        private const float BottomMargin = 2f;

        private static readonly Color iconBaseColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color iconMouseOverColor = new Color(0.6f, 0.6f, 0.4f, 1f);

        private static readonly Color SelectedOptionColor = new Color(0.5f, 1f, 0.5f, 1f);
        private static readonly Color constGrey = new Color(0.8f, 0.8f, 0.8f, 1f);

        private static float averageCarryCapacity = 35f;

        internal enum ExpansionMode { None, Vertical, Horizontal};
        
        //Copied these here since the default ones seem to not allow changing through code (probably for a sensible reason of some description).

        private static void drawBackground(Rect rect, Color background)
        {
            Color save = GUI.color;
            GUI.color = background;
            GUI.DrawTexture(rect, TexUI.FastFillTex);
            GUI.color = save;
        }

        public static bool HugsDrawerRebuild_FloatEntry(SettingHandle <float> handle, Rect controlRect, Color background)
        {
            drawBackground(controlRect, background);
            string valText = Widgets.TextField(controlRect, handle.Value.ToString());
            if (handle.Validator == null || handle.Validator.Invoke(valText))
            {
                handle.StringValue = valText;
                return true;
            }
            else
            {
                DrawBadTextValueOutline(controlRect);
                return false;
            }
        }

        public static bool HugsDrawerRebuild_IntEntry(SettingHandle<int> handle, Rect controlRect)
        {
            string valText = Widgets.TextField(controlRect, handle.Value.ToString());
            if (handle.Validator == null || handle.Validator.Invoke(valText))
            {
                handle.StringValue = valText;
                return true;
            }
            else
            {
                DrawBadTextValueOutline(controlRect);
                return false;
            }
        }

        private static readonly Color BadValueOutlineColor = new Color(.9f, .1f, .1f, 1f);

        private static void DrawBadTextValueOutline(Rect rect)
        {
            var prevColor = GUI.color;
            GUI.color = BadValueOutlineColor;
            Widgets.DrawBox(rect);
            GUI.color = prevColor;
        }

        public static bool HugsDrawerRebuild_Spinner(SettingHandle <int> handle, Rect controlRect, Color background)
        {
            drawBackground(controlRect, background);
            var buttonSize = controlRect.height;
            var leftButtonRect = new Rect(controlRect.x, controlRect.y, buttonSize, buttonSize);
            var rightButtonRect = new Rect(controlRect.x + controlRect.width - buttonSize, controlRect.y, buttonSize, buttonSize);
            var changed = false;
            if (Widgets.ButtonText(leftButtonRect, "-"))
            {
                handle.Value -= handle.SpinnerIncrement;
                changed = true;
            }
            if (Widgets.ButtonText(rightButtonRect, "+"))
            {
                handle.Value += handle.SpinnerIncrement;
                changed = true;
            }
            var textRect = new Rect(controlRect.x + buttonSize + 1, controlRect.y, controlRect.width - buttonSize * 2 - 2f, controlRect.height);
            if (HugsDrawerRebuild_IntEntry(handle, textRect))
            {
                changed = true;
            }
            return changed;
        }
        
        public static bool HugsDrawerRebuild_Checkbox(SettingHandle <bool> handle, Rect controlRect, Color background)
        {
            drawBackground(controlRect, background);
            const float defaultCheckboxHeight = 24f;
            var checkOn = handle.Value;
            Widgets.Checkbox(controlRect.x, controlRect.y + (controlRect.height - defaultCheckboxHeight) / 2, ref checkOn);
            if (checkOn != handle.Value)
            {
                handle.Value = checkOn;
                return true;
            }
            return false;
        }

        public static bool CustomDrawer_FloatSlider(Rect rect, SettingHandle<float> setting, bool def_isPercentage, float def_min, float def_max, Color background)
        {
            drawBackground(rect, background);

            Rect sliderPortion = new Rect(rect);
            sliderPortion.width = sliderPortion.width - 50;
            Rect labelPortion = new Rect(rect);
            labelPortion.width = 50;
            labelPortion.position = new Vector2(sliderPortion.position.x + sliderPortion.width + 5f, sliderPortion.position.y + 4f);

            sliderPortion = sliderPortion.ContractedBy(2f);

            if (def_isPercentage)
                Widgets.Label(labelPortion, (Mathf.Round(setting.Value * 100f)).ToString("F0") + "%");
            else
                Widgets.Label(labelPortion, setting.Value.ToString("F2"));

            float val = Widgets.HorizontalSlider(sliderPortion, setting.Value, def_min, def_max, true);
            bool change = false;

            if (setting.Value != val)
                change = true;

            setting.Value = val;

            return change;
        }

        public static bool CustomDrawer_FloatSlider(Rect rect, SettingHandle<float> setting, bool def_isPercentage, Color background)
        {
            return CustomDrawer_FloatSlider(rect, setting, def_isPercentage, 0, 1, background);
        }

        public static bool CustomDrawer_SpeedBiasSliderMelee(Rect rect, SettingHandle<float> setting, Color background, float min = 0.5f, float max = 2f, float mid = 1f)
        {
            return CustomDrawer_SpeedBiasSlider(rect, setting, background, false, min, max, mid);
        }

        public static bool CustomDrawer_SpeedBiasSliderRanged(Rect rect, SettingHandle<float> setting, Color background, float min = 0.5f, float max = 2f, float mid = 1f)
        {
            return CustomDrawer_SpeedBiasSlider(rect, setting, background, true, min, max, mid);
        }
         
        public static bool CustomDrawer_SpeedBiasSlider(Rect rect, SettingHandle<float> setting, Color background, bool isForMelee, float min, float max, float mid)
        {
            float diff_midToMin = mid - min;
            float diff_maxToMid = max - mid;
            float diff_maxToMin = max - min;

            float val = setting.Value;

            drawBackground(rect, background);

            Rect innerRect = new Rect(rect);
            innerRect.y += 24;
            innerRect.height -= 24;

            Rect sliderPortion = new Rect(innerRect);
            sliderPortion.width = sliderPortion.width - 50;
            Rect labelPortion = new Rect(innerRect);
            labelPortion.width = 50;
            labelPortion.position = new Vector2(sliderPortion.position.x + sliderPortion.width + 5f, sliderPortion.position.y + 4f);

            sliderPortion = sliderPortion.ContractedBy(2f);

            Widgets.Label(labelPortion, (Mathf.Round(setting.Value * 100f)).ToString("F0") + "%");

            

            {
                float centerX = sliderPortion.width * (diff_midToMin / diff_maxToMin);
                Color save = GUI.color;

                if (isForMelee)
                {
                    DrawIconForWeapon(DefDatabase<ThingDef>.GetNamed("Gun_Revolver"), rect, new Vector2(0, 0), -1, true);
                    DrawIconForWeapon(DefDatabase<ThingDef>.GetNamed("Gun_Autopistol"), rect, new Vector2(centerX - IconSize / 2, 0), -1, true);
                    DrawIconForWeapon(DefDatabase<ThingDef>.GetNamed("Gun_MachinePistol"), rect, new Vector2(sliderPortion.width - IconSize, 0), -1, true);
                }
                else
                {
                    DrawIconForWeapon(DefDatabase<ThingDef>.GetNamed("MeleeWeapon_Club"), rect, new Vector2(0, 0), -1, true);
                    DrawIconForWeapon(DefDatabase<ThingDef>.GetNamed("MeleeWeapon_LongSword"), rect, new Vector2(centerX - IconSize / 2, 0), -1, true);
                    DrawIconForWeapon(DefDatabase<ThingDef>.GetNamed("MeleeWeapon_Knife"), rect, new Vector2(sliderPortion.width - IconSize, 0), -1, true);
                }

                if (setting.Value >= mid)
                {
                    GUI.color = Color.white;
                    float valFraction = (val - mid) / diff_maxToMid;
                    float width = (sliderPortion.width - centerX);
                    Rect backRect = new Rect(sliderPortion.x + centerX, sliderPortion.y, width * valFraction, sliderPortion.height);
                    GUI.DrawTextureWithTexCoords(backRect, TextureResources.SpeedBiasSliderPositive, new Rect(0,0,valFraction,1));
                }
                else
                {
                    GUI.color = Color.white;
                    float valFraction = (mid - val) / diff_midToMin;
                    float width = sliderPortion.width - (sliderPortion.width - centerX);
                    Rect backRect = new Rect(sliderPortion.x + (centerX - (width * valFraction)), sliderPortion.y, width * valFraction, sliderPortion.height);
                    GUI.DrawTextureWithTexCoords(backRect, TextureResources.SpeedBiasSliderNegative, new Rect(1-valFraction, 0, valFraction, 1));
                }
                GUI.color = save;
            }

            float result = Widgets.HorizontalSlider(sliderPortion, setting.Value, min, max, true);
            bool change = false;

            if (setting.Value != result)
                change = true;

            setting.Value = result;

            return change;
        }

        internal static bool CustomDrawer_Enumlist(SettingHandle handle, Rect controlRect, string[] enumNames, float[] forcedWidths, ExpansionMode expansionMode, Color background)
        {
            drawBackground(controlRect, background);
            if (enumNames == null) return false;
            if (enumNames.Length != forcedWidths.Length) return false;

            if (expansionMode == ExpansionMode.Horizontal)
                throw new NotImplementedException("Horizontal scrolling not yet implemented.");

            float buttonWidth = controlRect.width;
            int forcedButtons = 0;
            for (int i = 0; i < forcedWidths.Length; i++)
            {
                if(forcedWidths[i] != 0f)
                {
                    forcedButtons++;
                    buttonWidth -= forcedWidths[i];
                }
            }
            if (forcedButtons != enumNames.Length)
                buttonWidth /= (float)(enumNames.Length - forcedButtons);

            float position = controlRect.position.x;

            bool changed = false;

            for(int i = 0; i < enumNames.Length; i++)
            {
                float width = (forcedWidths[i] == 0f) ? buttonWidth : forcedWidths[i];

                Rect buttonRect = new Rect(controlRect);
                buttonRect.position = new Vector2(position, buttonRect.position.y);
                buttonRect.width = width;
                //buttonRect = buttonRect.ContractedBy(2f);
                bool interacted = false;

                bool selected = handle.StringValue.Equals(enumNames[i]);

                string label = (handle.EnumStringPrefix + enumNames[i]).Translate();

                if(expansionMode == ExpansionMode.Vertical)
                {
                    float height = Text.CalcHeight(label, width);
                    if (handle.CustomDrawerHeight < height) handle.CustomDrawerHeight = height;
                }

                Color activeColor = GUI.color;
                if (selected)
                    GUI.color = SelectedOptionColor;

                interacted = Widgets.ButtonText(buttonRect, label);

                if (selected)
                    GUI.color = activeColor;

                if (interacted)
                {
                    handle.StringValue = enumNames[i];
                    changed = true;
                }

                position += width;
            }
            return changed;
        }
        
        internal static bool CustomDrawer_ButtonExportConfig(Rect rect, string label, SimpleSidearms instance, Color background)
        {
            drawBackground(rect, background);
            Rect buttonRect = new Rect(rect);
            buttonRect.position = new Vector2(buttonRect.position.x + buttonRect.width - 200, buttonRect.position.y);
            buttonRect.width = 200;
            Color keptColor = GUI.color;
            GUI.color = Color.cyan;
            bool clicked = Widgets.ButtonText(buttonRect, label.Translate());
            if (clicked)
            {
                UnsafeUtilities.ExportConfigViaReflection(instance);
            }
            GUI.color = keptColor;
            return false; 
        }

        internal static bool CustomDrawer_ButtonLoadConfig(Rect rect, SettingHandle<Preset> setting, string label, SimpleSidearms instance, Color background)
        {
            drawBackground(rect, background);
            Rect buttonRect = new Rect(rect);
            buttonRect.width = 50;
            Color keptColor = GUI.color;
            if (SimpleSidearms.ActivePreset.Value == setting.Value)
                GUI.color = SelectedOptionColor;
            bool clicked = Widgets.ButtonText(buttonRect, label.Translate());
            if (clicked)
            {
                Presets.presetChanged(setting.Value, instance);
            }
            GUI.color = keptColor;
            return clicked;
        }

        internal static bool CustomDrawer_RighthandSideLine(Rect wholeRect, SettingHandle setting, Color background)
        {
            return CustomDrawer_RighthandSideLine(wholeRect, setting, constGrey, background);
        }

        internal static bool CustomDrawer_RighthandSideLine(Rect wholeRect, SettingHandle setting, Color color, Color background)
        {
            drawBackground(wholeRect, background);
            wholeRect.position = new Vector2(wholeRect.position.x, wholeRect.position.y + (wholeRect.height - 1f) / 2f);
            wholeRect.height = 1f;
            GUI.color = color;
            GUI.DrawTexture(wholeRect, TexUI.FastFillTex);
            GUI.color = Color.white;
            return false;
        }

        internal static bool CustomDrawer_RighthandSideLabel(Rect wholeRect, string label, Color background)
        {
            drawBackground(wholeRect, background);
            DrawLabel(label, wholeRect, TextMargin);
            return false;
        }

        internal static bool CustomDrawer_MatchingWeapons_passiveRelative(Rect wholeRect, SettingHandle<WeaponListKind> setting, Color background)
        {
            float weightLimit = averageCarryCapacity;
            switch (setting.Value)
            {
                case WeaponListKind.Melee:
                    weightLimit *= SimpleSidearms.LimitModeSingleMelee_Relative.Value;
                    break;
                case WeaponListKind.Ranged:
                    weightLimit *= SimpleSidearms.LimitModeSingleRanged_Relative.Value;
                    break;
                case WeaponListKind.Both:
                default:
                    weightLimit *= SimpleSidearms.LimitModeSingle_Relative.Value;
                    break;
            }
            return CustomDrawer_MatchingWeapons_passive(wholeRect, setting, weightLimit, "Sidearms below avg. max. weight ("+weightLimit.ToString("F1")+"kg)", background);
        }

        internal static bool CustomDrawer_MatchingWeapons_passiveAbsolute(Rect wholeRect, SettingHandle<WeaponListKind> setting, Color background)
        {
            float weightLimit = 0;
            switch (setting.Value)
            {
                case WeaponListKind.Melee:
                    weightLimit = SimpleSidearms.LimitModeSingleMelee_Absolute.Value;
                    break;
                case WeaponListKind.Ranged:
                    weightLimit = SimpleSidearms.LimitModeSingleRanged_Absolute.Value;
                    break;
                case WeaponListKind.Both:
                default:
                    weightLimit = SimpleSidearms.LimitModeSingle_Absolute.Value;
                    break;
            }
            return CustomDrawer_MatchingWeapons_passive(wholeRect, setting, weightLimit, "Sidearms below maximum weight", background);

        }

        private static bool CustomDrawer_MatchingWeapons_passive(Rect wholeRect, SettingHandle<WeaponListKind> setting, float weightLimit , string label, Color background)
        {
            drawBackground(wholeRect, background);
            Rect offsetRect = new Rect(wholeRect);
            offsetRect.height = wholeRect.height - TextMargin + BottomMargin;
            offsetRect.position = new Vector2(offsetRect.position.x, offsetRect.position.y);

            DrawLabel(label, offsetRect, TextMargin);

            offsetRect.position = new Vector2(offsetRect.position.x, offsetRect.position.y + TextMargin);

            IEnumerable<ThingStuffPair> validSidearms = GettersFilters.getValidSidearms();

            List<ThingStuffPair> matchingSidearms = GettersFilters.filterForWeaponKind(validSidearms, MiscUtils.LimitTypeToListType(setting.Value)).ToList();

            matchingSidearms = matchingSidearms.OrderBy(t => t.thing.GetStatValueAbstract(StatDefOf.Mass, t.stuff)).ToList();

            List<ThingStuffPair> matchingSidearmsUnderLimit =  new List<ThingStuffPair>();
            for (int i = 0; i < matchingSidearms.Count; i++)
            {
                float mass = matchingSidearms[i].thing.GetStatValueAbstract(StatDefOf.Mass);
                if (mass > weightLimit)
                    break; 
                else
                    matchingSidearmsUnderLimit.Add(matchingSidearms[i]);
            }

            int iconsPerRow = (int)(offsetRect.width / (IconGap + IconSize));

            int biggerRows = ((matchingSidearmsUnderLimit.Count - 1) / iconsPerRow) + 1;
            setting.CustomDrawerHeight = (biggerRows * IconSize) + ((biggerRows) * IconGap) + TextMargin;

            for (int i = 0; i < matchingSidearmsUnderLimit.Count; i++)
            {
                int collum = (i % iconsPerRow);
                int row = (i / iconsPerRow);
                bool interacted = DrawIconForWeapon(matchingSidearmsUnderLimit[i], offsetRect, new Vector2(IconSize* collum + collum * IconGap, IconSize * row + row * IconGap),i);
                if(interacted)
                {
                    //nothing, since this is the passive list
                }
            }
            return false;
        }

        private static void DrawLabel(string labelText, Rect textRect, float offset)
        {
            var labelHeight = Text.CalcHeight(labelText, textRect.width);
            labelHeight -= 2f;
            var labelRect = new Rect(textRect.x, textRect.yMin - labelHeight + offset, textRect.width, labelHeight);
            GUI.DrawTexture(labelRect, TexUI.GrayTextBG);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(labelRect, labelText);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }

        internal static bool CustomDrawer_MatchingWeapons_active(Rect wholeRect, SettingHandle<ThingDefHashSetHandler> setting, WeaponListKind kind, Color background, string yesText = "Sidearms", string noText = "Not sidearms")
        {
            drawBackground(wholeRect, background);
            if (setting.Value == null)
                setting.Value = new ThingDefHashSetHandler();

            GUI.color = Color.white;

            Rect leftRect = new Rect(wholeRect);
            leftRect.width = leftRect.width / 2;
            leftRect.height = wholeRect.height - TextMargin + BottomMargin;
            leftRect.position = new Vector2(leftRect.position.x, leftRect.position.y);
            Rect rightRect = new Rect(wholeRect);
            rightRect.width = rightRect.width / 2;
            leftRect.height = wholeRect.height - TextMargin + BottomMargin;
            rightRect.position = new Vector2(rightRect.position.x + leftRect.width, rightRect.position.y);

            DrawLabel(yesText, leftRect, TextMargin);
            DrawLabel(noText, rightRect, TextMargin);

            leftRect.position = new Vector2(leftRect.position.x, leftRect.position.y + TextMargin);
            rightRect.position = new Vector2(rightRect.position.x, rightRect.position.y + TextMargin);

            int iconsPerRow = (int)(leftRect.width / (IconGap + IconSize));

            IEnumerable<ThingDef> matchingSidearmsEnumerable;
            IEnumerable<ThingDef> allSidearmsEnumerable = GettersFilters.getValidWeaponsThingDefsOnly();

            switch (kind)
            {
                case WeaponListKind.Melee:
                    matchingSidearmsEnumerable = allSidearmsEnumerable.Where(t => t.IsMeleeWeapon);
                    break;
                case WeaponListKind.Ranged:
                    matchingSidearmsEnumerable = allSidearmsEnumerable.Where(t => t.IsRangedWeapon);
                    break;
                case WeaponListKind.Both:
                default:
                    matchingSidearmsEnumerable = allSidearmsEnumerable;
                    break;
            }


            List<ThingDef> matchingSidearms = matchingSidearmsEnumerable.ToList();
            List<ThingDef> allSidearms = allSidearmsEnumerable.ToList();

            matchingSidearms = matchingSidearms.OrderBy(t => t.GetStatValueAbstract(StatDefOf.Mass)).ToList();

            List<ThingDef> selectedSidearms = setting.Value.InnerList.ToList();

            List<ThingDef> unselectedSidearms = new List<ThingDef>();
            for (int i = 0; i < matchingSidearms.Count; i++)
            {
                if (!selectedSidearms.Contains(matchingSidearms[i]))
                    unselectedSidearms.Add(matchingSidearms[i]);
            }

            bool change = false;

            int biggerRows = Math.Max((selectedSidearms.Count - 1) / iconsPerRow, (unselectedSidearms.Count - 1) / iconsPerRow) + 1;
            setting.CustomDrawerHeight = (biggerRows * IconSize) + ((biggerRows) * IconGap) + TextMargin;

            for(int i = 0; i < selectedSidearms.Count; i++)
            {
                int collum = (i % iconsPerRow);
                int row = (i / iconsPerRow);
                bool interacted = DrawIconForWeapon(selectedSidearms[i], leftRect, new Vector2(IconSize * collum + collum * IconGap, IconSize * row + row * IconGap), i);
                if (interacted)
                {
                    change = true;
                    setting.Value.InnerList.Remove(selectedSidearms[i]);
                }
            }

            for (int i = 0; i < unselectedSidearms.Count; i++)
            {
                int collum = (i % iconsPerRow);
                int row = (i / iconsPerRow);
                bool interacted = DrawIconForWeapon(unselectedSidearms[i], rightRect, new Vector2(IconSize * collum + collum * IconGap, IconSize * row + row * IconGap), i);
                if (interacted)
                {
                    change = true;
                    setting.Value.InnerList.Add(unselectedSidearms[i]);
                }
            } 
            return change;
        }

        private static bool DrawIconForWeapon(ThingDef weaponDef, Rect contentRect, Vector2 iconOffset, int buttonID, bool isBackground = false)
        {
            return DrawIconForWeapon(new ThingStuffPair(weaponDef, null), contentRect, iconOffset, buttonID, isBackground);
            
        }

        private static bool DrawIconForWeapon(ThingStuffPair weapon, Rect contentRect, Vector2 iconOffset, int buttonID, bool isBackground = false)
        {
            var iconTex = weapon.thing.uiIcon;
            Graphic g = weapon.thing.graphicData.Graphic;
            Color color = weapon.getDrawColor();
            Color colorTwo = weapon.getDrawColorTwo();
            Graphic g2 = weapon.thing.graphicData.Graphic.GetColoredVersion(g.Shader, color, colorTwo);

            var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, IconSize, IconSize);

            if (!contentRect.Contains(iconRect))
                return false;

            if (!isBackground)
            {
                string label = weapon.getLabelCap();

                TooltipHandler.TipRegion(iconRect, label);
                MouseoverSounds.DoRegion(iconRect, SoundDefOf.Mouseover_Command);
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
            }

            Texture resolvedIcon;
            if (!weapon.thing.uiIconPath.NullOrEmpty())
            {
                resolvedIcon = weapon.thing.uiIcon;
            }
            else
            {
                resolvedIcon = g2.MatSingle.mainTexture;
            }
            GUI.color = color;
            GUI.DrawTexture(iconRect, resolvedIcon);
            GUI.color = Color.white;

            if (!isBackground)
            {
                if (Widgets.ButtonInvisible(iconRect, true))
                {
                    Event.current.button = buttonID;
                    return true;
                }
            }
            return false;
        }
    }
}
