using System;

namespace Inkybot.Events
{
    public class ConfigModifiedEventArgs : EventArgs
    {
        public readonly ItemConfig ItemConfig;
        public readonly bool Changed;
        public readonly bool StructureChanged;

        public ConfigModifiedEventArgs(ItemConfig itemConfig, bool changed, bool structureChanged) {
            this.ItemConfig = itemConfig;
            this.Changed = changed;
            this.StructureChanged = structureChanged;
        }
    }
}
