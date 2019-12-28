using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
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
        bool random = false;
        bool repeated = false;
        double oldValueVolume;
        int repeat = 0;
        Timer timer;
        private IKeyboardMouseEvents _hook;
        
        public MainWindow()
        {
            InitializeComponent();
            _player.MediaEnded += _player_MediaEnded;
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += timer_Elapsed;
            timer.Start();

            _hook = Hook.GlobalEvents();
            _hook.KeyUp += keyUp_hook;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {

            Dispatcher.Invoke(() =>
            {
                if (_player.Source != null && _player.NaturalDuration.HasTimeSpan)
                {
                    TimeSlider.Maximum = _player.NaturalDuration.TimeSpan.TotalSeconds;
                    durationTblock.Text = _player.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                    TimeSlider.Value = _player.Position.TotalSeconds;
                    currentPostTblock.Text = _player.Position.ToString(@"mm\:ss");
                    durationTblock.Text = _player.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                }

            });
        }
     

        private void _player_MediaEnded(object sender, EventArgs e)
        {
            if(random == false)
            {   
                _lastIndex++;
                if (_lastIndex == _fullPaths.Count())
                {
                    if (repeat == 0)
                    {

                        _lastIndex = -1;
                        return;

                    }
                    else
                    {
                        if (repeat == 1)
                        {
                            _lastIndex = 0;
                        }
                        else
                        {
                            if (repeated == false)
                            {
                                repeated = true;
                                _lastIndex = 0;
                            }
                            else
                            {
                                _lastIndex = -1;
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                if (positionNotPlayedYet.Count() == 0)
                {
                    _lastIndex = -1;

                    if (repeat == 0)
                    {
                        _lastIndex = -1;
                        return;
                    }
                    else
                    {
                        if (repeat == 1)
                        {
                            positionNotPlayedYet.Clear();
                            for (int i = 0; i < _fullPaths.Count(); i++)
                                positionNotPlayedYet.Add(i);
                        }
                        else
                        {
                            if (repeated == false)
                            {
                                repeated = true;
                                positionNotPlayedYet.Clear();
                                for (int i = 0; i < _fullPaths.Count(); i++)
                                    positionNotPlayedYet.Add(i);
                            }
                            else
                            {
                                _lastIndex = -1;
                                return;
                            }
                        }
                    }
                }

                Random rnd = new Random();
                int position = rnd.Next(0, positionNotPlayedYet.Count() - 1);
                _lastIndex = positionNotPlayedYet[position];
                positionNotPlayedYet.RemoveAt(position);

            }

            PlaySelectedIndex(_lastIndex);
        }

        private void PlaySelectedIndex(int i)
        {
            if (_isPlaying == false)
                return; 

            
            string filename = _fullPaths[i].FullName;
            _player.Open(new Uri(filename, UriKind.Absolute));
            _player.Play();
            _isPlaying = true;

        }

        BindingList<FileInfo> _fullPaths = new BindingList<FileInfo>();
        List<int> positionNotPlayedYet = new List<int>();

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
            _isPlaying = true;

            if (playlistListBox.SelectedIndex >= 0)
            {
                if (random == true)
                {
                    positionNotPlayedYet.Clear();
                    for (int i = 0; i < _fullPaths.Count(); i++)
                        positionNotPlayedYet.Add(i);

                    positionNotPlayedYet.Remove(playlistListBox.SelectedIndex);
                }

                if(repeat == 2)
                    repeated = false;

                _lastIndex = playlistListBox.SelectedIndex;
                PlaySelectedIndex(_lastIndex);
                showUI();
            }
            else
            {
               MessageBox.Show("No file selected!");
            }
        }
        private void showUI()
        {
            if (_isPlaying == false)
                return;

            currentPostTblock.Text = _player.Position.ToString(@"mm\:ss");
            _isPlaying = true;
        }
        private void TimeSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _player.Position = TimeSpan.FromSeconds(TimeSlider.Value);
        }

        private void Playbtn_Click(object sender, RoutedEventArgs e)
        {
            if (playlistListBox.SelectedIndex < 0)
                return;

            playOrPauseSong();
        }

        private void playOrPauseSong()
        {

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
                _isPlaying = true;
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

        private void keyUp_hook(object sneder, System.Windows.Forms.KeyEventArgs e)
        {
            if(e.Control && e.Shift && (e.KeyCode == System.Windows.Forms.Keys.S))
                playOrPauseSong();


            if (e.Control && e.Shift && (e.KeyCode == System.Windows.Forms.Keys.A))
                playPreviousSong();

            if (e.Control && e.Shift && (e.KeyCode == System.Windows.Forms.Keys.D))
                playNextSong();

        }
        private void Window_Closed(object sender, EventArgs e)
        {
            _hook.KeyUp -= keyUp_hook;
            _hook.Dispose();
        }

        private void playNextSong()
        {
            _lastIndex++;
            if (_lastIndex >= _fullPaths.Count())
                _lastIndex = 0;
            PlaySelectedIndex(_lastIndex);
            showUI();
        }
        private void playPreviousSong()
        {
            _lastIndex--;

            if (_lastIndex < 0)
                _lastIndex = _fullPaths.Count() - 1;

            PlaySelectedIndex(_lastIndex);
            showUI();
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            playNextSong();
        }

        private void Previousbtn_Click(object sender, RoutedEventArgs e)
        {
            playPreviousSong();
        }

        private void Random_Click(object sender, RoutedEventArgs e)
        {
            if (random == false)
            {
                RandomEllipse.Fill = new SolidColorBrush(Colors.Red);
                random = true;
                positionNotPlayedYet.Clear();

                for (int i = 0; i < _fullPaths.Count(); i++)
                    positionNotPlayedYet.Add(i);

                if (_lastIndex >= 0)
                    positionNotPlayedYet.Remove(_lastIndex);
            }
            else
            {
                RandomEllipse.Fill = new SolidColorBrush(Colors.Transparent);
                random = false;
            }

        }

        private void repeatbtn_Click(object sender, RoutedEventArgs e)
        {
            if(repeat == 0)
            {
                repeatEllipse.Fill = new SolidColorBrush(Colors.Red);
                repeat = 1;
                return;
            }

            if(repeat == 1)
            {
                repeatImage.Source = new BitmapImage(new Uri("Images/repeat1.png", UriKind.Relative));
                repeat = 2;
                return;
            }

            if(repeat == 2)
            {
                repeatEllipse.Fill = new SolidColorBrush(Colors.Transparent);
                repeatImage.Source = new BitmapImage(new Uri("Images/repeat.png", UriKind.Relative));
                repeat = 0;
                return;
            }
        }
    }
}
