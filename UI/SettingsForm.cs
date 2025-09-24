using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using SharuaTaskManager.Services;

namespace SharuaTaskManager.UI
{
    public partial class SettingsForm : Form
    {
        private CheckBox _autoStartCheckBox;
        private ComboBox _themeComboBox;
        private TextBox _addTaskHotkeyTextBox;
        private TextBox _toggleViewHotkeyTextBox;
        private TextBox _showMainHotkeyTextBox;
        private Button _saveButton;
        private Button _cancelButton;
        private bool _isDarkMode;

        public event EventHandler SettingsChanged;

        public SettingsForm()
        {
            _isDarkMode = IsDarkMode();
            InitializeComponent();
            SetupUI();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            this.Text = "Settings";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = _isDarkMode ? Color.FromArgb(30, 30, 30) : Color.FromArgb(250, 250, 250);
            this.TopMost = true;
            this.Opacity = 0.95;
            
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            var mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.Transparent;
            mainPanel.Padding = new Padding(30);

            var titleLabel = new Label();
            titleLabel.Text = "Settings";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = _isDarkMode ? Color.White : Color.Black;
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(30, 30);

            // Auto-start section
            var autoStartLabel = new Label();
            autoStartLabel.Text = "Auto-start with Windows:";
            autoStartLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            autoStartLabel.ForeColor = _isDarkMode ? Color.White : Color.Black;
            autoStartLabel.AutoSize = true;
            autoStartLabel.Location = new Point(30, 80);

            _autoStartCheckBox = new CheckBox();
            _autoStartCheckBox.Text = "Start Sharua Task Manager when Windows starts";
            _autoStartCheckBox.Font = new Font("Segoe UI", 9);
            _autoStartCheckBox.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _autoStartCheckBox.Location = new Point(30, 105);
            _autoStartCheckBox.AutoSize = true;

            // Theme section
            var themeLabel = new Label();
            themeLabel.Text = "Theme:";
            themeLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            themeLabel.ForeColor = _isDarkMode ? Color.White : Color.Black;
            themeLabel.AutoSize = true;
            themeLabel.Location = new Point(30, 140);

            _themeComboBox = new ComboBox();
            _themeComboBox.Location = new Point(30, 165);
            _themeComboBox.Size = new Size(200, 25);
            _themeComboBox.Font = new Font("Segoe UI", 9);
            _themeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _themeComboBox.BackColor = _isDarkMode ? Color.FromArgb(40, 40, 40) : Color.White;
            _themeComboBox.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _themeComboBox.Items.AddRange(new object[] { "Auto", "Light", "Dark" });

            // Hotkeys section
            var hotkeysLabel = new Label();
            hotkeysLabel.Text = "Hotkeys:";
            hotkeysLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            hotkeysLabel.ForeColor = _isDarkMode ? Color.White : Color.Black;
            hotkeysLabel.AutoSize = true;
            hotkeysLabel.Location = new Point(30, 200);

            var addTaskLabel = new Label();
            addTaskLabel.Text = "Add Task:";
            addTaskLabel.Font = new Font("Segoe UI", 9);
            addTaskLabel.ForeColor = _isDarkMode ? Color.White : Color.Black;
            addTaskLabel.AutoSize = true;
            addTaskLabel.Location = new Point(30, 225);

            _addTaskHotkeyTextBox = new TextBox();
            _addTaskHotkeyTextBox.Location = new Point(100, 225);
            _addTaskHotkeyTextBox.Size = new Size(150, 20);
            _addTaskHotkeyTextBox.Font = new Font("Segoe UI", 9);
            _addTaskHotkeyTextBox.BackColor = _isDarkMode ? Color.FromArgb(40, 40, 40) : Color.White;
            _addTaskHotkeyTextBox.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _addTaskHotkeyTextBox.ReadOnly = true;
            _addTaskHotkeyTextBox.KeyDown += AddTaskHotkeyTextBox_KeyDown;

            var toggleViewLabel = new Label();
            toggleViewLabel.Text = "Toggle View:";
            toggleViewLabel.Font = new Font("Segoe UI", 9);
            toggleViewLabel.ForeColor = _isDarkMode ? Color.White : Color.Black;
            toggleViewLabel.AutoSize = true;
            toggleViewLabel.Location = new Point(30, 250);

            _toggleViewHotkeyTextBox = new TextBox();
            _toggleViewHotkeyTextBox.Location = new Point(100, 250);
            _toggleViewHotkeyTextBox.Size = new Size(150, 20);
            _toggleViewHotkeyTextBox.Font = new Font("Segoe UI", 9);
            _toggleViewHotkeyTextBox.BackColor = _isDarkMode ? Color.FromArgb(40, 40, 40) : Color.White;
            _toggleViewHotkeyTextBox.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _toggleViewHotkeyTextBox.ReadOnly = true;
            _toggleViewHotkeyTextBox.KeyDown += ToggleViewHotkeyTextBox_KeyDown;

            var showMainLabel = new Label();
            showMainLabel.Text = "Show Main:";
            showMainLabel.Font = new Font("Segoe UI", 9);
            showMainLabel.ForeColor = _isDarkMode ? Color.White : Color.Black;
            showMainLabel.AutoSize = true;
            showMainLabel.Location = new Point(30, 275);

            _showMainHotkeyTextBox = new TextBox();
            _showMainHotkeyTextBox.Location = new Point(100, 275);
            _showMainHotkeyTextBox.Size = new Size(150, 20);
            _showMainHotkeyTextBox.Font = new Font("Segoe UI", 9);
            _showMainHotkeyTextBox.BackColor = _isDarkMode ? Color.FromArgb(40, 40, 40) : Color.White;
            _showMainHotkeyTextBox.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _showMainHotkeyTextBox.ReadOnly = true;
            _showMainHotkeyTextBox.KeyDown += ShowMainHotkeyTextBox_KeyDown;

            // Buttons
            _saveButton = new Button();
            _saveButton.Text = "Save";
            _saveButton.Size = new Size(80, 30);
            _saveButton.Location = new Point(300, 320);
            _saveButton.BackColor = Color.FromArgb(76, 175, 80);
            _saveButton.ForeColor = Color.White;
            _saveButton.FlatStyle = FlatStyle.Flat;
            _saveButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _saveButton.FlatAppearance.BorderSize = 0;
            _saveButton.Click += SaveButton_Click;

            _cancelButton = new Button();
            _cancelButton.Text = "Cancel";
            _cancelButton.Size = new Size(80, 30);
            _cancelButton.Location = new Point(390, 320);
            _cancelButton.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(200, 200, 200);
            _cancelButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _cancelButton.FlatStyle = FlatStyle.Flat;
            _cancelButton.Font = new Font("Segoe UI", 10);
            _cancelButton.FlatAppearance.BorderSize = 0;
            _cancelButton.Click += (s, e) => this.Close();

            // Close button
            var closeButton = new Button();
            closeButton.Text = "×";
            closeButton.Size = new Size(30, 30);
            closeButton.Location = new Point(450, 10);
            closeButton.BackColor = Color.Transparent;
            closeButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => this.Close();

            mainPanel.Controls.Add(titleLabel);
            mainPanel.Controls.Add(autoStartLabel);
            mainPanel.Controls.Add(_autoStartCheckBox);
            mainPanel.Controls.Add(themeLabel);
            mainPanel.Controls.Add(_themeComboBox);
            mainPanel.Controls.Add(hotkeysLabel);
            mainPanel.Controls.Add(addTaskLabel);
            mainPanel.Controls.Add(_addTaskHotkeyTextBox);
            mainPanel.Controls.Add(toggleViewLabel);
            mainPanel.Controls.Add(_toggleViewHotkeyTextBox);
            mainPanel.Controls.Add(showMainLabel);
            mainPanel.Controls.Add(_showMainHotkeyTextBox);
            mainPanel.Controls.Add(_saveButton);
            mainPanel.Controls.Add(_cancelButton);
            mainPanel.Controls.Add(closeButton);

            this.Controls.Add(mainPanel);
        }

