﻿// #define MP3BUG -- helping find slow to play bug
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;
using TimeSpan = System.TimeSpan;

namespace Lomont.Scoreganizer.WPF.Views
{
    /// <summary>
    /// Interaction logic for MediaPlayerControl.xaml
    /// </summary>
    public partial class MediaPlayerControl : UserControl
    {
        public MediaPlayerControl()
        {
            InitializeComponent();
#if !MP3BUG
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += (o, e) => Tick();
            MediaPlayer.MediaFailed += MediaFailedEvent;
#endif
        }

        void MediaFailedEvent(object sender, ExceptionRoutedEventArgs e)
        {
            Trace.TraceError($"{e.ErrorException}");
        }

        DispatcherTimer timer = new DispatcherTimer();

        public static DependencyProperty FilepathProperty =
            DependencyProperty.Register("Filepath", typeof(string), typeof(MediaPlayerControl));

        public string Filepath
        {
            get => (string)GetValue(FilepathProperty);
            set => SetValue(FilepathProperty, value);
        }
        void Tick()
        {
#if !MP3BUG
            if (MediaPlayer.NaturalDuration.HasTimeSpan)
            {
                var dur = MediaPlayer.NaturalDuration.TimeSpan;
                var cur = MediaPlayer.Position;
                PositionText.Text = $"{cur:mm\\:ss}/{dur:mm\\:ss}";

                if (!isDragging)
                {
                    PositionSlider.Value = cur.TotalSeconds;

                    // perform looping
                    if (endLoopTime.TotalSeconds - cur.TotalSeconds < 0.25)
                        MediaPlayer.Position = startLoopTime;
                }

                DrawVolume();
            }
#endif
        }
        void PositionSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            return; // todo - causes hiccups - fix later
            if (MediaPlayer.NaturalDuration.HasTimeSpan && MediaPlayer.ScrubbingEnabled)
            {
                var dur = MediaPlayer.NaturalDuration.TimeSpan;
                var cur = MediaPlayer.Position;

                var ts = dur.TotalSeconds;
                var cs = cur.TotalSeconds;

                var v = PositionSlider.Value; // 0-1
                MediaPlayer.Position = TimeSpan.FromSeconds(cs * v);

                //if (ts != 0)
                //    PositionSlider.Value = cs / ts;
            }
        }

        void StopButtonClicked(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
        }
        void RewindButtonClicked(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Position = startLoopTime; //  TimeSpan.Zero;
        }
        bool isPaused;
        void PauseButtonClicked(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer.CanPause)
            {
                if (!isPaused)
                    MediaPlayer.Pause();
                else
                    MediaPlayer.Play();
                isPaused = !isPaused;
            }
        }

        double[] speeds = new[] {0.125, 0.25, 0.5, 0.75, 1.0, 1.25, 1.5, 1.75, 2.0, 3.0, 4.0, 5.0, 10.0};

        double NextSpeed(double current, int dir)
        {
            double tolerance = 0.001;
            int i = 0;
            while (i < speeds.Length && tolerance < Math.Abs(speeds[i] - current) )
                ++i;
            i += dir;
            if (i >= speeds.Length)
                i = speeds.Length - 1;
            if (i < 0) i = 0;
            return speeds[i];
        }

        void FasterButtonClicked(object sender, RoutedEventArgs e)
        {
            MediaPlayer.SpeedRatio = NextSpeed(MediaPlayer.SpeedRatio, 1);
            PlaybackSpeedText.Text = $"{MediaPlayer.SpeedRatio:F3}";
        }
        void SlowerButtonClicked(object sender, RoutedEventArgs e)
        {
            MediaPlayer.SpeedRatio = NextSpeed(MediaPlayer.SpeedRatio, -1);
            PlaybackSpeedText.Text = $"{MediaPlayer.SpeedRatio:F3}";
        }

        void PlayButtonClicked(object sender, RoutedEventArgs e) 
        {
            var path = Filepath;
            if (String.IsNullOrEmpty(path))
                return;
            MediaPlayer.Source = new Uri(path);
            MediaPlayer.Play();

#if !MP3BUG
            NowPlayingText.Text = Path.GetFileName(path);
            MediaPlayer.Position = startLoopTime;
#endif

            //MediaPlayer.CanPause
            //MediaPlayer.HasAudio
            //MediaPlayer.HasVideo
            //MediaPlayer.NaturalVideoHeight
            //MediaPlayer.NaturalVideoWidth
            //MediaPlayer.ScrubbingEnabled
            //MediaPlayer.SpeedRatio;
            //MediaPlayer.MediaEnded
            //MediaPlayer.MediaFailed
            //MediaPlayer.MediaOpened
        }

        void DrawVolume()
        {
            if (MediaPlayer != null)
            {
                var vol = VolumeSlider.Value;
                MediaPlayer.Volume = vol;
                VolumeSliderText.Text = $"Vol {(int)(vol * 100)}%";
            }
        }

        void VolumeSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            DrawVolume();
        }

        void MediaOpenedEvent(object sender, RoutedEventArgs e)
        {
#if !MP3BUG
            if (MediaPlayer.NaturalDuration.HasTimeSpan)
            {
                var ts = MediaPlayer.NaturalDuration.TimeSpan;
                PositionSlider.Maximum = ts.TotalSeconds;
                PositionSlider.SmallChange = 1;
                PositionSlider.LargeChange = Math.Min(10, ts.Seconds / 10);
            }
            timer.Start();
#endif
        }

        bool isDragging = false;
        void PositionDragStarted(object sender, DragStartedEventArgs e)
        {
            isDragging = true;
        }

        void PositionDragCompleted(object sender, DragCompletedEventArgs e)
        {
            isDragging = false;
            MediaPlayer.Position = TimeSpan.FromSeconds(PositionSlider.Value);
        }

#region Looping

        TimeSpan startLoopTime = TimeSpan.Zero;
        TimeSpan endLoopTime = TimeSpan.FromDays(100);
        private void StartLoopCheckboxChecked(object sender, RoutedEventArgs e)
        {
            if (startLoopCheckbox.IsChecked == true)
            {
                startLoopTime = MediaPlayer.Position;
                startLoopCheckbox.Content = $"Start loop {startLoopTime:mm\\:ss}";
            }
            else
            {
                startLoopTime = TimeSpan.Zero;
                startLoopCheckbox.Content = "Start loop";
            }
        }

        private void EndLoopCheckboxChecked(object sender, RoutedEventArgs e)
        {
            if (endLoopCheckbox.IsChecked == true)
            {
                endLoopTime = MediaPlayer.Position;
                endLoopCheckbox.Content = $"End loop {endLoopTime:mm\\:ss}";
            }
            else
            {
                endLoopTime = TimeSpan.FromDays(100);
                endLoopCheckbox.Content = "End loop";
            }
        }
#endregion

    }
}
