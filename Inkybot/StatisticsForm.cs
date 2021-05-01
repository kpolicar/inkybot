using System;
using System.ComponentModel;
using System.Diagnostics;
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
            apiClient = Program.Services.GetService<ApiClient>();
        }

        private void OnVisibleChanged(object sender, EventArgs e) {
            if (apiClient.Connection == null)
                return;
            
            webBrowser.Navigate(
                Server.StatisticsViewUrl, 
                "",
                new byte[]{},
                "Authorization: Bearer "+apiClient.Connection.AuthDetails.access_token);
        }

        private void StatisticsForm_Closing(object sender, CancelEventArgs cancelEventArgs) {
            cancelEventArgs.Cancel = true;
            Hide();
        }

        private void openInBrowserLabelLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start($"{Server.BaseUrl}/profile#statistics");
        }
    }
}

