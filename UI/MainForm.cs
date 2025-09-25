using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharuaTaskManager.Models;
using SharuaTaskManager.Services;

namespace SharuaTaskManager.UI
{
    public partial class MainForm : Form
    {
        // Windows API для глобальных горячих клавиш
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID_TOGGLE = 1;
        private const int HOTKEY_ID_ADD_TASK = 2;

        private TaskService _taskService;
        private SettingsService _settingsService;
        private Panel _mainPanel;
        private Panel _todayTasksPanel;
        private Panel _statsPanel;
        private Button _addTaskButton;
        private Button _backlogButton;
        private Button _settingsButton;
        private Label _titleLabel;
        private FlowLayoutPanel _statsFlow;
        private bool _isDarkMode;

        public MainForm()
        {
            InitializeComponent();
            _taskService = new TaskService();
            _settingsService = new SettingsService();
            _isDarkMode = _settingsService.IsDarkMode();
            SetupUI();
            LoadTodayTasks();
            LoadStats();
            SetupHotkeys();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            this.Text = "Sharua Task Manager";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.Opacity = 0.95;
            
            this.ResumeLayout(false);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            
            if (m.Msg == WM_HOTKEY)
            {
                switch (m.WParam.ToInt32())
                {
                    case HOTKEY_ID_TOGGLE:
                        ViewToggleButton_Click(this, EventArgs.Empty);
                        break;
                    case HOTKEY_ID_ADD_TASK:
                        AddTaskButton_Click(this, EventArgs.Empty);
                        break;
                }
                return;
            }
            
            base.WndProc(ref m);
        }

