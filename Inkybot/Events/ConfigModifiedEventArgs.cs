using System;
using Inkybot.Domain;

namespace Inkybot.Events
{
    public class ConfigModifiedEventArgs : EventArgs
    {
        public readonly Config Config;
        public readonly bool Changed;
        public readonly bool StructureChanged;

        public ConfigModifiedEventArgs(Config config, bool changed, bool structureChanged) {
            this.Config = config;
            this.Changed = changed;
            this.StructureChanged = structureChanged;
        }
    }
}
