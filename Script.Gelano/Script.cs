using Inkybot.Dofus;
using Inkybot.Dofus.Domain;
using Inkybot.Dofus.Contracts;
using System.Windows.Forms;

namespace Script.Gelano
{
    public class Script : DofusMagingAI
    {
        public override void Init() {
            MessageBox.Show("Script loaded successfully.");
        }

        public override IAction ResolveAction(Item item) {
            throw new System.NotImplementedException();
        }
    }
}