        private void SetupUI()
        {
            _mainPanel = new Panel();
            _mainPanel.Dock = DockStyle.Fill;
            _mainPanel.BackColor = Color.Transparent;
            _mainPanel.Padding = new Padding(20);

            _titleLabel = new Label();
            _titleLabel.Text = "Today's Tasks";
            _titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            _titleLabel.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _titleLabel.AutoSize = true;
            _titleLabel.Location = new Point(20, 20);

            _addTaskButton = new Button();
            _addTaskButton.Text = "+ Add Task";
            _addTaskButton.Size = new Size(100, 30);
            _addTaskButton.Location = new Point(650, 20);
            _addTaskButton.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(240, 240, 240);
            _addTaskButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _addTaskButton.FlatStyle = FlatStyle.Flat;
            _addTaskButton.Font = new Font("Segoe UI", 9);
            _addTaskButton.FlatAppearance.BorderSize = 0;
            _addTaskButton.Click += AddTaskButton_Click;

            _backlogButton = new Button();
            _backlogButton.Text = "Backlog";
            _backlogButton.Size = new Size(80, 30);
            _backlogButton.Location = new Point(560, 20);
            _backlogButton.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(240, 240, 240);
            _backlogButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _backlogButton.FlatStyle = FlatStyle.Flat;
            _backlogButton.Font = new Font("Segoe UI", 9);
            _backlogButton.FlatAppearance.BorderSize = 0;
            _backlogButton.Click += BacklogButton_Click;

            // View toggle button
            var viewToggleButton = new Button();
            viewToggleButton.Text = "📋";
            viewToggleButton.Size = new Size(40, 30);
            viewToggleButton.Location = new Point(500, 20);
            viewToggleButton.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(240, 240, 240);
            viewToggleButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            viewToggleButton.FlatStyle = FlatStyle.Flat;
            viewToggleButton.Font = new Font("Segoe UI", 12);
            viewToggleButton.FlatAppearance.BorderSize = 0;
            viewToggleButton.Click += ViewToggleButton_Click;

            // Settings button
            _settingsButton = new Button();
            _settingsButton.Text = "⚙";
            _settingsButton.Size = new Size(40, 30);
            _settingsButton.Location = new Point(450, 20);
            _settingsButton.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(240, 240, 240);
            _settingsButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _settingsButton.FlatStyle = FlatStyle.Flat;
            _settingsButton.Font = new Font("Segoe UI", 12);
            _settingsButton.FlatAppearance.BorderSize = 0;
            _settingsButton.Click += SettingsButton_Click;


            _todayTasksPanel = new Panel();
            _todayTasksPanel.Location = new Point(20, 70);
            _todayTasksPanel.Size = new Size(760, 300);
            _todayTasksPanel.BackColor = Color.Transparent;

            // Create FlowLayoutPanel for card view
            var todayTasksFlow = new FlowLayoutPanel();
            todayTasksFlow.Dock = DockStyle.Fill;
            todayTasksFlow.FlowDirection = FlowDirection.TopDown;
            todayTasksFlow.WrapContents = false;
            todayTasksFlow.AutoScroll = true;
            todayTasksFlow.BackColor = Color.Transparent;

            _statsPanel = new Panel();
            _statsPanel.Location = new Point(20, 390);
            _statsPanel.Size = new Size(760, 150);
            _statsPanel.BackColor = Color.Transparent;

            var statsLabel = new Label();
            statsLabel.Text = "Activity (Last 30 days)";
            statsLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            statsLabel.ForeColor = _isDarkMode ? Color.White : Color.Black;
            statsLabel.AutoSize = true;
            statsLabel.Location = new Point(0, 0);

            _statsFlow = new FlowLayoutPanel();
            _statsFlow.Location = new Point(0, 25);
            _statsFlow.Size = new Size(760, 120);
            _statsFlow.FlowDirection = FlowDirection.LeftToRight;
            _statsFlow.WrapContents = true;
            _statsFlow.AutoScroll = true;
            _statsFlow.BackColor = Color.Transparent;

            _todayTasksPanel.Controls.Add(todayTasksFlow);
            _statsPanel.Controls.Add(statsLabel);
            _statsPanel.Controls.Add(_statsFlow);

            _mainPanel.Controls.Add(_titleLabel);
            _mainPanel.Controls.Add(_addTaskButton);
            _mainPanel.Controls.Add(_backlogButton);
            _mainPanel.Controls.Add(_settingsButton);
            _mainPanel.Controls.Add(viewToggleButton);
            _mainPanel.Controls.Add(_todayTasksPanel);
            _mainPanel.Controls.Add(_statsPanel);

            this.Controls.Add(_mainPanel);

            var closeButton = new Button();
            closeButton.Text = "×";
            closeButton.Size = new Size(30, 30);
            closeButton.Location = new Point(750, 10);
            closeButton.BackColor = Color.Transparent;
            closeButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => this.Close();
            this.Controls.Add(closeButton);
        }

        private void LoadTodayTasks()
        {
            var todayTasks = _taskService.GetTodayTasks();
            LoadTasksInCardView(todayTasks);
        }

        private void LoadTasksInCardView(List<Models.Task> tasks)
        {
            var flowPanel = _todayTasksPanel.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();
            if (flowPanel != null)
            {
                flowPanel.Controls.Clear();
                
                if (!tasks.Any())
                {
                    var noTasksLabel = new Label();
                    noTasksLabel.Text = "No tasks for today. Great job! 🎉";
                    noTasksLabel.Font = new Font("Segoe UI", 12);
                    noTasksLabel.ForeColor = _isDarkMode ? Color.LightGray : Color.Gray;
                    noTasksLabel.AutoSize = true;
                    flowPanel.Controls.Add(noTasksLabel);
                    return;
                }

                foreach (var task in tasks)
                {
                    var taskControl = CreateTaskControl(task);
                    flowPanel.Controls.Add(taskControl);
                }
            }
        }


