using System;

namespace Inkybot.Events
{
    public class ConfigChangedEventArgs : EventArgs
    {
        public readonly ItemConfig ItemConfig;
        public readonly bool StructureChanged;

        public ConfigChangedEventArgs(ItemConfig itemConfig, bool structureChanged) {
            this.ItemConfig = itemConfig;
            this.StructureChanged = structureChanged;
        }
    }
}
