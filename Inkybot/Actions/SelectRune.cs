using System.Threading;

namespace Inkybot.Actions
{
    public class SelectRune : MouseAction
    {
        private readonly Rune rune;

        public SelectRune(Rune rune) {
            this.rune = rune;
        }

        public Item.ItemStat Target { get; set; }

        public override void Execute() {
            var itemStats = screenDataProvider.lastScanResults;
            var column = (int) rune.type;

            for (var row = 0; row < itemStats.Length; row++) {
                if (rune.stat.DisplayName != itemStats[row].stat.DisplayName)
                    continue;

                mouse.CtrlDoubleClick(1065 + column * 55, 320 + row * 39);
                System.Diagnostics.Debug.WriteLine($"Rune changed to {rune.stat.DisplayName}");
            }
        }
    }
}
