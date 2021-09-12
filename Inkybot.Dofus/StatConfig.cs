using System;

namespace Inkybot.Dofus
{
    /**
     * <summary>
     * The StatConfig struct represents the AI's configuration the user is using
     * during the maging process.
     * It is a readonly struct. Whenever the user modifies his config, a new struct
     * is instantiated.
     * </summary>
     */
    public readonly struct StatConfig
    {
        /**
         * <summary>An empty stat configuration</summary>
         */
        public static StatConfig None = new StatConfig(default, default);

        public override bool Equals(object obj) {
            if (obj is StatConfig other) {
                return Equals(other);
            }
            return base.Equals(obj);
        }

        public bool Equals(StatConfig other) =>
            Deconstruct().Equals(other.Deconstruct());

        public override int GetHashCode() {
         unchecked {
          var hashCode = MaxValueAtWhichSmRuneCanHit.GetHashCode();
          hashCode = (hashCode * 397) ^ ChangeToPaRuneThreshold.GetHashCode();
          hashCode = (hashCode * 397) ^ UseSmRunes.GetHashCode();
          hashCode = (hashCode * 397) ^ UsePaRunes.GetHashCode();
          hashCode = (hashCode * 397) ^ UseRaRunes.GetHashCode();
          hashCode = (hashCode * 397) ^ MaxValueAtWhichPaRuneCanHit.GetHashCode();
          hashCode = (hashCode * 397) ^ ChangeToRaRuneThreshold.GetHashCode();
          hashCode = (hashCode * 397) ^ HighSinkStat.GetHashCode();
          return hashCode;
         }
        }

        /**
         * <summary>
         * The maximum value at which a rune of SM strength can still land on the stat.
         * If set to null, SM runes can always land.
         * </summary>
         */
        public readonly int? MaxValueAtWhichSmRuneCanHit;
        
        /**
         * <summary>
         * The lowest value at which a rune of PA strength should begin to be used.
         * If set to null, PA runes should not be used.
         * </summary>
         */
        public readonly int? ChangeToPaRuneThreshold;
        
        public readonly bool UseSmRunes;
        public readonly bool UsePaRunes;
        public readonly bool UseRaRunes;
        
        /**
         * <summary>
         * The maximum value at which a rune of PA strength can still land on the stat.
         * If set to null, PA runes can always land.
         * </summary>
         */
        public readonly int? MaxValueAtWhichPaRuneCanHit;
        
        /**
         * <summary>
         * The lowest value at which a rune of RA strength should begin to be used.
         * If set to null, RA runes should not be used.
         * </summary>
         */
        public readonly int? ChangeToRaRuneThreshold;
        
        /**
         * <summary>
         * Whether or not runes of SM strength should be used.
         * </summary>
         */
        public readonly bool ShouldUseSmRunes => UseSmRunes;
        
        /**
         * <summary>
         * Whether or not runes of PA strength should be used.
         * </summary>
         */
        public readonly bool ShouldUsePaRunes => UsePaRunes && ChangeToPaRuneThreshold != null;
        
        /**
         * <summary>
         * Whether or not runes of RA strength should be used.
         * </summary>
         */
        public readonly bool ShouldUseRaRunes => UseRaRunes && ChangeToRaRuneThreshold != null;
        
       /**
        * <summary>
        * Determines whether or not the stat should be interpreted as a high-sink stat.
        * </summary>
        */
       public readonly bool HighSinkStat;

        /**
         * <param name="maxValueSmRuneCanHit">
         * The maximum value at which a rune of SM strength can still land on the stat.
         * If set to null, SM runes can always land.
         * </param>
         *
         * <param name="changeToPaRuneThreshold">
         * The lowest value at which a rune of PA strength should begin to be used.
         * If set to null, PA runes should not be used.
         * </param>
         *
         * <param name="maxValuePaRuneCanHit">
         * The maximum value at which a rune of PA strength can still land on the stat.
         * If set to null, PA runes can always land.
         * </param>
         *
         * <param name="changeToRaRuneThreshold">
         * The lowest value at which a rune of RA strength should begin to be used.
         * If set to null, RA runes should not be used.
         * </param>
         *
         * <param name="highSinkStat">
         * Determines whether or not the stat should be interpreted as a high-sink stat.
         * </param>
         */
        public StatConfig(
            int? maxValueSmRuneCanHit=null,
            int? changeToPaRuneThreshold=null,
            int? maxValuePaRuneCanHit=null,
            int? changeToRaRuneThreshold=null,
            bool useSmRunes=true,
            bool usePaRunes=true,
            bool useRaRunes=true,
            bool highSinkStat=false) =>
            (UseSmRunes, UsePaRunes, UseRaRunes, MaxValueAtWhichSmRuneCanHit, ChangeToPaRuneThreshold, MaxValueAtWhichPaRuneCanHit, ChangeToRaRuneThreshold, HighSinkStat) =
            (useSmRunes, usePaRunes, useRaRunes, maxValueSmRuneCanHit, changeToPaRuneThreshold, maxValuePaRuneCanHit, changeToRaRuneThreshold, highSinkStat);

        /**
         * <param name="a">Tuple of configuration options</param>
         */
        public StatConfig((
            int? maxValueSmRuneCanHit,
            int? changeToPaRuneThreshold,
            int? maxValuePaRuneCanHit,
            int? changeToRaRuneThreshold,
            bool useSmRunes,
            bool usePaRunes,
            bool useRaRunes,
            bool highSinkStat) a) :
            this(
                a.maxValueSmRuneCanHit,
                a.changeToPaRuneThreshold,
                a.maxValuePaRuneCanHit,
                a.changeToRaRuneThreshold,
                a.useSmRunes,
                a.usePaRunes,
                a.useRaRunes,
                a.highSinkStat
                ) { }

        /**
         * <returns>A tuple of configuration options</returns>
         */
        public (int? maxValueSmRuneCanHit,
            int? changeToPaRuneThreshold,
            int? maxValuePaRuneCanHit,
            int? changeToRaRuneThreshold,
            bool useSmRunes,
            bool usePaRunes,
            bool useRaRunes,
            bool highSinkStat)
            Deconstruct() => (
            MaxValueAtWhichSmRuneCanHit,
            ChangeToPaRuneThreshold,
            MaxValueAtWhichPaRuneCanHit,
            ChangeToRaRuneThreshold,
            UseSmRunes,
            UsePaRunes,
            UseRaRunes,
            HighSinkStat
        );
    }
}
