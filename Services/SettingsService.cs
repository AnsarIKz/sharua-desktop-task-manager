using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SharuaTaskManager.Services
{
    public class SettingsService
    {
        private const string REGISTRY_PATH = @"Software\SharuaTaskManager";

        public bool AutoStart { get; set; }
        public int Theme { get; set; } // 0 = Auto, 1 = Light, 2 = Dark
        public string AddTaskHotkey { get; set; }
        public string ToggleViewHotkey { get; set; }
        public string ShowMainHotkey { get; set; }

        public SettingsService()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            try
            {
                // Load auto-start setting
                var runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                var value = runKey != null ? runKey.GetValue("SharuaTaskManager") : null;
                AutoStart = value != null;

                // Load other settings
                var settingsKey = Registry.CurrentUser.OpenSubKey(REGISTRY_PATH);
                if (settingsKey != null)
                {
                    Theme = (int)(settingsKey.GetValue("Theme") ?? 0);
                    var addTaskValue = settingsKey.GetValue("AddTaskHotkey");
                    AddTaskHotkey = addTaskValue != null ? addTaskValue.ToString() : "Ctrl+N";
                    var toggleValue = settingsKey.GetValue("ToggleViewHotkey");
                    ToggleViewHotkey = toggleValue != null ? toggleValue.ToString() : "Ctrl+T";
                    var showMainValue = settingsKey.GetValue("ShowMainHotkey");
                    ShowMainHotkey = showMainValue != null ? showMainValue.ToString() : "Ctrl+Alt+T";
                }
                else
                {
                // Default values
                Theme = 0;
                AddTaskHotkey = "Ctrl+N";
                ToggleViewHotkey = "Ctrl+T";
                ShowMainHotkey = "Ctrl+Alt+T";
                }
            }
            catch
            {
                // Use default values on error
                AutoStart = false;
                Theme = 0;
                AddTaskHotkey = "Ctrl+N";
                ToggleViewHotkey = "Ctrl+T";
                ShowMainHotkey = "Ctrl+Alt+T";
            }
        }

        public void SaveSettings()
        {
            try
            {
                // Save auto-start setting
                var runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (AutoStart)
                {
                    var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    runKey.SetValue("SharuaTaskManager", appPath);
                }
                else
                {
                    runKey.DeleteValue("SharuaTaskManager", false);
                }

                // Save other settings
                var settingsKey = Registry.CurrentUser.CreateSubKey(REGISTRY_PATH);
                settingsKey.SetValue("Theme", Theme);
                settingsKey.SetValue("AddTaskHotkey", AddTaskHotkey);
                settingsKey.SetValue("ToggleViewHotkey", ToggleViewHotkey);
                settingsKey.SetValue("ShowMainHotkey", ShowMainHotkey);
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving settings: " + ex.Message);
            }
        }

        public bool IsDarkMode()
        {
            if (Theme == 1) return false; // Light
            if (Theme == 2) return true;  // Dark
            
            // Auto - detect system theme
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

        public Keys ParseHotkey(string hotkeyString)
        {
            if (string.IsNullOrEmpty(hotkeyString))
                return Keys.None;

            var parts = hotkeyString.Split('+');
            Keys result = Keys.None;

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                switch (trimmed.ToUpper())
                {
                    case "CTRL":
                        result |= Keys.Control;
                        break;
                    case "ALT":
                        result |= Keys.Alt;
                        break;
                    case "SHIFT":
                        result |= Keys.Shift;
                        break;
                    default:
                        Keys key;
                        if (Enum.TryParse(trimmed, true, out key))
                            result |= key;
                        break;
                }
            }

            return result;
        }
    }
}