        private Control CreateTaskControl(Models.Task task)
        {
            var panel = new Panel();
            panel.Size = new Size(720, 60);
            panel.BackColor = _isDarkMode ? Color.FromArgb(40, 40, 40) : Color.FromArgb(250, 250, 250);
            panel.Margin = new Padding(0, 5, 0, 5);

            var titleLabel = new Label();
            titleLabel.Text = task.Title;
            titleLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            titleLabel.ForeColor = _isDarkMode ? Color.White : Color.Black;
            titleLabel.Location = new Point(15, 10);
            titleLabel.Size = new Size(500, 20);
            titleLabel.AutoEllipsis = true;

            var descLabel = new Label();
            descLabel.Text = !string.IsNullOrEmpty(task.Description) ? task.Description : "No description";
            descLabel.Font = new Font("Segoe UI", 9);
            descLabel.ForeColor = _isDarkMode ? Color.LightGray : Color.Gray;
            descLabel.Location = new Point(15, 30);
            descLabel.Size = new Size(500, 15);
            descLabel.AutoEllipsis = true;

            var dueDateLabel = new Label();
            dueDateLabel.Text = task.DueDate.HasValue ? task.DueDate.Value.ToString("MMM dd") : "No date";
            dueDateLabel.Font = new Font("Segoe UI", 8);
            dueDateLabel.ForeColor = _isDarkMode ? Color.LightGray : Color.Gray;
            dueDateLabel.Location = new Point(15, 45);
            dueDateLabel.Size = new Size(100, 15);
            dueDateLabel.AutoEllipsis = true;

            var priorityLabel = new Label();
            priorityLabel.Text = task.Priority.ToString();
            priorityLabel.Font = new Font("Segoe UI", 8);
            priorityLabel.ForeColor = GetPriorityColor(task.Priority);
            priorityLabel.Location = new Point(120, 45);
            priorityLabel.Size = new Size(60, 15);
            priorityLabel.AutoEllipsis = true;

            var completeButton = new Button();
            completeButton.Text = "✓";
            completeButton.Size = new Size(30, 30);
            completeButton.Location = new Point(650, 15);
            completeButton.BackColor = Color.FromArgb(76, 175, 80);
            completeButton.ForeColor = Color.White;
            completeButton.FlatStyle = FlatStyle.Flat;
            completeButton.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            completeButton.FlatAppearance.BorderSize = 0;
            completeButton.Click += (s, e) => CompleteTask(task.Id);

            var backlogButton = new Button();
            backlogButton.Text = "📋";
            backlogButton.Size = new Size(30, 30);
            backlogButton.Location = new Point(610, 15);
            backlogButton.BackColor = Color.FromArgb(255, 152, 0);
            backlogButton.ForeColor = Color.White;
            backlogButton.FlatStyle = FlatStyle.Flat;
            backlogButton.Font = new Font("Segoe UI", 10);
            backlogButton.FlatAppearance.BorderSize = 0;
            backlogButton.Click += (s, e) => MoveToBacklog(task.Id);

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(descLabel);
            panel.Controls.Add(dueDateLabel);
            panel.Controls.Add(priorityLabel);
            panel.Controls.Add(completeButton);
            panel.Controls.Add(backlogButton);

            return panel;
        }

        private Color GetPriorityColor(TaskPriority priority)
        {
            switch (priority)
            {
                case TaskPriority.Low:
                    return Color.FromArgb(76, 175, 80);
                case TaskPriority.Medium:
                    return Color.FromArgb(255, 152, 0);
                case TaskPriority.High:
                    return Color.FromArgb(255, 87, 34);
                case TaskPriority.Urgent:
                    return Color.FromArgb(244, 67, 54);
                default:
                    return _isDarkMode ? Color.LightGray : Color.Gray;
            }
        }

