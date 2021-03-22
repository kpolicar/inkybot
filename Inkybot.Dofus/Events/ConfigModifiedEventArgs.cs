using System;
using Inkybot.Dofus;

namespace Inkybot.Events
{
    public class ConfigModifiedEventArgs : EventArgs
    {
        public readonly MageConfig Config;
        public readonly bool Changed;
        public readonly bool StructureChanged;

        public ConfigModifiedEventArgs(MageConfig config, bool changed, bool structureChanged) {
            this.Config = config;
            this.Changed = changed;
            this.StructureChanged = structureChanged;
        }
    }
}
