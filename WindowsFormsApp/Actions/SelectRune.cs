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
            var column = (int) rune.type;
            
            for (int row = 0; row < itemStats.Length; row++) {
                if (rune.stat.DisplayName != itemStats[row].stat.DisplayName)
                    continue;

                mouse.DoubleClick(1065 + column*55, 320 + row*39);
            }
        }
    }
}