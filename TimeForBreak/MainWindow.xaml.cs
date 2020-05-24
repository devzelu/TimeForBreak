using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Media;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.Reflection;

namespace TimeForBreak
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int timeLeft;

        private System.Windows.Threading.DispatcherTimer timer1 = new System.Windows.Threading.DispatcherTimer();

        private System.Media.SoundPlayer player = new SoundPlayer(System.Reflection.Assembly.GetEntryAssembly().Location.Replace("TimeForBreak.dll", "sound.wav"));

        public MainWindow()
        {
            InitializeComponent();
            ApplyAppSettings();

            timer1.Interval = new TimeSpan(0, 0, 1);
            timer1.Start();
            SetStartup();
        }

        private void SetStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("TimeForBreak", "\"" + Assembly.GetExecutingAssembly().Location.Replace("TimeForBreak.dll", "TimeForBreak.exe") + "\"");
                // var x = Assembly.GetExecutingAssembly().Location.Replace("TimeForBreak.dll","TimeForBreak.exe");
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

        private void Window_MouseMove(object sender, MouseEventArgs e)
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
            else
            {
                string[] totalSeconds = TimerInput.Text.Split(":");
                int minutes = Convert.ToInt32(totalSeconds[0]);
                int seconds = Convert.ToInt32(totalSeconds[1]);
                timeLeft = (minutes * 60) + seconds;

                TimerInput.IsReadOnly = true;

                timer1.Tick += new EventHandler(timer1_Tick);
                timer1.Start();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            timer1.Stop();
            timeLeft = 0;
            TimerInput.IsReadOnly = false;
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
                var result = MessageBox.Show("Time to take a break", "Info!", MessageBoxButton.OK);
                if (result == MessageBoxResult.OK)
                {
                    player.Stop();
                    TimerInput.IsReadOnly = false;
                }
            }
        }
    }
}