        private void LoadStats()
        {
            _statsFlow.Controls.Clear();
            var stats = _taskService.GetTaskStats(30);
            var maxTasks = stats.Values.DefaultIfEmpty(0).Max();

            var statsList = stats.ToList();
            var last30Days = statsList.Skip(Math.Max(0, statsList.Count - 30));
            foreach (var kvp in last30Days)
            {
                var intensity = maxTasks > 0 ? (float)kvp.Value / maxTasks : 0;
                var color = GetActivityColor(intensity);
                
                var dayControl = new Panel();
                dayControl.Size = new Size(20, 20);
                dayControl.BackColor = color;
                dayControl.Margin = new Padding(2);
                dayControl.Cursor = Cursors.Hand;

                var tooltip = new ToolTip();
                tooltip.SetToolTip(dayControl, kvp.Key.ToString("MMM dd") + ": " + kvp.Value + " tasks completed");

                _statsFlow.Controls.Add(dayControl);
            }
        }

        private Color GetActivityColor(float intensity)
        {
            if (intensity == 0)
                return _isDarkMode ? Color.FromArgb(30, 30, 30) : Color.FromArgb(240, 240, 240);

            var baseColor = _isDarkMode ? Color.FromArgb(100, 150, 255) : Color.FromArgb(0, 100, 255);
            var alpha = (int)(100 + intensity * 155);
            return Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B);
        }

        private void CompleteTask(Guid taskId)
        {
            _taskService.CompleteTask(taskId);
            LoadTodayTasks();
            LoadStats();
        }

        private void MoveToBacklog(Guid taskId)
        {
            _taskService.MoveToBacklog(taskId);
            LoadTodayTasks();
        }

