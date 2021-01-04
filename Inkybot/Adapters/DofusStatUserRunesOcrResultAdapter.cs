using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;
using Inkybot.Domain;
using RuneQuantity = Inkybot.Services.ScreenReaderDataProvider.RuneQuantityScan;

namespace Inkybot.Adapters
{
    public class DofusStatUserRunesOcrResultAdapter : DofusOcrResultAdapter
    {
        private readonly Item Item;
        private readonly RuneQuantity[] RuneQuantities;

        public DofusStatUserRunesOcrResultAdapter(Item item, RuneQuantity[] runeQuantities) {
            this.Item = item;
            this.RuneQuantities = runeQuantities;
        }

        public IEnumerable<KeyValuePair<Stat, UserRune[]>> ToUserRunes() {
            var runeQuantitiesPerStat = RuneQuantities.GroupBy(scan => scan.Row);

            var statsRuneQuantities = Item.Stats.Zip(runeQuantitiesPerStat, (itemStat, runeQuantities) => {
                var statRuneQuantities = new UserRune[3];

                var smQuantity = runeQuantities.First();
                statRuneQuantities[0] = new UserRune(new Rune(itemStat.stat, Rune.Type.Sm), smQuantity.Quantity);

                var paQuantity = runeQuantities.Skip(1).First();
                statRuneQuantities[1] = new UserRune(new Rune(itemStat.stat, Rune.Type.Pa), paQuantity.Quantity);

                var raQuantity = runeQuantities.Skip(2).First();
                statRuneQuantities[2] = new UserRune(new Rune(itemStat.stat, Rune.Type.Ra), raQuantity.Quantity);

                return new KeyValuePair<Stat, UserRune[]>(itemStat.stat, statRuneQuantities);
            });

            return statsRuneQuantities;
        }
    }
}
