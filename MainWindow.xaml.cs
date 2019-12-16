using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WpfApp_Windows_Project3_MultimediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MediaPlayer _player = new MediaPlayer();
        int _lastIndex = -1;
        bool _isPlaying = false;
        bool volumeOff = false;
        Timer timer;
        
        public MainWindow()
        {
            InitializeComponent();
            _player.MediaEnded += _player_MediaEnded;
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }
        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {

            Dispatcher.Invoke(() =>
            {
                if (_player.Source != null)
                {                  
                    TimeSlider.Value = _player.Position.TotalSeconds;
                    currentPostTblock.Text = _player.Position.ToString(@"mm\:ss");
                    durationTblock.Text = _player.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                }

            });
        }
     

        private void _player_MediaEnded(object sender, EventArgs e)
        {
            _lastIndex++;
            PlaySelectedIndex(_lastIndex);
        }

        private void PlaySelectedIndex(int i)
        {

            string filename = _fullPaths[i].FullName;

            _player.Open(new Uri(filename, UriKind.Absolute));
            System.Threading.Thread.Sleep(500);
            _player.Play();
            _isPlaying = true;
        }


        BindingList<FileInfo> _fullPaths = new BindingList<FileInfo>();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            playlistListBox.ItemsSource = _fullPaths;
            _player.Volume = 1;
            volumeSlider.Value = volumeSlider.Maximum;
            volumeOff = true;

        }

        private void Browserbtn_Click(object sender, RoutedEventArgs e)
        {
            var screen = new Microsoft.Win32.OpenFileDialog();
            if (screen.ShowDialog() == true)
            {
                var info = new FileInfo(screen.FileName);
                _fullPaths.Add(info);
            }
        }
        private void playlistListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
       
            if (playlistListBox.SelectedIndex >= 0)
            {
                _lastIndex = playlistListBox.SelectedIndex;
                PlaySelectedIndex(_lastIndex);
                TimeSlider.Maximum = _player.NaturalDuration.TimeSpan.TotalSeconds;

                currentPostTblock.Text = _player.Position.ToString(@"mm\:ss");
                durationTblock.Text = _player.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                _isPlaying = true;
            }
            else
            {
               MessageBox.Show("No file selected!");
            }
        }

        private void TimeSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _player.Position = TimeSpan.FromSeconds(TimeSlider.Value);
        }

        private void Playbtn_Click(object sender, RoutedEventArgs e)
        {
            if (playlistListBox.SelectedIndex < 0)
                return;

            if (_isPlaying == true)
            {
                _player.Pause();
                Playimg.Source = new BitmapImage(new Uri("Images/continue.png", UriKind.Relative));
                _isPlaying = false;
            }
            else
            {
                _player.Play();
                Playimg.Source = new BitmapImage(new Uri("Images/pause.png", UriKind.Relative));
                _isPlaying = true ;
            }
        }

        private void volumeSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _player.Volume = volumeSlider.Value;

            if (volumeOff == true)
            {
                _player.Volume = volumeSlider.Value;
                volumeImg.Source = new BitmapImage(new Uri("Images/volumelevel.png", UriKind.Relative));
                volumeOff = false;
            }

            if (_player.Volume == 0)
            {
                volumeImg.Source = new BitmapImage(new Uri("Images/volumeoff.png", UriKind.Relative));
                volumeOff = true;
            }

        }

        double oldValueVolume;
        private void volumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (volumeOff == true)
            {
                _player.Volume = oldValueVolume;
                volumeSlider.Value = oldValueVolume;
                volumeImg.Source = new BitmapImage(new Uri("Images/volumelevel.png", UriKind.Relative));
                volumeOff = false;
            }
            else
            {
                oldValueVolume = _player.Volume;
                _player.Volume = 0;
                volumeSlider.Value = 0;
                volumeImg.Source = new BitmapImage(new Uri("Images/volumeoff.png", UriKind.Relative));
                volumeOff = true;

            }
        }
    }
}
