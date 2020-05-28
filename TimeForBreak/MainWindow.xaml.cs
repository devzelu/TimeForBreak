using Microsoft.Win32;
using System;
using System.Configuration;
using System.IO;
using System.Media;
using System.Reflection;
using System.Windows;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using System.ComponentModel;
using System.Linq;

namespace TimeForBreak
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int timeLeft;
        private bool state;
        private DispatcherTimer timer1 = new DispatcherTimer();
        private NotifyIcon icon = new NotifyIcon();
        private ContextMenu contextMenu;
        private string title = "Time For Break";
        private string message = "Time to take a break";
        private System.Media.SoundPlayer player = new SoundPlayer("./sound.wav");

        public MainWindow()
        {
            InitializeComponent();
            SetStartup();
            ApplyAppSettings();

            icon.Icon = new System.Drawing.Icon("./favicon.ico");
            icon.Text = title;
            icon.Visible = true;

            icon.MouseClick += TrayMouse_Click;

            timer1.Interval = new TimeSpan(0, 0, 1);
            timer1.Tick += new EventHandler(timer1_Tick);
        }

        private void TrayMouse_Click(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenu contextMenu = new ContextMenu();
                var settingsMenu = new MenuItem() { Header = "Settings" };
                var hideShowMenu = new MenuItem() { Header = "Hide/Show" };
                var about = new MenuItem() { Header = "About" };
                var closeMenu = new MenuItem() { Header = "Close" };

                contextMenu.Items.Add(settingsMenu);
                settingsMenu.Click += SettingsMenu_Click;
                contextMenu.Items.Add(hideShowMenu);
                hideShowMenu.Click += hideShowMenu_Click;
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(about);
                about.Click += About_Click;
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(closeMenu);
                closeMenu.Click += closeMenu_Click;
                contextMenu.IsOpen = true;
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("About");
        }

        private void SettingsMenu_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings tab was clicked");
            // Open settings window
        }

        private void hideShowMenu_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hide/Show tab was clicked");
        }

        private void closeMenu_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SetStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("TimeForBreak", "\"" + Assembly.GetExecutingAssembly().Location.Replace("Time For Break.dll", "Time For Break.exe") + "\"");
            }
        }

        private static void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Error writing app settings");
            }
        }

        private static void ApplyAppSettings()
        {
            Application.Current.MainWindow.Left = Convert.ToDouble(ConfigurationManager.AppSettings.Get("PositionX"));
            Application.Current.MainWindow.Top = Convert.ToDouble(ConfigurationManager.AppSettings.Get("PositionY"));
        }

        private (double, double) GetWindowPosition()
        {
            var x = Application.Current.MainWindow.Left;
            var y = Application.Current.MainWindow.Top;
            return (x, y);
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
                var x = GetWindowPosition();
                AddUpdateAppSettings("PositionX", x.Item1.ToString());
                AddUpdateAppSettings("PositionY", x.Item2.ToString());
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (TimerInput.Text == "00:00")
            {
                MessageBox.Show("Please enter the time to start!", "Enter the time", MessageBoxButton.OK);
            }
            else if (TimerInput.Text.Contains(':'))
            {
                string[] totalSeconds = TimerInput.Text.Split(":");
                int minutes = Convert.ToInt32(totalSeconds[0]);
                int seconds = Convert.ToInt32(totalSeconds[1]);
                timeLeft = (minutes * 60) + seconds;

                TimerInput.IsReadOnly = true;
                timer1.Start();
                state = true; //on
            }
            else
            {
                MessageBox.Show("Invalid format [mm:ss]", "Please enter the time to start!", MessageBoxButton.OK);
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (state == true)
            {
                timer1.Stop();
                TimerInput.IsReadOnly = false;
                state = false;
            }
            else
            {
                timer1.Stop();
                TimerInput.Text = "60:00";
                TimerInput.IsReadOnly = false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (timeLeft > 0)
            {
                timeLeft = timeLeft - 1;
                var timespan = TimeSpan.FromSeconds(timeLeft);
                TimerInput.Text = timespan.ToString(@"mm\:ss");
            }
            else
            {
                timer1.Stop();
                player.PlayLooping();
                icon.BalloonTipTitle = title;
                icon.BalloonTipText = message;
                icon.ShowBalloonTip(3000);
                var result = MessageBox.Show(message, "Info!", MessageBoxButton.OK);
                if (result == MessageBoxResult.OK)
                {
                    TimerInput.Text = "60:00";
                    player.Stop();
                    TimerInput.IsReadOnly = false;
                }
            }
        }
    }
}