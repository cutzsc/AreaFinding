using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IslandSquare
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SettingsWindow settingsWin;

        ObservableCollection<ColorRange> chosenColors;

        OpenFileDialog openFileDialog;
        MonteCarlo monte;

        bool recordingToRange = false;
        bool isRecording = false;

        public enum AppState : byte
        {
            Pause,
            Working,
            Ready,
            WaitingForImage
        }
        static public AppState state;
        public void SetAppState(AppState state)
        {
            MainWindow.state = state;
            switch (state)
            {
                case AppState.WaitingForImage:
                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri("Imgs\\placeholder-image.png", UriKind.Relative);
                    img.EndInit();
                    picture.Source = img;
                    statusTBlock.Text = "Ожидается изображение...";
                    conditionsRange.IsEnabled = true;
                    break;
                case AppState.Pause:
                    statusTBlock.Text = "Пауза";
                    conditionsRange.IsEnabled = false;
                    break;
                case AppState.Ready:
                    statusTBlock.Text = "Готово";
                    conditionsRange.IsEnabled = true;
                    break;
                case AppState.Working:
                    statusTBlock.Text = "В работе";
                    conditionsRange.IsEnabled = false;
                    break;
                default:
                    break;
            }
        }

        void SetWindowSize()
        {
            double width = SystemParameters.FullPrimaryScreenWidth - 100;
            double height = width * SystemParameters.PrimaryScreenHeight / SystemParameters.PrimaryScreenWidth;
            if (width > 1150)
                return;
            else if (width < 1000)
            {
                width = 1000;
                height = 710;
            }
            Top = 10;
            Left = 10;
            Width = width;
            Height = height;
        }
        
        public MainWindow()
        {
            InitializeComponent();
            openFileDialog = new OpenFileDialog();
            monte = new MonteCarlo();
            monte.setLabelsData += SetLabelsData;
            monte.setAppState += SetAppState;
            chosenColors = new ObservableCollection<ColorRange>();
            chosenColorsList.ItemsSource = chosenColors;
            rangeColorsList.ItemsSource = monte.RangeColors;
            SetWindowSize();
            SetAppState(AppState.WaitingForImage);
        }

        private void DeleteSelectedColors(object sender)
        {
            ListView listView = sender as ListView;
            if (listView.SelectedItem == null)
                return;

            if (listView.SelectedItem.GetType() == typeof(ColorRange))
            {
                ObservableCollection<ColorRange> list = (ObservableCollection<ColorRange>)listView.ItemsSource;
                while (listView.SelectedItems.Count > 0)
                {
                    list.Remove((ColorRange)listView.SelectedItem);
                }
            }
            else if (listView.SelectedItem.GetType() == typeof(DesiredColorRange))
            {
                ObservableCollection<DesiredColorRange> list = (ObservableCollection<DesiredColorRange>)listView.ItemsSource;
                while (listView.SelectedItems.Count > 0)
                {
                    list.Remove((DesiredColorRange)listView.SelectedItem);
                }
            }
        }

        void SetLabelsData(int shots, int accuracy, double area)
        {
            shotsCountTBlock.Text = shots.ToString();
            accuracyCountTBlock.Text = accuracy.ToString();
            areaTBlock.Text = area.ToString("0.000");
        }

        // MENU EVENTS BEGIN
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            monte.Pause();
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    monte.Picture = new Picture(openFileDialog.FileName);
                    picture.Source = monte.Picture.Bitmap;
                    SetAppState(AppState.Ready);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    monte.Resume();
                }
            }
            else
            {
                monte.Resume();
            }
        }

        private void RefreshFile_Click(object sender, RoutedEventArgs e)
        {
            monte.Stop();
            monte.Picture?.Refresh();
        }

        private void CloseFile_Click(object sender, RoutedEventArgs e)
        {
            monte.Stop();
            picture.Source = null;
            monte.Picture = null;
            SetAppState(AppState.WaitingForImage);
            currentCursorPosLabel.Text = "X: 000\tY: 000";
            currentCursorColorLabel.Text = "\tRGB (000, 000, 000)";
            currentColorRect.Fill = new SolidColorBrush(Colors.White);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            monte.Stop();
            Close();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            monte.Pause();
            settingsWin = new SettingsWindow();
            settingsWin.Owner = this;
            monte.IsSettingsChanged = false;
            settingsWin.Settings = monte;
            settingsWin.Top = Top + 100;
            settingsWin.Left = Left + 100;
            settingsWin.ShowDialog();
        }
        // MENU EVENTS END

        // LIST EVENTS BEGIN
        private void ClearList_Click(object sender, RoutedEventArgs e)
        {
            string name = ((Button)sender).Name;
            switch (name)
            {
                case "clearChosenListButton":
                    chosenColors.Clear();
                    break;
                case "clearRangeListButton":
                    monte.RangeColors.Clear();
                    break;
                default:
                    break;
            }
        }

        private void DelColor_Click(object sender, RoutedEventArgs e) // delete in monte
        {
            string name = ((Button)sender).Name;
            switch (name)
            {
                case "delChosenColButton":
                    DeleteSelectedColors(chosenColorsList);
                    break;
                case "delRangeColButton":
                    DeleteSelectedColors(rangeColorsList);
                    break;
                default:
                    break;
            }
        }

        private void ColorsList_KeyDown(object sender, KeyEventArgs e) // delete in monte
        {
            if (e.Key == Key.Delete)
            {
                DeleteSelectedColors(sender);
            }
        }

        private void RecordRange_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            b.Content = b.Content.ToString() == "Запись" ? "Стоп" : "Запись";
            recordingToRange = !recordingToRange;
        }
        
        private void AddToRange_Click(object sender, RoutedEventArgs e)
        {
            monte.AddToRange(chosenColors);
        }
        // LIST EVENTS END

        // PICTURE EVENTS BEGIN
        private void Picture_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isRecording = true;
        }

        private void Picture_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isRecording = false;
        }

        private void Picture_MouseMove(object sender, MouseEventArgs e)
        {
            if (state == AppState.WaitingForImage)
                return;

            double actualX = e.GetPosition(picture).X;
            double actualY = e.GetPosition(picture).Y;

            int x = (int)(actualX * monte.Picture.Bitmap.PixelWidth / picture.ActualWidth);
            int y = (int)(actualY * monte.Picture.Bitmap.PixelHeight / picture.ActualHeight);

            if (y == monte.Picture.Bitmap.PixelHeight)
                y = monte.Picture.Bitmap.PixelHeight - 1;
            if (x == monte.Picture.Bitmap.PixelWidth)
                x = monte.Picture.Bitmap.PixelWidth - 1;

            Color c;
            c = monte.Picture.GetPixelColor(x, y);
            SolidColorBrush scb = new SolidColorBrush(c);
            currentCursorPosLabel.Text = "X: " + x + "\tY: " + y;
            currentCursorColorLabel.Text = "\tRGB (" + c.R + ", " + c.G + ", " +  c.B + ")";
            currentColorRect.Fill = scb;

            if (recordingToRange && isRecording)
            {
                chosenColors.Add(new ColorRange(c));
            }
        }

        private void Picture_MouseLeave(object sender, MouseEventArgs e)
        {
            isRecording = false;
        }
        // PICTURE EVENTS END
        
        // MAIN PAGE
        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            if (state == AppState.WaitingForImage)
                return;
            int n = 0;
            double sourceSquare = 0;
            try
            {
                n = Convert.ToInt32(nTBox.Text);
                sourceSquare = Convert.ToDouble(sourceAreaTBox.Text.Replace('.', ','));
                if (n <= 0 || sourceSquare <= 0)
                    throw new Exception();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\nПоле \"количество испытаний\" должно иметь целое положительное число больше нуля.\nПоле \"исходная площадь\" может иметь вещественные и целые положительные числа больше нуля.");
                return;
            }

            SetAppState(AppState.Working);
            
            monte.Calculate(n, sourceSquare, shapeRangeChBox.IsChecked == true, instantCalcChBox.IsChecked == true);
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            monte.Pause();
        }

        private void Resume_Click(object sender, RoutedEventArgs e)
        {
            monte.Resume();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            monte.Stop();
        }
    }
}
