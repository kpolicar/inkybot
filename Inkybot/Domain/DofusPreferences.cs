using System.Reflection;
using System;

namespace Inkybot.Domain
{
    public static partial class DetectUserGame
    {
        [Serializable]
        [Obfuscation(Exclude = true, ApplyToMembers = true)]
        public struct DofusPreferences : IEquatable<DofusPreferences>
        {
            public DofusIntValue uiScale;
            public DofusRenderingScaleWrapper renderingScale;
            public DofusIntValue dofusQuality;
            public DofusIntValue windowResolutionMode;
            public DofusIntValue windowDisplayMode;

            public static readonly DofusPreferences Ideal = new DofusPreferences {
                uiScale             = new DofusIntValue { value = 90 },
                renderingScale      = new DofusRenderingScaleWrapper { value = new DofusRenderingScale { isMute = false, value = 100 } },
                dofusQuality        = new DofusIntValue { value = 1 },
                windowResolutionMode = new DofusIntValue { value = 0 },
                windowDisplayMode   = new DofusIntValue { value = 1 },
            };

            public bool Equals(DofusPreferences other) =>
                uiScale == other.uiScale &&
                renderingScale == other.renderingScale &&
                dofusQuality == other.dofusQuality &&
                windowResolutionMode == other.windowResolutionMode &&
                windowDisplayMode == other.windowDisplayMode;

            public override bool Equals(object? obj) => obj is DofusPreferences other && Equals(other);
            public override int GetHashCode() { unchecked { int h = 17; h = h * 31 + uiScale.GetHashCode(); h = h * 31 + renderingScale.GetHashCode(); h = h * 31 + dofusQuality.GetHashCode(); h = h * 31 + windowResolutionMode.GetHashCode(); h = h * 31 + windowDisplayMode.GetHashCode(); return h; } }
            public static bool operator ==(DofusPreferences a, DofusPreferences b) => a.Equals(b);
            public static bool operator !=(DofusPreferences a, DofusPreferences b) => !a.Equals(b);
        }

        [Serializable]
        public struct DofusIntValue : IEquatable<DofusIntValue>
        {
            public int value;

            public bool Equals(DofusIntValue other) => value == other.value;
            public override bool Equals(object? obj) => obj is DofusIntValue other && Equals(other);
            public override int GetHashCode() => value.GetHashCode();
            public static bool operator ==(DofusIntValue a, DofusIntValue b) => a.value == b.value;
            public static bool operator !=(DofusIntValue a, DofusIntValue b) => a.value != b.value;
        }

        [Serializable]
        public struct DofusRenderingScaleWrapper : IEquatable<DofusRenderingScaleWrapper>
        {
            public DofusRenderingScale value;

            public bool Equals(DofusRenderingScaleWrapper other) => value == other.value;
            public override bool Equals(object? obj) => obj is DofusRenderingScaleWrapper other && Equals(other);
            public override int GetHashCode() => value.GetHashCode();
            public static bool operator ==(DofusRenderingScaleWrapper a, DofusRenderingScaleWrapper b) => a.value == b.value;
            public static bool operator !=(DofusRenderingScaleWrapper a, DofusRenderingScaleWrapper b) => !a.Equals(b);
        }

        [Serializable]
        public struct DofusRenderingScale : IEquatable<DofusRenderingScale>
        {
            public bool isMute;
            public int value;

            public bool Equals(DofusRenderingScale other) => isMute == other.isMute && value == other.value;
            public override bool Equals(object? obj) => obj is DofusRenderingScale other && Equals(other);
            public override int GetHashCode() { unchecked { return isMute.GetHashCode() * 31 + value.GetHashCode(); } }
            public static bool operator ==(DofusRenderingScale a, DofusRenderingScale b) => a.Equals(b);
            public static bool operator !=(DofusRenderingScale a, DofusRenderingScale b) => !a.Equals(b);
        }
    }
}
