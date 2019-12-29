using Gma.System.MouseKeyHook;
using Microsoft.Win32;
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
                    var filename = _fullPaths[_lastIndex].Name;
                    var converter = new NameConverter();
                    var shortname = converter.Convert(filename, null, null, null);
                    NameOfSong.Text = shortname.ToString();

                    TimeSlider.Maximum = _player.NaturalDuration.TimeSpan.TotalSeconds;
                    TimeSlider.Value = _player.Position.TotalSeconds;
                    currentPostTblock.Text = _player.Position.ToString(@"mm\:ss");
                    durationTblock.Text = _player.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                }

            });
        }
     

        private void _player_MediaEnded(object sender, EventArgs e)
        {
            //if(random == false)
            //{   
            //    _lastIndex++;
            //    if (_lastIndex == _fullPaths.Count())
            //    {
            //        if (repeat == 0)
            //        {
            //            stopSong();
            //            return;

            //        }
            //        else
            //        {
            //            if (repeat == 1)
            //            {
            //                _lastIndex = 0;
            //            }
            //            else
            //            {
            //                if (repeated == false)
            //                {
            //                    repeated = true;
            //                    _lastIndex = 0;
            //                }
            //                else
            //                {
            //                    stopSong();
            //                    return;
            //                }
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    if (positionNotPlayedYet.Count() == 0)
            //    {
            //        _lastIndex = -1;

            //        if (repeat == 0)
            //        {
            //            stopSong();
            //            return;
            //        }
            //        else
            //        {
            //            if (repeat == 1)
            //            {
            //                positionNotPlayedYet.Clear();
            //                for (int i = 0; i < _fullPaths.Count(); i++)
            //                    positionNotPlayedYet.Add(i);
            //            }
            //            else
            //            {
            //                if (repeated == false)
            //                {
            //                    repeated = true;
            //                    positionNotPlayedYet.Clear();
            //                    for (int i = 0; i < _fullPaths.Count(); i++)
            //                        positionNotPlayedYet.Add(i);
            //                }
            //                else
            //                {
            //                    stopSong();
            //                    return;
            //                }
            //            }
            //        }
            //    }

            //    Random rnd = new Random();
            //    int position = rnd.Next(0, positionNotPlayedYet.Count() - 1);
            //    _lastIndex = positionNotPlayedYet[position];
            //    positionNotPlayedYet.RemoveAt(position);

            //}

            //PlaySelectedIndex(_lastIndex);

            playNextSong(false);
        }

        private void PlaySelectedIndex(int i)
        {
            if (_isPlaying == false)
                return;
            if (_lastIndex < 0)
                return;
            
            string filename = _fullPaths[i].FullName;
            _player.Open(new Uri(filename, UriKind.Absolute));
            _player.Play();

        }

        BindingList<FileInfo> _fullPaths = new BindingList<FileInfo>();
        List<String> SongDirectory = new List<string>();
        List<int> positionNotPlayedYet = new List<int>();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            playlistListBox.ItemsSource = _fullPaths;
            _player.Volume = 1;
            volumeSlider.Value = volumeSlider.Maximum;
            volumeOff = true;

            loadCurrentPlaylist();
            
        }

        private void loadCurrentPlaylist()
        {
            string Dir = $"{AppDomain.CurrentDomain.BaseDirectory}currentPlaylist.txt";
            if (!File.Exists(Dir))
                return;

            var reader = new StreamReader(Dir);
            var position = double.Parse(reader.ReadLine());
            _lastIndex = int.Parse(reader.ReadLine());

            while (true)
            {
                string result = reader.ReadLine();
                if (result == null)
                    break;

                SongDirectory.Add(result);
                var info = new FileInfo(result);
                _fullPaths.Add(info);
            }
            _isPlaying = true;
            PlaySelectedIndex(_lastIndex);
            playlistListBox.SelectedIndex = _lastIndex;
            _player.Position = TimeSpan.FromSeconds(position);
            Playimg.Source = new BitmapImage(new Uri("Images/continue.png", UriKind.Relative));
            _isPlaying = false;
            _player.Pause();
        }
        private void Browserbtn_Click(object sender, RoutedEventArgs e)
        {
            var screen = new Microsoft.Win32.OpenFileDialog();
            screen.Multiselect = true;
            screen.Filter = "Music files (.mp3)|*.mp3; *.MP3";
            if (screen.ShowDialog() == true)
            {
                foreach(var item in screen.FileNames)
                {
                    SongDirectory.Add(item);
                    var info = new FileInfo(item);
                    _fullPaths.Add(info);
                }

                if(random == true)
                    randomList = createRandomList();

            }
        }
        private void playlistListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_isPlaying == false)
                Playimg.Source = new BitmapImage(new Uri("Images/pause.png", UriKind.Relative));

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
                playNextSong(true);

        }
        private void Window_Closed(object sender, EventArgs e)
        {
            _hook.KeyUp -= keyUp_hook;
            _hook.Dispose();


            if(_fullPaths.Count != 0)
            {
                string Dir = $"{AppDomain.CurrentDomain.BaseDirectory}currentPlaylist.txt";
                using (StreamWriter sw = File.CreateText(Dir))
                {
                    sw.WriteLine($"{_player.Position.TotalSeconds}");
                    sw.WriteLine($"{_lastIndex.ToString()}");
                    for (int i = 0; i < SongDirectory.Count(); i++)
                        sw.WriteLine($"{SongDirectory[i]}");
                }
            }

        }

        private void playNextSong(bool type)
        {

            if(random == false)
            {
                _lastIndex++;

                if (repeat == 1)
                {
                    if (_lastIndex == _fullPaths.Count())
                        _lastIndex = 0;
                }
                else
                {
                    if (repeat == 0)
                    {
                        if (_lastIndex == _fullPaths.Count())
                        {
                            if (type == false)
                                stopSong();
                            else
                                _lastIndex = 0;
                        }
                    }
                    else
                        _lastIndex--;
                }
            }
            else
            {
                int index=-1;

                for(int i = 0; i < randomList.Count(); i++)
                {
                    if(_lastIndex == randomList[i])
                    {
                        index = i;
                        break;
                    }
                }

                index = index + 1; 

                if (repeat == 1)
                {
                    if (index == randomList.Count())
                    {
                        if (type == false)
                            stopSong();
                        else
                            _lastIndex = randomList[0];
                    }
                    else
                    {
                        _lastIndex = randomList[index];
                    }

                }
                else
                {
                    if (repeat == 0)
                    {
                        if (index == randomList.Count)
                        {
                            if (type == false)
                                stopSong();
                            else
                                _lastIndex = randomList[0];
                        }
                        else
                        {
                            _lastIndex = randomList[index];
                        }
                    }
                }

              
            }

            PlaySelectedIndex(_lastIndex);
            showUI();
        }

        private void playPreviousSong()
        {
            if (random == false)
            {
                _lastIndex--;

                if (repeat == 1)
                {
                    if (_lastIndex == -1)
                        _lastIndex = _fullPaths.Count() -1;
                }
                else
                {
                    if (repeat == 0)
                    {
                        if (_lastIndex == -1)
                        {
                            _lastIndex = _fullPaths.Count() -1;
                        }
                    }
                    else
                        _lastIndex++;
                }
            }
            else
            {
                int index = -1;

                for (int i = 0; i < randomList.Count(); i++)
                {
                    if (_lastIndex == randomList[i])
                    {
                        index = i;
                        break;
                    }
                }

                index = index - 1;

                if (repeat == 1)
                {
                    if (index == -1)
                    {

                            _lastIndex = randomList[randomList.Count()-1];
                    }
                    else
                    {
                        _lastIndex = randomList[index];
                    }

                }
                else
                {
                    if (repeat == 0)
                    {
                        if (index == -1)
                        {
                                _lastIndex = randomList[randomList.Count()-1];
                        }
                        else
                        {
                            _lastIndex = randomList[index];
                        }
                    }
                }


            }
            PlaySelectedIndex(_lastIndex);
            showUI();
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            playNextSong(true);
        }

        private void Previousbtn_Click(object sender, RoutedEventArgs e)
        {
            playPreviousSong();
        }
        List<int> randomList;
        private List<int> createRandomList()
        {
            List<int> randomList1 = new List<int>();
            bool ok = false;

            if(_lastIndex != -1)
            {
                randomList1.Add(_lastIndex);
                ok = true;
            }

            for (int i = 0; i < _fullPaths.Count(); i++)
            { 
                if(_lastIndex!= i)
                    randomList1.Add(i);
            }


            for (int i = 1; i < _fullPaths.Count(); i++)
            {
                Random rnd = new Random();
                int position1;
                if (ok == true)
                    position1 = rnd.Next(1, _fullPaths.Count() - 1);
                else
                    position1 = rnd.Next(0, _fullPaths.Count() - 1);

                if (position1 != i)
                {
                    int temp = randomList1[position1];
                    randomList1[position1] = randomList1[i];
                    randomList1[i] = temp;
                }
            }

            String result = "";
            for (int i = 0; i < randomList1.Count(); i++)
                result = result + randomList1[i].ToString();

            MessageBox.Show(result);
            return randomList1;
        }
        private void Random_Click(object sender, RoutedEventArgs e)
        {
            if (random == false)
            {
                RandomEllipse.Fill = new SolidColorBrush(Colors.Red);
                random = true;

                 randomList = createRandomList();
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

        private void stopSong()
        {
            durationTblock.Text = "00:00";
            currentPostTblock.Text = "";
            NameOfSong.Text = "";
            _player.Stop();
            TimeSlider.Value = 0;
            _isPlaying = false;
            _lastIndex = -1;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            stopSong();
        }

        private void resetPlaylist()
        {
            durationTblock.Text = "";
            currentPostTblock.Text = "";
            NameOfSong.Text = "";
            _player.Stop();
            TimeSlider.Value = 0;
            _isPlaying = false;
            _lastIndex = -1;
            _fullPaths.Clear();
            SongDirectory.Clear();
        }

        private void Remove_item_file(object sender, RoutedEventArgs e)
        {

                if (playlistListBox.SelectedIndex == _lastIndex)
                {
                     stopSong();
                }

                SongDirectory.RemoveAt(playlistListBox.SelectedIndex);

                _fullPaths.RemoveAt(playlistListBox.SelectedIndex);

                if(random == true)
                    randomList = createRandomList();


        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            resetPlaylist();
        }

        private void savePlaylist()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt";
            saveFileDialog1.CreatePrompt = true;
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.DefaultExt = "txt";
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == true)
            {
                List<string> stringFile = new List<string>();

                stringFile.Add($"{_lastIndex.ToString()}");
                for (int i = 0; i < SongDirectory.Count(); i++)
                    stringFile.Add($"{SongDirectory[i]}");

                File.WriteAllLines(saveFileDialog1.FileName, stringFile);
                MessageBox.Show("Playlist saved");
            }
        }
        private void SavePlayList_Click(object sender, RoutedEventArgs e)
        {

            savePlaylist();
        }


        private void LoadPlayList_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "txt files (*.txt)|*.txt";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                stopSong();
                var converter = new NameConverter();
                string Dir = openFileDialog.FileName;

                if (!File.Exists(Dir))
                    return;

                var reader = new StreamReader(Dir);
                _lastIndex = int.Parse(reader.ReadLine());
                SongDirectory.Clear();
                _fullPaths.Clear();

                while (true)
                {
                    string result = reader.ReadLine();
                    if (result == null)
                        break;

                    SongDirectory.Add(result);
                    var info = new FileInfo(result);
                    _fullPaths.Add(info);
                }
            }

        }
    }
}
