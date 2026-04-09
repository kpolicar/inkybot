using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using Inkybot.Resources;
using Inkybot.Services;

namespace Inkybot
{
    public partial class CustomizeControl : UserControl
    {
        private const int MaxScriptTabs = 10;
        private readonly CustomizeApiClient apiClient;
        private readonly FileSystemUserSettingsConfigManager settingsManager;
        private readonly MagingAIServiceManager magingAiManager;
        private readonly List<ChatMessage> chatHistory = new List<ChatMessage>();
        private int nextScriptNumber = 1;
        private int? appliedScriptNumber;

        public CustomizeControl() {
            InitializeComponent();
            apiClient = new CustomizeApiClient();
            settingsManager = (FileSystemUserSettingsConfigManager)
                Program.Services.GetService<Contracts.UserSettingsConfigManager>();
            magingAiManager = Program.Services.GetService<MagingAIServiceManager>();

            LoadPersistedScripts();
            UpdateStatusIndicator();

            // Set splitter distance once the control is sized (avoids InvalidOperationException during init)
            this.Layout += (s, e) => {
                if (splitContainer.Width > 0 && splitContainer.SplitterDistance < 10) {
                    splitContainer.SplitterDistance = splitContainer.Width / 2;
                }
            };
        }

        private void sendButton_Click(object sender, EventArgs e) {
            var message = chatInputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;
            chatInputTextBox.Text = "";
            _ = SendMessage(message);
        }

