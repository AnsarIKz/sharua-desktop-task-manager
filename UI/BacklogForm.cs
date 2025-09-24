using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SharuaTaskManager.Models;
using SharuaTaskManager.Services;

namespace SharuaTaskManager.UI
{
    public partial class BacklogForm : Form
    {
        private TaskService _taskService;
        private FlowLayoutPanel _backlogFlow;
        private bool _isDarkMode;

        public event EventHandler TaskMoved;

        public BacklogForm(TaskService taskService)
        {
            _taskService = taskService;
            _isDarkMode = IsDarkMode();
            InitializeComponent();
            SetupUI();
            LoadBacklogTasks();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            this.Text = "Backlog";
            this.Size = new Size(700, 500);
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
            titleLabel.Text = "Backlog Tasks";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = _isDarkMode ? Color.White : Color.Black;
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(30, 30);

            _backlogFlow = new FlowLayoutPanel();
            _backlogFlow.Location = new Point(30, 70);
            _backlogFlow.Size = new Size(640, 380);
            _backlogFlow.FlowDirection = FlowDirection.TopDown;
            _backlogFlow.WrapContents = false;
            _backlogFlow.AutoScroll = true;
            _backlogFlow.BackColor = Color.Transparent;

            var closeButton = new Button();
            closeButton.Text = "×";
            closeButton.Size = new Size(30, 30);
            closeButton.Location = new Point(650, 10);
            closeButton.BackColor = Color.Transparent;
            closeButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => this.Close();

            mainPanel.Controls.Add(titleLabel);
            mainPanel.Controls.Add(_backlogFlow);
            mainPanel.Controls.Add(closeButton);

            this.Controls.Add(mainPanel);
        }

        private void LoadBacklogTasks()
        {
            _backlogFlow.Controls.Clear();
            var backlogTasks = _taskService.GetBacklogTasks();

            if (!backlogTasks.Any())
            {
                var noTasksLabel = new Label();
                noTasksLabel.Text = "No tasks in backlog.";
                noTasksLabel.Font = new Font("Segoe UI", 12);
                noTasksLabel.ForeColor = _isDarkMode ? Color.LightGray : Color.Gray;
                noTasksLabel.AutoSize = true;
                _backlogFlow.Controls.Add(noTasksLabel);
                return;
            }

            foreach (var task in backlogTasks)
            {
                var taskControl = CreateBacklogTaskControl(task);
                _backlogFlow.Controls.Add(taskControl);
            }
        }

        private Control CreateBacklogTaskControl(Task task)
        {
            var panel = new Panel();
            panel.Size = new Size(600, 80);
            panel.BackColor = _isDarkMode ? Color.FromArgb(40, 40, 40) : Color.FromArgb(250, 250, 250);
            panel.Margin = new Padding(0, 5, 0, 5);

            var titleLabel = new Label();
            titleLabel.Text = task.Title;
            titleLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            titleLabel.ForeColor = _isDarkMode ? Color.White : Color.Black;
            titleLabel.Location = new Point(15, 10);
            titleLabel.Size = new Size(400, 20);
            titleLabel.AutoEllipsis = true;

            var descLabel = new Label();
            descLabel.Text = task.Description;
            descLabel.Font = new Font("Segoe UI", 9);
            descLabel.ForeColor = _isDarkMode ? Color.LightGray : Color.Gray;
            descLabel.Location = new Point(15, 30);
            descLabel.Size = new Size(400, 15);
            descLabel.AutoEllipsis = true;

            var priorityLabel = new Label();
            priorityLabel.Text = "Priority: " + task.Priority;
            priorityLabel.Font = new Font("Segoe UI", 8);
            priorityLabel.ForeColor = GetPriorityColor(task.Priority);
            priorityLabel.Location = new Point(15, 50);
            priorityLabel.AutoSize = true;

            var moveToTodayButton = new Button();
            moveToTodayButton.Text = "Move to Today";
            moveToTodayButton.Size = new Size(100, 30);
            moveToTodayButton.Location = new Point(450, 10);
            moveToTodayButton.BackColor = Color.FromArgb(33, 150, 243);
            moveToTodayButton.ForeColor = Color.White;
            moveToTodayButton.FlatStyle = FlatStyle.Flat;
            moveToTodayButton.Font = new Font("Segoe UI", 9);
            moveToTodayButton.FlatAppearance.BorderSize = 0;
            moveToTodayButton.Click += (s, e) => MoveToToday(task.Id);

            var scheduleButton = new Button();
            scheduleButton.Text = "Schedule";
            scheduleButton.Size = new Size(80, 30);
            scheduleButton.Location = new Point(450, 45);
            scheduleButton.BackColor = Color.FromArgb(255, 152, 0);
            scheduleButton.ForeColor = Color.White;
            scheduleButton.FlatStyle = FlatStyle.Flat;
            scheduleButton.Font = new Font("Segoe UI", 9);
            scheduleButton.FlatAppearance.BorderSize = 0;
            scheduleButton.Click += (s, e) => ScheduleTask(task.Id);

            var deleteButton = new Button();
            deleteButton.Text = "🗑";
            deleteButton.Size = new Size(30, 30);
            deleteButton.Location = new Point(560, 10);
            deleteButton.BackColor = Color.FromArgb(244, 67, 54);
            deleteButton.ForeColor = Color.White;
            deleteButton.FlatStyle = FlatStyle.Flat;
            deleteButton.Font = new Font("Segoe UI", 10);
            deleteButton.FlatAppearance.BorderSize = 0;
            deleteButton.Click += (s, e) => DeleteTask(task.Id);

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(descLabel);
            panel.Controls.Add(priorityLabel);
            panel.Controls.Add(moveToTodayButton);
            panel.Controls.Add(scheduleButton);
            panel.Controls.Add(deleteButton);

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
                    return Color.Gray;
            }
        }

