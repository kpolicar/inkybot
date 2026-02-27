using System;

namespace Inkybot.Domain
{
    public static partial class DetectUserGame
    {
        [Serializable]
        public class DofusPreferences
        {
            public DofusIntValue uiScale;
            public DofusRenderingScaleWrapper renderingScale;
            public DofusIntValue dofusQuality;
            public DofusIntValue windowResolutionMode;
            public DofusIntValue windowDisplayMode;
        }

        [Serializable]
        public class DofusIntValue
        {
            public int value;
        }

        [Serializable]
        public class DofusRenderingScaleWrapper
        {
            public DofusRenderingScale value;
        }

        [Serializable]
        public class DofusRenderingScale
        {
            public bool isMute;
            public int value;
        }
    }
}
