using System;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Domain;

namespace Inkybot
{
    public partial class StatisticsForm : Form
    {
        private ApiClient apiClient;
        
        
        public StatisticsForm() {
            InitializeComponent();
            VisibleChanged += OnVisibleChanged;
            webBrowser.Url = new Uri(Server.StatisticsViewUrl, UriKind.Absolute);
            apiClient = Program.Services.GetService<ApiClient>();
        }

        private void OnVisibleChanged(object sender, EventArgs e) {
            if (apiClient.Connection == null)
                return;
            
            webBrowser.Navigate(
                Server.StatisticsViewUrl, 
                "",
                new byte[]{},
                "Authorization : Bearer "+apiClient.Connection.AuthDetails);
        }
    }
}