        private void MoveToToday(Guid taskId)
        {
            _taskService.MoveFromBacklog(taskId, DateTime.Today);
            LoadBacklogTasks();
            if (TaskMoved != null)
                TaskMoved(this, EventArgs.Empty);
        }

        private void ScheduleTask(Guid taskId)
        {
            var scheduleForm = new ScheduleForm();
            if (scheduleForm.ShowDialog() == DialogResult.OK)
            {
                _taskService.MoveFromBacklog(taskId, scheduleForm.SelectedDate);
                LoadBacklogTasks();
                if (TaskMoved != null)
                    TaskMoved(this, EventArgs.Empty);
            }
        }

        private void DeleteTask(Guid taskId)
        {
            var result = MessageBox.Show("Are you sure you want to delete this task?", "Confirm Delete", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                _taskService.DeleteTask(taskId);
                LoadBacklogTasks();
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

    public partial class ScheduleForm : Form
    {
        private DateTimePicker _datePicker;
        private Button _okButton;
        private Button _cancelButton;
        private bool _isDarkMode;

        public DateTime SelectedDate { get; private set; }

        public ScheduleForm()
        {
            _isDarkMode = IsDarkMode();
            InitializeComponent();
            SetupUI();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            this.Text = "Schedule Task";
            this.Size = new Size(300, 200);
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
            titleLabel.Text = "Select Date";
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.ForeColor = _isDarkMode ? Color.White : Color.Black;
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(30, 30);

            _datePicker = new DateTimePicker();
            _datePicker.Location = new Point(30, 70);
            _datePicker.Size = new Size(200, 25);
            _datePicker.Font = new Font("Segoe UI", 10);
            _datePicker.Format = DateTimePickerFormat.Short;
            _datePicker.Value = DateTime.Today.AddDays(1);

            _okButton = new Button();
            _okButton.Text = "OK";
            _okButton.Size = new Size(80, 30);
            _okButton.Location = new Point(100, 120);
            _okButton.BackColor = Color.FromArgb(76, 175, 80);
            _okButton.ForeColor = Color.White;
            _okButton.FlatStyle = FlatStyle.Flat;
            _okButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _okButton.FlatAppearance.BorderSize = 0;
            _okButton.Click += (s, e) =>
            {
                SelectedDate = _datePicker.Value.Date;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            _cancelButton = new Button();
            _cancelButton.Text = "Cancel";
            _cancelButton.Size = new Size(80, 30);
            _cancelButton.Location = new Point(190, 120);
            _cancelButton.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(200, 200, 200);
            _cancelButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _cancelButton.FlatStyle = FlatStyle.Flat;
            _cancelButton.Font = new Font("Segoe UI", 10);
            _cancelButton.FlatAppearance.BorderSize = 0;
            _cancelButton.Click += (s, e) => this.Close();

            mainPanel.Controls.Add(titleLabel);
            mainPanel.Controls.Add(_datePicker);
            mainPanel.Controls.Add(_okButton);
            mainPanel.Controls.Add(_cancelButton);

            this.Controls.Add(mainPanel);
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
    }
}