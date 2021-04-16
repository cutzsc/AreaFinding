using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace IslandSquare
{
    public class MonteCarlo : INotifyPropertyChanged
    {
        int n;
        int shots;
        int accuracy;
        double desiredSquare;
        double sourceSquare;
        Random rand;
        int rangeNoise;
        int framesPerSecond;
        int iterationsPerPass;
        DispatcherTimer timer;
        bool stopped = false;
        bool isShapeInRange = false;

        public delegate void SetLabelsData(int shots, int accuracy, double square);
        public delegate void SetAppState(MainWindow.AppState state);
        public SetLabelsData setLabelsData;
        public SetAppState setAppState;

        Picture picture;

        public Picture Picture
        {
            get { return picture; }
            set
            {
                picture = value;
            }
        }
        public ObservableCollection<DesiredColorRange> RangeColors { get; private set; }

        // Settings
        Color sourceShape;
        public Color SourceShape
        {
            get { return sourceShape; }
            set
            {
                IsSettingsChanged = true;
                sourceShape = value;
                OnPropertyChanged("SourceShape");
            }
        }
        Color desiredShape;
        public Color DesiredShape
        {
            get { return desiredShape; }
            set
            {
                IsSettingsChanged = true;
                desiredShape = value;
                OnPropertyChanged("DesiredShape");
            }
        }
        public int RangeNoise
        {
            get { return rangeNoise; }
            set
            {
                if (value < 0 || value > 20)
                {
                    MessageBox.Show("Отклонение может иметь целые значение от 0 до 20.");
                }
                else
                {
                    IsSettingsChanged = true;
                    rangeNoise = value;
                    OnPropertyChanged("RangeNoise");
                }
            }
        }
        public int FramesPerSecond
        {
            get { return framesPerSecond; }
            set
            {
                if (value < 1 || value > 250)
                    MessageBox.Show("Количество обновлений в секунду может иметь значение от 1 до 250.");
                else
                {
                    IsSettingsChanged = true;
                    framesPerSecond = value;
                    OnPropertyChanged("MillisecondsPerFrame");
                    timer.Interval = TimeSpan.FromMilliseconds(1000 / framesPerSecond);
                }
            }
        }
        public int IterationsPerPass
        {
            get { return iterationsPerPass; }
            set
            {
                if (value < 1)
                    MessageBox.Show("Количество итераций за один проход, может иметь целые значения больше нуля.");
                else
                {
                    IsSettingsChanged = true;
                    iterationsPerPass = value;
                    OnPropertyChanged("IterationsPerPass");
                }
            }
        }
        public bool IsSettingsChanged { get; set; }
        // Settings

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MonteCarlo()
        {
            RangeColors = new ObservableCollection<DesiredColorRange>();
            rand = new Random();
            timer = new DispatcherTimer();
            FramesPerSecond = 125;
            timer.Tick += Timer_Tick;
            IterationsPerPass = 1000;
            RangeNoise = 2;
            SourceShape = Color.FromArgb(255, 0, 255, 255);
            DesiredShape = Color.FromArgb(255, 128, 0, 0);
        }

        public void AddToRange(ObservableCollection<ColorRange> list)
        {
            if (list.Count == 0)
                return;
            Color first = list[0].Color;
            int[] r = new int[2]
            {
                first.R,
                first.R
            };
            int[] g = new int[2]
            {
                first.G,
                first.G
            };
            int[] b = new int[2]
            {
                first.B,
                first.B
            };

            for (int i = 1; i < list.Count; i++)
            {
                Color current = list[i].Color;

                if (current.R > r[1]) r[1] = current.R;
                if (current.G > g[1]) g[1] = current.G;
                if (current.B > b[1]) b[1] = current.B;
                if (current.R < r[0]) r[0] = current.R;
                if (current.G < g[0]) g[0] = current.G;
                if (current.B < b[0]) b[0] = current.B;
            }

            r[0] = r[0] - RangeNoise < 0 ? 0 : r[0] - RangeNoise;
            r[1] = r[1] + RangeNoise > 255 ? 255 : r[1] + RangeNoise;
            g[0] = g[0] - RangeNoise < 0 ? 0 : g[0] - RangeNoise;
            g[1] = g[1] + RangeNoise > 255 ? 255 : g[1] + RangeNoise;
            b[0] = b[0] - RangeNoise < 0 ? 0 : b[0] - RangeNoise;
            b[1] = b[1] + RangeNoise > 255 ? 255 : b[1] + RangeNoise;

            RangeColors.Add(new DesiredColorRange(Color.FromArgb(255, (byte)r[0], (byte)g[0], (byte)b[0]),
                Color.FromArgb(255, (byte)r[1], (byte)g[1], (byte)b[1])));
        }

        public void Calculate(int n, double sourceSquare, bool isShapeInRange, bool isInstantCalculate = false)
        {
            if (MainWindow.state == MainWindow.AppState.WaitingForImage)
                return;
            this.isShapeInRange = isShapeInRange;
            stopped = false;
            this.n = n;
            shots = 0;
            accuracy = 0;
            this.sourceSquare = sourceSquare;
            picture.Refresh();
            if (isInstantCalculate)
            {
                Pass(n);
            }
            else
                timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Pass(IterationsPerPass);
        }

        private void Pass(int iter)
        {
            for (int i = 0; i <= iter; i++)
            {
                if (shots == n || shots == picture.Bitmap.PixelWidth * picture.Bitmap.PixelHeight || stopped)
                {
                    Stop();
                    break;
                }
                int x = rand.Next(0, picture.Bitmap.PixelWidth);
                int y = rand.Next(0, picture.Bitmap.PixelHeight);
                Color pixel = picture.GetPixelColor(x, y);

                shots++;
                for (int k = 0; k < RangeColors.Count; k++)
                {
                    if (!isShapeInRange)
                    {
                        if (pixel.InRange(RangeColors[k].MinColor, RangeColors[k].MaxColor))
                        {
                            picture.SetPixelColor(x, y, SourceShape);
                            break;
                        }
                        else if (k == RangeColors.Count - 1)
                        {
                            accuracy++;
                            picture.SetPixelColor(x, y, DesiredShape);
                        }
                    }
                    else
                    {
                        if (pixel.InRange(RangeColors[k].MinColor, RangeColors[k].MaxColor))
                        {
                            accuracy++;
                            picture.SetPixelColor(x, y, DesiredShape);
                            break;
                        }
                        else if (k == RangeColors.Count - 1)
                        {
                            picture.SetPixelColor(x, y, SourceShape);
                        }
                    }
                }
            }
            desiredSquare = sourceSquare * accuracy / shots;
            setLabelsData(shots, accuracy, desiredSquare);
        }

        public void Pause()
        {
            if (MainWindow.state == MainWindow.AppState.Working)
            {
                timer.Stop();
                setAppState(MainWindow.AppState.Pause);
            }
        }

        public void Resume()
        {
            if (MainWindow.state == MainWindow.AppState.Pause)
            {
                timer.Start();
                setAppState(MainWindow.AppState.Working);
            }
        }

        public void Stop()
        {
            if (MainWindow.state != MainWindow.AppState.WaitingForImage)
            {
                stopped = true;
                timer.Stop();
                setAppState(MainWindow.AppState.Ready);
            }
        }
    }
}
