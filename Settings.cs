using System;
using Modding;
namespace AnyRadiance
{
    [Serializable]
    public class LocalSettings:ModSettings
    {
        public BossStatue.Completion Completion = new BossStatue.Completion
        {
            isUnlocked = true,
            hasBeenSeen = true,
        };
        public bool UsingAltVersion = false;
        public bool InBossDoor = false;
    }
}