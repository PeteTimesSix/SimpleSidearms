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

        public static readonly Texture2D lockOpen = ContentFinder<Texture2D>.Get("lockOpen", true);
        public static readonly Texture2D lockClosed = ContentFinder<Texture2D>.Get("lockClosed", true);
        public static readonly Texture2D autolockOn = ContentFinder<Texture2D>.Get("locklockOn", true);
        public static readonly Texture2D autolockOff = ContentFinder<Texture2D>.Get("locklockOff", true);

        public static readonly Texture2D drawPocket = ContentFinder<Texture2D>.Get("drawPocket", true);
        public static readonly Texture2D drawPocketPrimary = ContentFinder<Texture2D>.Get("drawPocketPrimary", true);
        public static readonly Texture2D drawPocketMemory = ContentFinder<Texture2D>.Get("drawPocketMemory", true);
        public static readonly Texture2D drawPocketMemoryPrimary = ContentFinder<Texture2D>.Get("drawPocketMemoryPrimary", true);
        public static readonly Texture2D drawPocketTemp = ContentFinder<Texture2D>.Get("drawPocketTemp", true);

        public static readonly Texture2D unarmedIcon = ContentFinder<Texture2D>.Get("unarmedIcon", true);

    }
}
