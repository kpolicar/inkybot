using Inkybot.Dofus.Contracts;

namespace Tests.Services
{
    public class MageConfigProviderMock : MageConfigProvider
    {
        public bool RestoreHighSinkStatsImmediately { get; set; } = true;
    }
}
