using System.Windows.Forms;
using WindowsFormsApp.Services;

namespace WindowsFormsApp.Domain
{
    public class UserSubscriptionFormEnforcer
    {
        private ApiDataProvider dataProvider;
        private Form form;

        public UserSubscriptionFormEnforcer(Form form)
        {
            this.form = form;
        }
    }
}