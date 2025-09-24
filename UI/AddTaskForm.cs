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
        private SettingsService _settingsService;
        private RichTextBox _titleTextBox;
        private RichTextBox _contentTextBox;
        private Panel _toolbarPanel;
        private Button _todayButton;
        private Button _tomorrowButton;
        private DateTimePicker _customDatePicker;
        private ComboBox _priorityComboBox;
        private Button _saveButton;
        private Button _cancelButton;
        private bool _isDarkMode;
        private DateTime? _selectedDate;
        private TaskPriority _selectedPriority;

        public event EventHandler<Models.Task> TaskAdded;

        public AddTaskForm(TaskService taskService, SettingsService settingsService)
        {
            _taskService = taskService;
            _settingsService = settingsService;
            _isDarkMode = _settingsService.IsDarkMode();
            InitializeComponent();
            SetupUI();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            this.Text = "New Task";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = _isDarkMode ? Color.FromArgb(32, 32, 32) : Color.FromArgb(255, 255, 255);
            this.TopMost = true;
            this.Opacity = 1.0;
            this.ShowInTaskbar = false;
            
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            var mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = _isDarkMode ? Color.FromArgb(32, 32, 32) : Color.FromArgb(255, 255, 255);
            mainPanel.Padding = new Padding(40, 60, 40, 40);

            // Title input - Notion style
            _titleTextBox = new RichTextBox();
            _titleTextBox.Location = new Point(40, 60);
            _titleTextBox.Size = new Size(720, 60);
            _titleTextBox.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            _titleTextBox.BackColor = _isDarkMode ? Color.FromArgb(32, 32, 32) : Color.FromArgb(255, 255, 255);
            _titleTextBox.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _titleTextBox.BorderStyle = BorderStyle.None;
            _titleTextBox.Multiline = true;
            _titleTextBox.ScrollBars = RichTextBoxScrollBars.None;
            _titleTextBox.Text = "Untitled";
            _titleTextBox.SelectionStart = 0;
            _titleTextBox.SelectionLength = _titleTextBox.Text.Length;
            _titleTextBox.GotFocus += TitleTextBox_GotFocus;
            _titleTextBox.LostFocus += TitleTextBox_LostFocus;
            _titleTextBox.TextChanged += TitleTextBox_TextChanged;
            _titleTextBox.KeyDown += TitleTextBox_KeyDown;

            // Content area - Notion style
            _contentTextBox = new RichTextBox();
            _contentTextBox.Location = new Point(40, 140);
            _contentTextBox.Size = new Size(720, 300);
            _contentTextBox.Font = new Font("Segoe UI", 14);
            _contentTextBox.BackColor = _isDarkMode ? Color.FromArgb(32, 32, 32) : Color.FromArgb(255, 255, 255);
            _contentTextBox.ForeColor = _isDarkMode ? Color.FromArgb(200, 200, 200) : Color.FromArgb(100, 100, 100);
            _contentTextBox.BorderStyle = BorderStyle.None;
            _contentTextBox.Multiline = true;
            _contentTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            _contentTextBox.Text = "Start writing...";
            _contentTextBox.GotFocus += ContentTextBox_GotFocus;
            _contentTextBox.LostFocus += ContentTextBox_LostFocus;
            _contentTextBox.TextChanged += ContentTextBox_TextChanged;

            // Toolbar panel
            _toolbarPanel = new Panel();
            _toolbarPanel.Location = new Point(40, 460);
            _toolbarPanel.Size = new Size(720, 50);
            _toolbarPanel.BackColor = _isDarkMode ? Color.FromArgb(40, 40, 40) : Color.FromArgb(248, 248, 248);
            _toolbarPanel.Padding = new Padding(10);

            // Date selection buttons
            _todayButton = new Button();
            _todayButton.Text = "Today";
            _todayButton.Size = new Size(80, 32);
            _todayButton.Location = new Point(10, 9);
            _todayButton.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(240, 240, 240);
            _todayButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _todayButton.FlatStyle = FlatStyle.Flat;
            _todayButton.Font = new Font("Segoe UI", 9);
            _todayButton.FlatAppearance.BorderSize = 0;
            _todayButton.Click += (s, e) => SelectDate(DateTime.Today);

            _tomorrowButton = new Button();
            _tomorrowButton.Text = "Tomorrow";
            _tomorrowButton.Size = new Size(80, 32);
            _tomorrowButton.Location = new Point(100, 9);
            _tomorrowButton.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(240, 240, 240);
            _tomorrowButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _tomorrowButton.FlatStyle = FlatStyle.Flat;
            _tomorrowButton.Font = new Font("Segoe UI", 9);
            _tomorrowButton.FlatAppearance.BorderSize = 0;
            _tomorrowButton.Click += (s, e) => SelectDate(DateTime.Today.AddDays(1));

            // Custom date picker
            _customDatePicker = new DateTimePicker();
            _customDatePicker.Location = new Point(190, 9);
            _customDatePicker.Size = new Size(120, 32);
            _customDatePicker.Font = new Font("Segoe UI", 9);
            _customDatePicker.Format = DateTimePickerFormat.Short;
            _customDatePicker.ValueChanged += (s, e) => SelectDate(_customDatePicker.Value.Date);


            // Save button - modern style
            _saveButton = new Button();
            _saveButton.Text = "Create Task";
            _saveButton.Size = new Size(120, 32);
            _saveButton.Location = new Point(320, 9);
            _saveButton.BackColor = Color.White;
            _saveButton.ForeColor = Color.Black;
            _saveButton.FlatStyle = FlatStyle.Flat;
            _saveButton.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            _saveButton.FlatAppearance.BorderSize = 0;
            _saveButton.Click += SaveButton_Click;
            _saveButton.Enabled = false;

            // Cancel button
            _cancelButton = new Button();
            _cancelButton.Text = "Cancel";
            _cancelButton.Size = new Size(80, 32);
            _cancelButton.Location = new Point(450, 9);
            _cancelButton.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(240, 240, 240);
            _cancelButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _cancelButton.FlatStyle = FlatStyle.Flat;
            _cancelButton.Font = new Font("Segoe UI", 9);
            _cancelButton.FlatAppearance.BorderSize = 0;
            _cancelButton.Click += (s, e) => this.Close();

            // Close button
            var closeButton = new Button();
            closeButton.Text = "×";
            closeButton.Size = new Size(32, 32);
            closeButton.Location = new Point(748, 10);
            closeButton.BackColor = Color.Transparent;
            closeButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => this.Close();

            // Add controls to toolbar
            _toolbarPanel.Controls.Add(_todayButton);
            _toolbarPanel.Controls.Add(_tomorrowButton);
            _toolbarPanel.Controls.Add(_customDatePicker);
            _toolbarPanel.Controls.Add(_saveButton);
            _toolbarPanel.Controls.Add(_cancelButton);

            mainPanel.Controls.Add(_titleTextBox);
            mainPanel.Controls.Add(_contentTextBox);
            mainPanel.Controls.Add(_toolbarPanel);
            mainPanel.Controls.Add(closeButton);

            this.Controls.Add(mainPanel);

            _titleTextBox.Focus();
            _selectedPriority = TaskPriority.Medium;
        }

        private void TitleTextBox_GotFocus(object sender, EventArgs e)
        {
            if (_titleTextBox.Text == "Untitled")
            {
                _titleTextBox.Text = "";
                _titleTextBox.ForeColor = _isDarkMode ? Color.White : Color.Black;
            }
        }

        private void TitleTextBox_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_titleTextBox.Text))
            {
                _titleTextBox.Text = "Untitled";
                _titleTextBox.ForeColor = _isDarkMode ? Color.FromArgb(150, 150, 150) : Color.FromArgb(150, 150, 150);
            }
        }

        private void ContentTextBox_GotFocus(object sender, EventArgs e)
        {
            if (_contentTextBox.Text == "Start writing...")
            {
                _contentTextBox.Text = "";
                _contentTextBox.ForeColor = _isDarkMode ? Color.White : Color.Black;
            }
        }

        private void ContentTextBox_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_contentTextBox.Text))
            {
                _contentTextBox.Text = "Start writing...";
                _contentTextBox.ForeColor = _isDarkMode ? Color.FromArgb(150, 150, 150) : Color.FromArgb(150, 150, 150);
            }
        }

        private void TitleTextBox_TextChanged(object sender, EventArgs e)
        {
            _saveButton.Enabled = !string.IsNullOrWhiteSpace(_titleTextBox.Text) && _titleTextBox.Text != "Untitled";
        }

        private void ContentTextBox_TextChanged(object sender, EventArgs e)
        {
            // Auto-resize content area
            var size = TextRenderer.MeasureText(_contentTextBox.Text, _contentTextBox.Font);
            var newHeight = Math.Max(60, Math.Min(300, size.Height + 20));
            _contentTextBox.Height = newHeight;
            _toolbarPanel.Location = new Point(40, 140 + newHeight);
        }

        private void TitleTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                _contentTextBox.Focus();
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_titleTextBox.Text) || _titleTextBox.Text == "Untitled")
            {
                MessageBox.Show("Please enter a task title.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var task = new Models.Task();
            task.Title = _titleTextBox.Text.Trim();
            task.Description = _contentTextBox.Text.Trim();
            task.DueDate = _selectedDate;
            task.IsInBacklog = !_selectedDate.HasValue;
            task.Priority = _selectedPriority;

            _taskService.AddTask(task);
            TaskAdded?.Invoke(this, task);
            this.Close();
        }

        private void SelectDate(DateTime date)
        {
            _selectedDate = date;
            UpdateDateButtons();
        }

        private void UpdateDateButtons()
        {
            // Reset all buttons
            _todayButton.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(240, 240, 240);
            _tomorrowButton.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(240, 240, 240);
            _customDatePicker.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(240, 240, 240);

            if (_selectedDate.HasValue)
            {
                if (_selectedDate.Value.Date == DateTime.Today)
                {
                    _todayButton.BackColor = Color.FromArgb(0, 122, 255);
                    _todayButton.ForeColor = Color.White;
                }
                else if (_selectedDate.Value.Date == DateTime.Today.AddDays(1))
                {
                    _tomorrowButton.BackColor = Color.FromArgb(0, 122, 255);
                    _tomorrowButton.ForeColor = Color.White;
                }
                else
                {
                    _customDatePicker.BackColor = Color.FromArgb(0, 122, 255);
                    _customDatePicker.ForeColor = Color.White;
                }
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