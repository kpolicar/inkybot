using System;

namespace Inkybot.Dofus
{
    public readonly struct StatConfig
    {
        public static StatConfig None = new StatConfig();
        public readonly int? MaxValueAtWhichSmRuneCanHit;
        public readonly int? ChangeToPaRuneThreshold;
        public readonly int? MaxValueAtWhichPaRuneCanHit;
        public readonly int? ChangeToRaRuneThreshold;
        public readonly bool HighSinkStat;
            
        public StatConfig(
            int? maxValueSmRuneCanHit=null,
            int? changeToPaRuneThreshold=null,
            int? maxValuePaRuneCanHit=null,
            int? changeToRaRuneThreshold=null,
            bool highSinkStat=false) =>
            (MaxValueAtWhichSmRuneCanHit, ChangeToPaRuneThreshold, MaxValueAtWhichPaRuneCanHit, ChangeToRaRuneThreshold, HighSinkStat) =
            (maxValueSmRuneCanHit, changeToPaRuneThreshold, maxValuePaRuneCanHit, changeToRaRuneThreshold, highSinkStat);

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
