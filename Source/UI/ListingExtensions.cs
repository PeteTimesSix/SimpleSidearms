using System;
using UnityEngine;
using Verse;

using static PeteTimesSix.SimpleSidearms.UI.UIComponents;

namespace PeteTimesSix.SimpleSidearms.UI
{
    public static class ListingExtensions
    {
        private static readonly Color SelectedButtonColor = new Color(.65f, 1f, .65f);
        private static float ButtonTextPadding = 5f;
        private static float AfterLabelMinGap = 10f;

        public static float ColumnGap = 17f;

        public static void SliderLabeled(this Listing_Standard instance, string label, ref float value, float min, float max, float displayMult = 1, string valueSuffix = "")
        {
            instance.Label($"{label}: {(value * displayMult).ToString("F0")}{valueSuffix}");
            value = instance.Slider(value, min, max);
        }

        public static void Spinner(this Listing_Standard instance, string label, ref int value, int increment = 1, int? min = null, int? max = null, string tooltip = null)
        {
            float lineHeight = Text.LineHeight;
            float labelWidth = Text.CalcSize(label).x + AfterLabelMinGap;

            var rect = instance.GetRect(lineHeight);
            var buttonSize = lineHeight;

            var textRect = new Rect(rect.x + buttonSize + 1, rect.y, rect.width - buttonSize * 2 - 2f, rect.height);
            NumberField(ref value, textRect);

            var leftButtonRect = new Rect(rect.x, rect.y, buttonSize, buttonSize);
            var rightButtonRect = new Rect(rect.x + rect.width - buttonSize, rect.y, buttonSize, buttonSize);
            if (Widgets.ButtonText(leftButtonRect, "-") && (!min.HasValue || min <= value - increment))
            {
                value -= increment;
            }
            if (Widgets.ButtonText(rightButtonRect, "+") && (!max.HasValue || max >= value + increment))
            {
                value += increment;
            }
        }

        public static void NumberField(ref int value, Rect rect)
        {
            string valText = Widgets.TextField(rect, value.ToString());
            if (int.TryParse(valText, out int result))
            {
                value = result;
            }
            else
            {
                DrawBadTextValueOutline(rect);
            }
        }

        public static void EnumSelector<T>(this Listing_Standard listing, string label, ref T value, string valueLabelPrefix, string valueTooltipPostfix = "_tooltip", string tooltip = null) where T : Enum
        {
            string[] names = Enum.GetNames(value.GetType());

            float lineHeight = Text.LineHeight;
            float labelWidth = Text.CalcSize(label).x + AfterLabelMinGap;

            var tempWidth = listing.ColumnWidth;

            float buttonsWidth = 0f;
            foreach (var name in names)
            {
                string text = (valueLabelPrefix + name).Translate();
                float width = Text.CalcSize(text).x + ButtonTextPadding * 2f;
                if (buttonsWidth < width)
                    buttonsWidth = width;
            }

            bool fitsOnLabelRow = (((buttonsWidth * names.Length) + labelWidth) < tempWidth);
            float buttonsRectWidth = fitsOnLabelRow ?
                listing.ColumnWidth - (labelWidth) :
                listing.ColumnWidth;

            int rowNum = 0;
            int columnNum = 0;
            int maxColumnNum = 0;
            foreach (var name in names)
            {
                if ((columnNum + 1) * buttonsWidth > buttonsRectWidth)
                {
                    columnNum = 0;
                    rowNum++;
                }
                float x = (columnNum * buttonsWidth);
                float y = rowNum * lineHeight;
                columnNum++;
                if (rowNum == 0 && maxColumnNum < columnNum)
                    maxColumnNum = columnNum;
            }
            rowNum++; //label row
            if (!fitsOnLabelRow)
                rowNum++;

            Rect wholeRect = listing.GetRect((float)rowNum * lineHeight);

            if (!tooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(wholeRect))
                {
                    Widgets.DrawHighlight(wholeRect);
                }
                TooltipHandler.TipRegion(wholeRect, tooltip);
            }

            Rect labelRect = wholeRect.TopPartPixels(lineHeight).LeftPartPixels(labelWidth);
            GUI.color = Color.white;
            Widgets.Label(labelRect, label);

            Rect buttonsRect = fitsOnLabelRow ?
                wholeRect.RightPartPixels(buttonsRectWidth) :
                wholeRect.BottomPartPixels(wholeRect.height - lineHeight);

            buttonsWidth = buttonsRectWidth / (float)maxColumnNum;

            rowNum = 0;
            columnNum = 0;
            foreach (var name in names)
            {
                if ((columnNum + 1) * buttonsWidth > buttonsRectWidth)
                {
                    columnNum = 0;
                    rowNum++;
                }
                float x = (columnNum * buttonsWidth);
                float y = rowNum * lineHeight;
                columnNum++;
                string buttonText = (valueLabelPrefix + name).Translate();
                var enumValue = (T)Enum.Parse(value.GetType(), name);
                GUI.color = value.Equals(enumValue) ? SelectedButtonColor : Color.white;
                var buttonRect = new Rect(buttonsRect.x + x, buttonsRect.y + y, buttonsWidth, lineHeight);
                if (valueTooltipPostfix != null)
                    TooltipHandler.TipRegion(buttonRect, (valueLabelPrefix + name + valueTooltipPostfix).Translate());
                bool clicked = Widgets.ButtonText(buttonRect, buttonText);
                if (clicked)
                    value = enumValue;
            }

            listing.Gap(listing.verticalSpacing);
            GUI.color = Color.white;
        }

        public static Listing_Standard BeginHiddenSection(this Listing_Standard instance, out float maxHeightAccumulator)
        {
            Rect rect = instance.GetRect(0);
            rect.height = 10000f; 
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(rect);
            maxHeightAccumulator = 0f;
            return listing_Standard;
        }

        public static void NewHiddenColumn(this Listing_Standard instance, ref float maxHeightAccumulator)
        {
            if (maxHeightAccumulator < instance.CurHeight)
                maxHeightAccumulator = instance.CurHeight;
            instance.NewColumn();
        }

        public static void EndHiddenSection(this Listing_Standard instance, Listing_Standard section, float maxHeightAccumulator)
        {
            instance.GetRect(Mathf.Max(section.CurHeight, maxHeightAccumulator));
            section.End();
        }
    }
}
