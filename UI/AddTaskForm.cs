using System;
using System.Drawing;
using System.Windows.Forms;
using SharuaTaskManager.Models;
using SharuaTaskManager.Services;

namespace SharuaTaskManager.UI
{
    public partial class AddTaskForm : Form
    {
        private TaskService _taskService;
        private TextBox _titleTextBox;
        private Button _todayButton;
        private Button _tomorrowButton;
        private DateTimePicker _customDatePicker;
        private Button _saveButton;
        private Button _cancelButton;
        private bool _isDarkMode;
        private DateTime? _selectedDate;

        public event EventHandler TaskAdded;

        public AddTaskForm(TaskService taskService)
        {
            _taskService = taskService;
            _isDarkMode = IsDarkMode();
            InitializeComponent();
            SetupUI();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            this.Text = "Add New Task";
            this.Size = new Size(600, 200);
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
            mainPanel.Padding = new Padding(20);

            // Title input - full width, no border like Notion
            _titleTextBox = new TextBox();
            _titleTextBox.Location = new Point(20, 20);
            _titleTextBox.Size = new Size(560, 40);
            _titleTextBox.Font = new Font("Segoe UI", 16);
            _titleTextBox.BackColor = Color.Transparent;
            _titleTextBox.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _titleTextBox.BorderStyle = BorderStyle.None;
            _titleTextBox.PlaceholderText = "What needs to be done?";
            _titleTextBox.TextChanged += TitleTextBox_TextChanged;

            // Add underline for title input
            var titleUnderline = new Panel();
            titleUnderline.Location = new Point(20, 60);
            titleUnderline.Size = new Size(560, 1);
            titleUnderline.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(200, 200, 200);

            // Date selection buttons
            _todayButton = new Button();
            _todayButton.Text = "Today";
            _todayButton.Size = new Size(80, 30);
            _todayButton.Location = new Point(20, 80);
            _todayButton.BackColor = _isDarkMode ? Color.FromArgb(50, 50, 50) : Color.FromArgb(240, 240, 240);
            _todayButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _todayButton.FlatStyle = FlatStyle.Flat;
            _todayButton.Font = new Font("Segoe UI", 9);
            _todayButton.FlatAppearance.BorderSize = 0;
            _todayButton.Click += (s, e) => SelectDate(DateTime.Today);

            _tomorrowButton = new Button();
            _tomorrowButton.Text = "Tomorrow";
            _tomorrowButton.Size = new Size(80, 30);
            _tomorrowButton.Location = new Point(110, 80);
            _tomorrowButton.BackColor = _isDarkMode ? Color.FromArgb(50, 50, 50) : Color.FromArgb(240, 240, 240);
            _tomorrowButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _tomorrowButton.FlatStyle = FlatStyle.Flat;
            _tomorrowButton.Font = new Font("Segoe UI", 9);
            _tomorrowButton.FlatAppearance.BorderSize = 0;
            _tomorrowButton.Click += (s, e) => SelectDate(DateTime.Today.AddDays(1));

            // Custom date picker
            _customDatePicker = new DateTimePicker();
            _customDatePicker.Location = new Point(200, 80);
            _customDatePicker.Size = new Size(120, 30);
            _customDatePicker.Font = new Font("Segoe UI", 9);
            _customDatePicker.Format = DateTimePickerFormat.Short;
            _customDatePicker.ValueChanged += (s, e) => SelectDate(_customDatePicker.Value.Date);

            // Save button - styled
            _saveButton = new Button();
            _saveButton.Text = "Add Task";
            _saveButton.Size = new Size(100, 35);
            _saveButton.Location = new Point(450, 80);
            _saveButton.BackColor = Color.FromArgb(76, 175, 80);
            _saveButton.ForeColor = Color.White;
            _saveButton.FlatStyle = FlatStyle.Flat;
            _saveButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _saveButton.FlatAppearance.BorderSize = 0;
            _saveButton.Click += SaveButton_Click;
            _saveButton.Enabled = false;

            // Cancel button
            _cancelButton = new Button();
            _cancelButton.Text = "Cancel";
            _cancelButton.Size = new Size(80, 35);
            _cancelButton.Location = new Point(340, 80);
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
            closeButton.Location = new Point(550, 10);
            closeButton.BackColor = Color.Transparent;
            closeButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => this.Close();

            mainPanel.Controls.Add(_titleTextBox);
            mainPanel.Controls.Add(titleUnderline);
            mainPanel.Controls.Add(_todayButton);
            mainPanel.Controls.Add(_tomorrowButton);
            mainPanel.Controls.Add(_customDatePicker);
            mainPanel.Controls.Add(_saveButton);
            mainPanel.Controls.Add(_cancelButton);
            mainPanel.Controls.Add(closeButton);

            this.Controls.Add(mainPanel);

            _titleTextBox.Focus();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_titleTextBox.Text))
            {
                MessageBox.Show("Please enter a task title.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var task = new Task();
            task.Title = _titleTextBox.Text.Trim();
            task.DueDate = _selectedDate;
            task.IsInBacklog = !_selectedDate.HasValue; // If no date selected, goes to backlog

            _taskService.AddTask(task);
            if (TaskAdded != null)
                TaskAdded(this, EventArgs.Empty);
            this.Close();
        }

        private void TitleTextBox_TextChanged(object sender, EventArgs e)
        {
            _saveButton.Enabled = !string.IsNullOrWhiteSpace(_titleTextBox.Text);
        }

        private void SelectDate(DateTime date)
        {
            _selectedDate = date;
            UpdateDateButtons();
        }

        private void UpdateDateButtons()
        {
            // Reset all buttons
            _todayButton.BackColor = _isDarkMode ? Color.FromArgb(50, 50, 50) : Color.FromArgb(240, 240, 240);
            _tomorrowButton.BackColor = _isDarkMode ? Color.FromArgb(50, 50, 50) : Color.FromArgb(240, 240, 240);
            _customDatePicker.BackColor = _isDarkMode ? Color.FromArgb(50, 50, 50) : Color.FromArgb(240, 240, 240);

            if (_selectedDate.HasValue)
            {
                if (_selectedDate.Value.Date == DateTime.Today)
                {
                    _todayButton.BackColor = Color.FromArgb(76, 175, 80);
                    _todayButton.ForeColor = Color.White;
                }
                else if (_selectedDate.Value.Date == DateTime.Today.AddDays(1))
                {
                    _tomorrowButton.BackColor = Color.FromArgb(76, 175, 80);
                    _tomorrowButton.ForeColor = Color.White;
                }
                else
                {
                    _customDatePicker.BackColor = Color.FromArgb(76, 175, 80);
                    _customDatePicker.ForeColor = Color.White;
                }
            }
        }

        private bool IsDarkMode()
        {
            try
            {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
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