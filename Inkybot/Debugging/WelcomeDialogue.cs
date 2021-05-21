using System;
using System.Windows.Forms;

#if DEBUG
namespace Inkybot.Debugging
{
    public class WelcomeDialogue
    {
        public static void Bind(Inkybot.WelcomeDialogue dialogue) {
            dialogue.Load += (sender, args) => {

                //Helpers.Debug.GetFieldValue<TextBox>(dialogue, "usernameTextBox").Text = "naltamer14@gmail.com";
                //Helpers.Debug.GetFieldValue<TextBox>(dialogue, "passwordTextBox").Text = "***REMOVED***";

                //Helpers.Debug.Call(dialogue,
                //    "button1_Click",
                //    new object[] {dialogue, EventArgs.Empty});
            };
        }
    }
}
#endif
