using Verse;

namespace PeteTimesSix.SimpleSidearms
{
    [StaticConstructorOnStartup]
    public static class SimpleSidearms_PostInit
    {

        static SimpleSidearms_PostInit()
        {
            InferredValues.Init();
        }

    }
}
