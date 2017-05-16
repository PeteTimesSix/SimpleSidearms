using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace SimpleSidearms
{
    [StaticConstructorOnStartup]
    class TextureResources
    {

        public static readonly Texture2D drawRanged = ContentFinder<Texture2D>.Get("drawRanged", true);
        public static readonly Texture2D drawMelee = ContentFinder<Texture2D>.Get("drawMelee", true);

        public static readonly Texture2D drawPocket = ContentFinder<Texture2D>.Get("drawPocket", true);

    }
}
