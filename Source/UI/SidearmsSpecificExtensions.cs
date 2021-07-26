using RimWorld;
using SimpleSidearms.rimworld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PeteTimesSix.SimpleSidearms.UI
{
    public static class SidearmsSpecificExtensions
    {
        public const float IconSize = 32f;
        public static readonly Color iconBaseColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        public static readonly Color iconMouseOverColor = new Color(0.6f, 0.6f, 0.4f, 1f);


        public static void SliderSpeedBias(this Listing_Standard instance, string label, ref float value, float min, float mid, float max, bool isMelee, float displayMult = 100, string valueSuffix = "%")
        {
            Color save = GUI.color;

            float diff_midToMin = mid - min;
            float diff_maxToMid = max - mid;
            float diff_maxToMin = max - min;

            instance.Label($"{label}: {(value * displayMult).ToString("F0")}{valueSuffix}");

            var iconRect = instance.GetRect(IconSize * 1.00f);
            
            float centerX = instance.ColumnWidth * (diff_midToMin / diff_maxToMin);

            {
                if (isMelee)
                {
                    DrawIconForWeapon(DefDatabase<ThingDef>.GetNamed("MeleeWeapon_Club"), iconRect, new Vector2(0, 0), true);
                    DrawIconForWeapon(DefDatabase<ThingDef>.GetNamed("MeleeWeapon_LongSword"), iconRect, new Vector2(centerX - IconSize / 2, 0), true);
                    DrawIconForWeapon(DefDatabase<ThingDef>.GetNamed("MeleeWeapon_Knife"), iconRect, new Vector2(iconRect.width - IconSize, 0), true);
                }
                else
                {
                    DrawIconForWeapon(DefDatabase<ThingDef>.GetNamed("Gun_Revolver"), iconRect, new Vector2(0, 0), true);
                    DrawIconForWeapon(DefDatabase<ThingDef>.GetNamed("Gun_Autopistol"), iconRect, new Vector2(centerX - IconSize / 2, 0), true);
                    DrawIconForWeapon(DefDatabase<ThingDef>.GetNamed("Gun_MachinePistol"), iconRect, new Vector2(iconRect.width - IconSize, 0), true);
                }
            }
            var texRect = instance.GetRect(0f); //zero to prevent actually moving the listing, but this rect has the correct x, y, and width
            texRect.height = 22f;               //sliders are 22 pixels tall and I want this texture *under* the slider

            { 
                if (value >= mid)
                {
                    GUI.color = Color.white;
                    float valFraction = (value - mid) / diff_maxToMid;
                    float width = (texRect.width - centerX);
                    Rect backRect = new Rect(texRect.x + centerX, texRect.y, width * valFraction, texRect.height);
                    GUI.DrawTextureWithTexCoords(backRect, TextureResources.SpeedBiasSliderPositive, new Rect(0, 0, valFraction, 1));
                }
                else
                {
                    GUI.color = Color.white;
                    float valFraction = (mid - value) / diff_midToMin;
                    float width = texRect.width - (texRect.width - centerX);
                    Rect backRect = new Rect(texRect.x + (centerX - (width * valFraction)), texRect.y, width * valFraction, texRect.height);
                    GUI.DrawTextureWithTexCoords(backRect, TextureResources.SpeedBiasSliderNegative, new Rect(1 - valFraction, 0, valFraction, 1));
                }
            }

            value = instance.Slider(value, min, max);

            GUI.color = save;
        }

        public static bool DrawIconForWeapon(ThingDef weaponDef, Rect contentRect, Vector2 iconOffset, bool isBackground = false)
        {
            return DrawIconForWeapon(new ThingDefStuffDefPair(weaponDef, null), contentRect, iconOffset, isBackground);
        }

        public static bool DrawIconForWeapon(ThingDefStuffDefPair weapon, Rect contentRect, Vector2 iconOffset, bool isBackground = false)
        {
            Graphic g = weapon.thing.graphicData.Graphic;
            Color color = weapon.getDrawColor();
            Color colorTwo = weapon.getDrawColorTwo();
            Graphic g2 = weapon.thing.graphicData.Graphic.GetColoredVersion(g.Shader, color, colorTwo);

            var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, IconSize, IconSize);

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
                    GUI.DrawTextureWithTexCoords(iconRect, TextureResources.drawPocket, new Rect(0, 0, 1, 1));
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
                    return true;
                }
            }
            return false;
        }
    }
}
