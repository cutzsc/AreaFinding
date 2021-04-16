using System;
using System.Windows;

namespace IslandSquare
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public MonteCarlo Settings { get; set; }
        
        public SettingsWindow()
        {
            InitializeComponent();
            Loaded += SettingsWindow_Loaded;
        }
        
        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = Settings;
            Closed += SettingsWindow_Closed;
        }

        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            if (Settings.IsSettingsChanged)
                Settings.Stop();
            else
                Settings.Resume();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
