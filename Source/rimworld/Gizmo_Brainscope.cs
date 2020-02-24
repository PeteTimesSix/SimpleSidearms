using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace SimpleSidearms.rimworld
{
    class Gizmo_Brainscope : Gizmo
    {
        private const float ContentPadding = 2f;
        private Pawn parent;

        internal static Dictionary<Pawn, string> curJobs = new Dictionary<Pawn, string>();
        internal static Dictionary<Pawn, string> lastJobs = new Dictionary<Pawn, string>();

        public Gizmo_Brainscope(Pawn parent)
        {
            this.parent = parent;
        }

        public override float GetWidth(float maxWidth)
        {
            return 250f;
        }


        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            var gizmoRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            var contentRect = gizmoRect.ContractedBy(ContentPadding);
            Widgets.DrawWindowBackground(gizmoRect);

            string curJob = parent.CurJobDef == null ? "null" : parent.CurJobDef.defName;
            if (!curJobs.ContainsKey(parent))
            {
                curJobs[parent] = curJob;
                lastJobs[parent] = "none yet";
            }
            else if(curJobs[parent] != curJob)
            {
                lastJobs[parent] = curJobs[parent];
                curJobs[parent] = curJob;
            }
            string curJobDriver = parent.jobs.curDriver == null ? "null" : parent.jobs.curDriver.ToString();

            float offset = 0;
            printBool("Idle:", parent.mindState.IsIdle, contentRect, 0);                        offset += 16f;
            printStringPair("Job:", curJobs[parent], Color.white, contentRect, offset);         offset += 16f;
            printStringPair("Last job:", lastJobs[parent], Color.white, contentRect, offset);   offset += 16f;
            printStringPair("JobDriver:", curJobDriver, Color.white, contentRect, offset);      offset += 16f;

            return new GizmoResult(GizmoState.Clear);
        }

        private void printString(string str, Rect contentRect, float offset)
        {
            var str1Rect = new Rect(contentRect.x, contentRect.y + offset - 3f, contentRect.width, 22f);
            GUI.color = Color.white;
            GUI.Label(str1Rect, str);
        }

        private void printBool(string label, bool value, Rect contentRect, float offset)
        {
            Color color = value ? Color.green : Color.red;
            printStringPair(label, value.ToString(), color, contentRect, offset);
        }

        private void printStringPair(string str1, string str2, Color secondStrColor, Rect contentRect, float offset)
        {
            var str1Rect = new Rect(contentRect.x, contentRect.y + offset - 3f, contentRect.width/4, 22f);
            var str2Rect = new Rect(contentRect.x + contentRect.width/4, contentRect.y + offset - 3f, (contentRect.width/4) * 3, 22f);
            GUI.color = Color.white;
            GUI.Label(str1Rect, str1);
            GUI.color = secondStrColor;
            GUI.Label(str2Rect, str2);
        }
    }
}