        private void AddTaskButton_Click(object sender, EventArgs e)
        {
            try
            {
                var addTaskForm = new AddTaskForm(_taskService, _settingsService);
                addTaskForm.TaskAdded += (s, task) =>
                {
                    LoadTodayTasks();
                    LoadStats();
                };
                addTaskForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Add Task form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BacklogButton_Click(object sender, EventArgs e)
        {
            var backlogForm = new BacklogForm(_taskService);
            backlogForm.TaskMoved += (s, args) =>
            {
                LoadTodayTasks();
                LoadStats();
            };
            backlogForm.ShowDialog();
        }

        private void SetupHotkeys()
        {
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
            RegisterGlobalHotkeys();
        }

        private void RegisterGlobalHotkeys()
        {
            try
            {
                // Регистрируем глобальные горячие клавиши
                var toggleKeys = _settingsService.ParseHotkey(_settingsService.ToggleViewHotkey);
                var addTaskKeys = _settingsService.ParseHotkey(_settingsService.AddTaskHotkey);

                RegisterHotKey(this.Handle, HOTKEY_ID_TOGGLE, GetModifiers(toggleKeys), GetKeyCode(toggleKeys));
                RegisterHotKey(this.Handle, HOTKEY_ID_ADD_TASK, GetModifiers(addTaskKeys), GetKeyCode(addTaskKeys));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error registering hotkeys: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private int GetModifiers(Keys keys)
        {
            int modifiers = 0;
            if ((keys & Keys.Control) == Keys.Control) modifiers |= 0x0002; // MOD_CONTROL
            if ((keys & Keys.Alt) == Keys.Alt) modifiers |= 0x0001; // MOD_ALT
            if ((keys & Keys.Shift) == Keys.Shift) modifiers |= 0x0004; // MOD_SHIFT
            return modifiers;
        }

        private int GetKeyCode(Keys keys)
        {
            return (int)(keys & Keys.KeyCode);
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Parse hotkeys from settings
            var addTaskKeys = _settingsService.ParseHotkey(_settingsService.AddTaskHotkey);
            var toggleViewKeys = _settingsService.ParseHotkey(_settingsService.ToggleViewHotkey);
            var showMainKeys = _settingsService.ParseHotkey(_settingsService.ShowMainHotkey);

            // Check for hotkey matches
            if (IsHotkeyMatch(e, addTaskKeys))
            {
                AddTaskButton_Click(sender, e);
            }
            else if (IsHotkeyMatch(e, toggleViewKeys))
            {
                ViewToggleButton_Click(sender, e);
            }
            else if (IsHotkeyMatch(e, showMainKeys))
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private bool IsHotkeyMatch(KeyEventArgs e, Keys targetKeys)
        {
            if (targetKeys == Keys.None) return false;

            var currentKeys = Keys.None;
            if (e.Control) currentKeys |= Keys.Control;
            if (e.Alt) currentKeys |= Keys.Alt;
            if (e.Shift) currentKeys |= Keys.Shift;
            currentKeys |= e.KeyCode;

            return currentKeys == targetKeys;
        }

        private void ViewToggleButton_Click(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                this.Hide();
            }
            else
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
                this.Activate();
            }
        }


        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // Отменяем регистрацию глобальных горячих клавиш
            try
            {
                UnregisterHotKey(this.Handle, HOTKEY_ID_TOGGLE);
                UnregisterHotKey(this.Handle, HOTKEY_ID_ADD_TASK);
            }
            catch { }
            
            base.OnFormClosed(e);
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            var settingsForm = new SettingsForm();
            settingsForm.SettingsChanged += (s, args) =>
            {
                // Reload settings
                _settingsService.LoadSettings();
                _isDarkMode = _settingsService.IsDarkMode();
                
                // Перерегистрируем горячие клавиши с новыми настройками
                try
                {
                    UnregisterHotKey(this.Handle, HOTKEY_ID_TOGGLE);
                    UnregisterHotKey(this.Handle, HOTKEY_ID_ADD_TASK);
                }
                catch { }
                
                RegisterGlobalHotkeys();
                
                // Update UI theme
                UpdateTheme();
            };
            settingsForm.ShowDialog();
        }

        private void UpdateTheme()
        {
            // Update form colors
            this.BackColor = _isDarkMode ? Color.FromArgb(30, 30, 30) : Color.FromArgb(250, 250, 250);
            
            // Update all controls recursively
            UpdateControlTheme(this);
            
            // Reload data to refresh colors
            LoadTodayTasks();
            LoadStats();
        }

        private void UpdateControlTheme(Control control)
        {
            if (control is Label)
            {
                var label = (Label)control;
                label.ForeColor = _isDarkMode ? Color.White : Color.Black;
            }
            else if (control is Button)
            {
                var button = (Button)control;
                if (button.Text == "×" || button.Text == "⚙" || button.Text == "📋")
                {
                    button.ForeColor = _isDarkMode ? Color.White : Color.Black;
                }
                else if (button.Text == "Add Task")
                {
                    button.BackColor = Color.FromArgb(76, 175, 80);
                }
                else
                {
                    button.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(240, 240, 240);
                    button.ForeColor = _isDarkMode ? Color.White : Color.Black;
                }
            }
            else if (control is DataGridView)
            {
                var dataGrid = (DataGridView)control;
                dataGrid.BackgroundColor = _isDarkMode ? Color.FromArgb(30, 30, 30) : Color.White;
                dataGrid.ForeColor = _isDarkMode ? Color.White : Color.Black;
                dataGrid.ColumnHeadersDefaultCellStyle.BackColor = _isDarkMode ? Color.FromArgb(50, 50, 50) : Color.FromArgb(240, 240, 240);
                dataGrid.ColumnHeadersDefaultCellStyle.ForeColor = _isDarkMode ? Color.White : Color.Black;
                dataGrid.DefaultCellStyle.BackColor = _isDarkMode ? Color.FromArgb(30, 30, 30) : Color.White;
                dataGrid.DefaultCellStyle.ForeColor = _isDarkMode ? Color.White : Color.Black;
                dataGrid.AlternatingRowsDefaultCellStyle.BackColor = _isDarkMode ? Color.FromArgb(40, 40, 40) : Color.FromArgb(250, 250, 250);
            }

            // Recursively update child controls
            foreach (Control child in control.Controls)
            {
                UpdateControlTheme(child);
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