using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Services;
using Inkybot.Domain;
using Inkybot.Events;

namespace Inkybot
{
    public partial class StatisticsForm : Form
    {
        private ApiClient apiClient;
        private bool init = false;

        private string Header => "Authorization: Bearer " + ApiConnection.AuthDetails.access_token ?? "";
        private string LocalizedUrl => Server.StatisticsViewUrl + "?locale=" + Program.Lang.TwoLetterISOLanguageName;
        
        public StatisticsForm() {
            InitializeComponent();
            webBrowser.Navigating += OnWebBrowserNavigating;
            VisibleChanged += OnVisibleChanged;
            apiClient = Program.Services.GetService<ApiClient>();
            var api = Program.Services.GetService<ApiClient>();
            api.UserFetched += OnUserFetched;
        }

        private void OnUserFetched(object sender, FetchedUserEventArgs e) {
            if (!e.user.canViewStatistics && Visible)
                Hide();
        }

        private void OnWebBrowserNavigating(object sender, WebBrowserNavigatingEventArgs e) {
            try {

                var headers = "headers=1";
                if (e.Url.ToString().Contains(headers))
                    return;

                var url = e.Url;
                var newUrl = url + (url.ToString().Contains("?") ? "&" : "?") + headers;
                if (!newUrl.Contains("locale"))
                    newUrl += "&locale=" + Program.Lang.TwoLetterISOLanguageName;

                if (url.ToString().StartsWith(Server.StatisticsNewSessionUrl)) {
                    System.Text.Encoding encoding = System.Text.Encoding.UTF8;
                    var bytes = encoding.GetBytes("_method=POST");


                    webBrowser.Navigate(newUrl, null, bytes, Header);
                } else {
                    webBrowser.Navigate(newUrl, null, new byte[] { }, Header);
                }

                e.Cancel = true;
            } catch (Exception) {
                
            }
        }

        private void OnVisibleChanged(object sender, EventArgs e) {
            if (apiClient.Connection == null || !Visible)
                return;
            try {
                webBrowser.Navigate(LocalizedUrl, "", new byte[] { }, Header);
            } catch (Exception) {
            }
        }

        private void StatisticsForm_Closing(object sender, CancelEventArgs cancelEventArgs) {
            cancelEventArgs.Cancel = true;
            Hide();
        }

        private void openInBrowserLabelLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start($"{Server.BaseUrl}/profile#statistics");
        }

        private void refreshButton_Click(object sender, EventArgs e) {
            MetricsLogger.Track("ui_statistics_refreshed");
            try {
                webBrowser.Navigate(LocalizedUrl, "", new byte[] { }, Header);
            } catch (Exception) {
            }
        }

        private void OnRefreshButtonPaint(object sender, PaintEventArgs e) {
            base.OnPaint(e);
            var format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            var rect = refreshButton.ClientRectangle;
            rect.Height -= 4;
            e.Graphics.DrawString(
                "⟲",
                refreshButton.Font,
                new SolidBrush(refreshButton.ForeColor),
                rect,
                format);
        }
    }
}