        private void chatInputTextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter && !e.Shift) {
                e.SuppressKeyPress = true;
                sendButton_Click(sender, e);
            }
        }

        private async Task SendMessage(string userMessage) {
            AppendChatMessage("You", userMessage);
            chatHistory.Add(new ChatMessage { Role = "user", Content = userMessage });

            sendButton.Enabled = false;
            chatInputTextBox.Enabled = false;

            try {
                var currentCode = GetActiveCodeEditor()?.Text ?? "";
                var response = await apiClient.GenerateScript(userMessage, currentCode, chatHistory);

                chatHistory.Add(new ChatMessage { Role = "assistant", Content = response.Message });
                AppendChatMessage("Assistant", response.Message);

                AddScriptTab(response.Code, userMessage);
            } catch (Exception ex) {
                AppendChatMessage("Error", ex.Message);
            } finally {
                sendButton.Enabled = true;
                chatInputTextBox.Enabled = true;
                chatInputTextBox.Focus();
            }
        }

        private void AppendChatMessage(string sender, string message) {
            if (chatHistoryTextBox.InvokeRequired) {
                chatHistoryTextBox.Invoke(new MethodInvoker(() => AppendChatMessage(sender, message)));
                return;
            }

            var startPos = chatHistoryTextBox.TextLength;
            chatHistoryTextBox.AppendText(sender + ": ");
            chatHistoryTextBox.Select(startPos, sender.Length + 2);
            chatHistoryTextBox.SelectionFont = new Font(chatHistoryTextBox.Font, FontStyle.Bold);
            chatHistoryTextBox.SelectionColor = sender == "You"
                ? Color.FromArgb(100, 180, 255)
                : sender == "Error"
                    ? Color.FromArgb(255, 100, 100)
                    : Color.FromArgb(100, 255, 100);

            chatHistoryTextBox.AppendText(message + "\n\n");
            chatHistoryTextBox.ScrollToCaret();
        }

        private void AddScriptTab(string code, string firstPrompt) {
            // Enforce max tabs
            while (scriptTabControl.TabPages.Count >= MaxScriptTabs) {
                var oldest = scriptTabControl.TabPages[0];
                scriptTabControl.TabPages.RemoveAt(0);
                oldest.Dispose();
            }

            var tabPage = new TabPage($"\u2b50{nextScriptNumber}");
            tabPage.ToolTipText = firstPrompt.Length > 60
                ? firstPrompt.Substring(0, 60) + "..."
                : firstPrompt;
            tabPage.Tag = nextScriptNumber;
            tabPage.BackColor = Color.FromArgb(30, 30, 30);

            var editor = CreateCodeEditor();
            editor.Text = code;
            tabPage.Controls.Add(editor);

            scriptTabControl.TabPages.Add(tabPage);
            scriptTabControl.SelectedTab = tabPage;

            nextScriptNumber++;
            PersistScripts();
            UpdateStatusIndicator();
        }

        private FastColoredTextBox CreateCodeEditor() {
            var editor = new FastColoredTextBox {
                Dock = DockStyle.Fill,
                Language = Language.CSharp,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(220, 220, 220),
                IndentBackColor = Color.FromArgb(25, 25, 25),
                LineNumberColor = Color.FromArgb(90, 90, 90),
                CaretColor = Color.White,
                SelectionColor = Color.FromArgb(60, 60, 100),
                CurrentLineColor = Color.FromArgb(40, 40, 40),
                Font = new Font("Consolas", 10f),
                ShowLineNumbers = true,
                TabLength = 4,
                AutoIndent = true,
            };
            editor.TextChanged += (s, e) => PersistScripts();
            return editor;
        }

        private FastColoredTextBox GetActiveCodeEditor() {
            if (scriptTabControl.SelectedTab == null) return null;
            return scriptTabControl.SelectedTab.Controls.Count > 0
                ? scriptTabControl.SelectedTab.Controls[0] as FastColoredTextBox
                : null;
        }

        private void applyButton_Click(object sender, EventArgs e) {
            var editor = GetActiveCodeEditor();
            if (editor == null || string.IsNullOrWhiteSpace(editor.Text)) return;

            var code = editor.Text;
            var tempPath = Path.Combine(Path.GetTempPath(), "inkybot_custom_script.cs");
            File.WriteAllText(tempPath, code);

            try {
                magingAiManager.UseCustomAIScript(tempPath);
                appliedScriptNumber = (int?) scriptTabControl.SelectedTab?.Tag;
                UpdateStatusIndicator();
                PersistScripts();
                AppendChatMessage("System", "Script applied successfully.");
            } catch (Exception ex) {
                AppendChatMessage("Error", "Compilation failed: " + ex.Message + "\nYou can ask me to fix this.");
            }
        }

        private void saveButton_Click(object sender, EventArgs e) {
            var editor = GetActiveCodeEditor();
            if (editor == null || string.IsNullOrWhiteSpace(editor.Text)) return;

            using (var dialog = new SaveFileDialog()) {
                dialog.Filter = "C# Script (*.cs)|*.cs";
                dialog.DefaultExt = "cs";
                dialog.FileName = "CustomScript.cs";
                if (dialog.ShowDialog() == DialogResult.OK) {
                    File.WriteAllText(dialog.FileName, editor.Text);
                }
            }
        }

        private void scriptTabControl_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateStatusIndicator();
        }

        private void UpdateStatusIndicator() {
            if (scriptTabControl.SelectedTab == null) {
                statusLabel.Text = "";
                return;
            }

            var tabNumber = (int?) scriptTabControl.SelectedTab.Tag;
            var isApplied = appliedScriptNumber != null && tabNumber == appliedScriptNumber;
            statusLabel.Text = isApplied ? "\u25cf Applied" : "\u25cf Not Applied";
            statusLabel.ForeColor = isApplied
                ? Color.FromArgb(80, 200, 80)
                : Color.FromArgb(140, 140, 140);
        }

        private void PersistScripts() {
            var scripts = new List<GeneratedScript>();
            foreach (TabPage tab in scriptTabControl.TabPages) {
                var editor = tab.Controls.Count > 0 ? tab.Controls[0] as FastColoredTextBox : null;
                if (editor == null) continue;
                scripts.Add(new GeneratedScript {
                    Number = (int) tab.Tag,
                    Code = editor.Text,
                    FirstPrompt = tab.ToolTipText ?? "",
                    IsApplied = appliedScriptNumber != null && (int) tab.Tag == appliedScriptNumber
                });
            }

            settingsManager.GeneratedScripts = new GeneratedScripts {
                Scripts = scripts.ToArray()
            };
        }

        private void LoadPersistedScripts() {
            var saved = settingsManager.GeneratedScripts;
            if (saved?.Scripts == null || saved.Scripts.Length == 0) return;

            foreach (var script in saved.Scripts) {
                var tabPage = new TabPage($"\u2b50{script.Number}");
                tabPage.ToolTipText = script.FirstPrompt;
                tabPage.Tag = script.Number;
                tabPage.BackColor = Color.FromArgb(30, 30, 30);

                var editor = CreateCodeEditor();
                editor.Text = script.Code;
                tabPage.Controls.Add(editor);

                scriptTabControl.TabPages.Add(tabPage);

                if (script.IsApplied)
                    appliedScriptNumber = script.Number;

                if (script.Number >= nextScriptNumber)
                    nextScriptNumber = script.Number + 1;
            }
        }
    }
}
