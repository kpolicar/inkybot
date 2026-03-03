namespace Inkybot.Contracts
{
    public interface IMagingDataProvider : DofusDataProvider
    {
        decimal? Sink();
        void Reset(bool resetMinMaxScan = true);
        void ResetMinMaxScan();
        void PrefetchForHistoryCheck();
        void SaveScan();
    }
}
