using Windows.UI.Xaml.Controls;

namespace WindowsFormsApp.Actions
{
    public class SelectRune : MouseAction
    {
        public Item.ItemStat Target { get; set; }
        private Rune rune;

        public SelectRune(Rune rune) {
            this.rune = rune;
        }
        
        public override void Execute() {
            var itemStats = screenDataProvider.lastScanResults;
            
            for (int row = 0; row < itemStats.Length; row++) {
                if (rune.stat.DisplayName != itemStats[row].stat.DisplayName)
                    continue;
                command.SelectRune(row, (int)rune.type);
            }
        }
    }
}