        private void LoadSettings()
        {
            var settingsService = new Services.SettingsService();
            
            // Load auto-start setting
            _autoStartCheckBox.Checked = settingsService.AutoStart;

            // Load theme setting
            _themeComboBox.SelectedIndex = settingsService.Theme;

            // Load hotkeys
            _addTaskHotkeyTextBox.Text = settingsService.AddTaskHotkey;
            _toggleViewHotkeyTextBox.Text = settingsService.ToggleViewHotkey;
            _showMainHotkeyTextBox.Text = settingsService.ShowMainHotkey;
        }

        private void AddTaskHotkeyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var keys = new System.Collections.Generic.List<string>();
            if (e.Control) keys.Add("Ctrl");
            if (e.Alt) keys.Add("Alt");
            if (e.Shift) keys.Add("Shift");
            keys.Add(e.KeyCode.ToString());
            
            _addTaskHotkeyTextBox.Text = string.Join("+", keys);
            e.Handled = true;
        }

        private void ToggleViewHotkeyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var keys = new System.Collections.Generic.List<string>();
            if (e.Control) keys.Add("Ctrl");
            if (e.Alt) keys.Add("Alt");
            if (e.Shift) keys.Add("Shift");
            keys.Add(e.KeyCode.ToString());
            
            _toggleViewHotkeyTextBox.Text = string.Join("+", keys);
            e.Handled = true;
        }

        private void ShowMainHotkeyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var keys = new System.Collections.Generic.List<string>();
            if (e.Control) keys.Add("Ctrl");
            if (e.Alt) keys.Add("Alt");
            if (e.Shift) keys.Add("Shift");
            keys.Add(e.KeyCode.ToString());
            
            _showMainHotkeyTextBox.Text = string.Join("+", keys);
            e.Handled = true;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                var settingsService = new Services.SettingsService();
                
                // Update settings
                settingsService.AutoStart = _autoStartCheckBox.Checked;
                settingsService.Theme = _themeComboBox.SelectedIndex;
                settingsService.AddTaskHotkey = _addTaskHotkeyTextBox.Text;
                settingsService.ToggleViewHotkey = _toggleViewHotkeyTextBox.Text;
                settingsService.ShowMainHotkey = _showMainHotkeyTextBox.Text;
                
                // Save settings
                settingsService.SaveSettings();
                
                if (SettingsChanged != null)
                    SettingsChanged(this, EventArgs.Empty);

                MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving settings: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsDarkMode()
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var value = key != null ? key.GetValue("AppsUseLightTheme") : null;
                return value != null && value.ToString() == "0";
            }
            catch
            {
                return false;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                const int WM_NCLBUTTONDOWN = 0xA1;
                const int HT_CAPTION = 0x2;
                var msg = Message.Create(this.Handle, WM_NCLBUTTONDOWN, new IntPtr(HT_CAPTION), IntPtr.Zero);
                this.WndProc(ref msg);
            }
            base.OnMouseDown(e);
        }
    }
}
