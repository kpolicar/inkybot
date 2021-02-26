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
        public static StatConfig None = new StatConfig();
        
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
         * Whether or not runes of PA strength should be used.
         * </summary>
         */
        public readonly bool ShouldUsePaRunes => ChangeToPaRuneThreshold != null;
        
        /**
         * <summary>
         * Whether or not runes of RA strength should be used.
         * </summary>
         */
        public readonly bool ShouldUseRaRunes => ChangeToRaRuneThreshold != null;
        
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
            bool highSinkStat=false) =>
            (MaxValueAtWhichSmRuneCanHit, ChangeToPaRuneThreshold, MaxValueAtWhichPaRuneCanHit, ChangeToRaRuneThreshold, HighSinkStat) =
            (maxValueSmRuneCanHit, changeToPaRuneThreshold, maxValuePaRuneCanHit, changeToRaRuneThreshold, highSinkStat);

        /**
         * <param name="a">Tuple of configuration options</param>
         */
        public StatConfig((
            int? maxValueSmRuneCanHit,
            int? changeToPaRuneThreshold,
            int? maxValuePaRuneCanHit,
            int? changeToRaRuneThreshold,
            bool highSinkStat) a) :
            this(
                a.maxValueSmRuneCanHit,
                a.changeToPaRuneThreshold,
                a.maxValuePaRuneCanHit,
                a.changeToRaRuneThreshold,
                a.highSinkStat) { }

        /**
         * <returns>A tuple of configuration options</returns>
         */
        public (int? maxValueSmRuneCanHit,
            int? changeToPaRuneThreshold,
            int? maxValuePaRuneCanHit,
            int? changeToRaRuneThreshold,
            bool highSinkStat)
            Deconstruct() => (
            MaxValueAtWhichSmRuneCanHit,
            ChangeToPaRuneThreshold,
            MaxValueAtWhichPaRuneCanHit,
            ChangeToRaRuneThreshold,
            HighSinkStat
        );
    }
}
