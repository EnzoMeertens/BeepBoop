using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BeepBoop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Playback properties
        /// <summary>
        /// Playback device descriptor.
        /// </summary>
        public struct Playback_Device
        {
            /// <summary>
            /// The hardware ID of this playback device.
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// The friendly name of this playback device.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Collection of available playback devices.
        /// </summary>
        public ObservableCollection<Playback_Device> Available_Devices { get; private set; } = new ObservableCollection<Playback_Device>();

        /// <summary>
        /// The selected playback devices.
        /// </summary>
        public ObservableCollection<Playback_Device> Playback_Devices { get; set; } = new ObservableCollection<Playback_Device>() { new Playback_Device(), new Playback_Device() };

        /// <summary>
        /// Playback volumes.
        /// </summary>
        public ObservableCollection<float> Playback_Volumes { get; set; } = new ObservableCollection<float>() { 65f, 65f };

        /// <summary>
        /// Physical one-shot playback devices.
        /// </summary>
        private WaveOut[] Playback_Devices_OneShot = new WaveOut[2] { new WaveOut(), new WaveOut() };

        /// <summary>
        /// Physical loop playback devices.
        /// </summary>
        private WaveOut[] Playback_Devices_Loop = new WaveOut[2] { new WaveOut(), new WaveOut() };

        /// <summary>
        /// Oneshot audio-file readers.
        /// </summary>
        private AudioFileReader[] Audio_File_Reader_OneShot = new AudioFileReader[2];

        /// <summary>
        /// Loop audio-file readers.
        /// </summary>
        private AudioFileReader[] Audio_File_Reader_Loop = new AudioFileReader[2];

        /// <summary>
        /// Streams for smooth looping.
        /// </summary>
        private LoopStream[] LoopStreams = new LoopStream[2];
        #endregion

        #region Keyboard capture
        /// <summary>
        /// Keyboard hook listener.
        /// </summary>
        private KeyboardHookListener Keyboard_Hook = new KeyboardHookListener(new GlobalHooker());
        #endregion

        /// <summary>
        /// List of playback items.
        /// </summary>
        public ObservableCollection<Playback_Item> Playback_Items { get; set; } = new ObservableCollection<Playback_Item>();

        #region Shuffle lists
        /// <summary>
        /// List of one-shot shuffle lists.
        /// </summary>
        private Dictionary<int, List<int>> Shuffles_OneShot = new Dictionary<int, List<int>>();

        /// <summary>
        /// List of loop shuffle lists.
        /// </summary>
        private Dictionary<int, List<int>> Shuffles_Loop = new Dictionary<int, List<int>>();

        /// <summary>
        /// The current one-shot shuffle list.
        /// </summary>
        private List<int> Shuffle_OneShot;

        /// <summary>
        /// The current loop shuffle list.
        /// </summary>
        private List<int> Shuffle_Loop;
        #endregion

        public MainWindow()
        {
            //Check if this is the first application instance.
            if (Application_Mutex.Mutex() == false)
                Environment.Exit(0); //Close this (second) process.

            InitializeComponent();

            #region Find playback devices (including full names).
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            for (int deviceId = 0; deviceId < WaveOut.DeviceCount; deviceId++)
            {
                WaveOutCapabilities capabilities = WaveOut.GetCapabilities(deviceId);
                foreach (MMDevice device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                {
                    if (device.FriendlyName.StartsWith(capabilities.ProductName))
                    {
                        Available_Devices.Add(new Playback_Device() { ID = deviceId, Name = device.FriendlyName });
                    }
                }
            }
            #endregion

            LoadSettings();

            Playback_Devices.CollectionChanged += Playback_Devices_CollectionChanged;
            Playback_Volumes.CollectionChanged += Playback_Volumes_CollectionChanged;

            Keyboard_Hook.KeyUp += Keyboard_Hook_KeyUp;
            Keyboard_Hook.KeyDown += Keyboard_Hook_KeyDown;
            Keyboard_Hook.Start();

            //Set the data context for binding.
            DataContext = this;
        }

        /// <summary>
        /// Event triggered when the volume of a playback device is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Playback_Volumes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < Audio_File_Reader_Loop.Length; ++i)
            {
                if (Audio_File_Reader_OneShot[i] != null)
                    Audio_File_Reader_OneShot[i].Volume = (float)(Playback_Volumes[i] / 100.0);
            }

            for (int i = 0; i < Audio_File_Reader_Loop.Length; ++i)
            {
                if (Audio_File_Reader_Loop[i] != null)
                    Audio_File_Reader_Loop[i].Volume = (float)(Playback_Volumes[i] / 100.0);
            }

            Configuration.Instance.Playback_Volume1 = Playback_Volumes[0];
            Configuration.Instance.Playback_Volume2 = Playback_Volumes[1];
        }

        /// <summary>
        /// Load settings from ini-file.
        /// </summary>
        /// <returns></returns>
        private bool LoadSettings()
        {
            for (int j = 0; j < Enum.GetValues(typeof(Modifier)).Length; j++)
            {
                for (int i = 1; i < 10; i++)
                {
                    Playback_Item item = Configuration.Instance.Playback_Items.Find(f => f.Number == j * 9 + i);
                    Playback_Items.Add(item);
                }
            }

            if(Available_Devices.Any(d => d.Name == Configuration.Instance.Playback_Device1))
                Playback_Devices[0] = Available_Devices.First(d => d.Name == Configuration.Instance.Playback_Device1);
            if (Available_Devices.Any(d => d.Name == Configuration.Instance.Playback_Device2))
                Playback_Devices[1] = Available_Devices.First(d => d.Name == Configuration.Instance.Playback_Device2);

            Playback_Volumes[0] = Configuration.Instance.Playback_Volume1;
            Playback_Volumes[1] = Configuration.Instance.Playback_Volume2;

            return true;
        }

        /// <summary>
        /// Event triggered when a selected playback device changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Playback_Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //Stop playback and cleanup resources.
            Stop();

            //Just instantiate them all again. This only happens once in a while anyway.
            for (int i = 0; i < Playback_Devices.Count; ++i)
            {
                Playback_Devices_OneShot[i] = new WaveOut();
                Playback_Devices_Loop[i] = new WaveOut();
                Playback_Devices_Loop[i].DeviceNumber = Playback_Devices_OneShot[i].DeviceNumber = Playback_Devices[i].ID;
                Playback_Devices_Loop[i].Volume = Playback_Devices_OneShot[i].Volume = 1.0f;
            }

            Configuration.Instance.Playback_Device1 = Playback_Devices[0].Name;
            Configuration.Instance.Playback_Volume1 = Playback_Volumes[0];

            Configuration.Instance.Playback_Device2 = Playback_Devices[1].Name;
            Configuration.Instance.Playback_Volume2 = Playback_Volumes[1];

            Configuration.Instance.Save();
        }

        /// <summary>
        /// Event triggered when a playback item is double-clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlaybackItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Check for double-click.
            if (e.ClickCount < 2)
                return;

            //Find the specified playback item.
            Playback_Item item = (sender as Border).Tag as Playback_Item;

            //Open a new edit window.
            EditWindow window = new EditWindow(item);

            window.Closed += (object s, EventArgs args) => 
            {
                Configuration.Instance.Save();
            };

            //Show the edit window.
            window.Show();
        }

        private void Keyboard_Hook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            //Set the tab control to show the Ctrl modifier.
            if ((e.KeyData & System.Windows.Forms.Keys.Control) == System.Windows.Forms.Keys.Control)
                TabControl.SelectedItem = TabItem_CTRL;

            //Set the tab control to show the Alt modifier.
            if((e.Modifiers & System.Windows.Forms.Keys.Alt) == System.Windows.Forms.Keys.Alt)
                TabControl.SelectedItem = TabItem_ALT;
        }

        private void Keyboard_Hook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            int modifier = 0;
            modifier = (e.KeyData & System.Windows.Forms.Keys.Control) == System.Windows.Forms.Keys.Control ? 9 : modifier;
            modifier = (e.Modifiers & System.Windows.Forms.Keys.Alt) == System.Windows.Forms.Keys.Alt ? 18 : modifier;

            //Only allow NumPad inputs.
            if (e.KeyValue < (int)System.Windows.Forms.Keys.NumPad0 || e.KeyValue > (int)System.Windows.Forms.Keys.NumPad9)
            {
                //Reset the tab control to NumPad.
                TabControl.SelectedItem = TabItem_NumPad;
                return;
            }

            //Pass-through the key press (or not).
            e.Handled = SettingsWindow.Instance.Catch_Keys;

            //Stop playing when NumPad0 is pressed.
            if (e.KeyCode == System.Windows.Forms.Keys.NumPad0)
            {
                Stop();
                return;
            }

            try
            {
                int index = (modifier + e.KeyValue - (int)System.Windows.Forms.Keys.NumPad1);

                string intro = Playback_Items[index].One_Shot;
                string loop = Playback_Items[index].Loop;

                #region Ghastly shuffle code
                //If there's an intro to play...
                if (string.IsNullOrWhiteSpace(intro) == false)
                {
                    //If the one-shot shuffle list does not contain the current index...
                    if (Shuffles_OneShot.ContainsKey(index) == false)
                    {
                        //Create a new shuffle list.
                        List<int> shuffle_list = new List<int>();
                        string[] files = null;

                        bool directory = File.GetAttributes(intro).HasFlag(FileAttributes.Directory);
                        if (directory)
                        {
                            //Get all one-shot files if needed (.dir specified instead of a file path).
                            files = GetFiles(intro, "*.wav|*.mp3|*.ogg", SearchOption.TopDirectoryOnly);

                            //If no audio files are found: skip the rest.
                            if (files != null && files.Length > 0)
                            {
                                //If any one-shot files are found, add them to the shuffle list.
                                for (int i = 0; i < files.Length; i++)
                                    shuffle_list.Add(i);

                                //Shuffle the list of one-shot files.
                                shuffle_list.Shuffle();

                                //Add the shuffled shuffle list to the list of shuffled shuffle lists for the current index.
                                Shuffles_OneShot.Add(index, shuffle_list);

                                //Set the current shuffle to the current index.
                                Shuffle_OneShot = Shuffles_OneShot[index];
                            }
                        }
                    }
                    else
                    {
                        //Get the shuffle list for the current index.
                        Shuffle_OneShot = Shuffles_OneShot[index];

                        //Used up all the items in the shuffle list.
                        if (Shuffle_OneShot.Count == 0)
                        {
                            string[] intro_files = null;

                            //Get all one-shot files if needed (.dir specified instead of a file path).
                            if (File.GetAttributes(intro).HasFlag(FileAttributes.Directory))
                                intro_files = GetFiles(intro, "*.wav|*.mp3|*.ogg", SearchOption.TopDirectoryOnly);

                            if (intro_files != null)
                                for (int i = 0; i < intro_files.Length; i++)
                                    Shuffle_OneShot.Add(i);
                        }

                        Shuffle_OneShot.Shuffle();
                    }
                }

                if (string.IsNullOrWhiteSpace(loop) == false)
                {
                    //Check if the loop shuffle list does not contain the desired index.
                    if (Shuffles_Loop.ContainsKey(index) == false)
                    {
                        //Create a new shuffle list.
                        List<int> shuffle_list = new List<int>();
                        string[] files = null;

                        bool directory = File.GetAttributes(loop).HasFlag(FileAttributes.Directory);
                        if (directory)
                        {
                            //Get all one-shot files if needed (.dir specified instead of a file path).
                            files = GetFiles(loop, "*.wav|*.mp3|*.ogg", SearchOption.TopDirectoryOnly);

                            //If no audio files are found: skip the rest.
                            if (files != null && files.Length > 0)
                            {
                                //If any one-shot files are found, add them to the shuffle list.
                                for (int i = 0; i < files.Length; i++)
                                    shuffle_list.Add(i);

                                //Shuffle the list of one-shot files.
                                shuffle_list.Shuffle();

                                //Add the shuffled shuffle list to the list of shuffled shuffle lists for the current index.
                                Shuffles_Loop.Add(index, shuffle_list);

                                //Set the current shuffle to the current index.
                                Shuffle_Loop = Shuffles_Loop[index];
                            }
                        }
                    }
                    else
                    {
                        Shuffle_Loop = Shuffles_Loop[index];

                        string[] loopFiles = null;

                        if (File.GetAttributes(loop).HasFlag(FileAttributes.Directory))
                            loopFiles = GetFiles(loop, "*.wav|*.mp3|*.ogg", SearchOption.TopDirectoryOnly);

                        if (Shuffle_Loop.Count == 0)
                            if (loopFiles != null)
                                for (int i = 0; i < loopFiles.Length; i++)
                                    Shuffle_Loop.Add(i);

                        Shuffle_Loop.Shuffle();
                    }
                }
                #endregion

                Play(intro, loop);
            }
            catch { }
        }

        #region Play
        /// <summary>
        /// Play the specified intro/loop (in order).
        /// </summary>
        /// <param name="intro">The path to the intro-part to play.</param>
        /// <param name="loop">The path to the loop-part to play.</param>
        public void Play(string intro, string loop)
        {
            bool play_intro = !string.IsNullOrWhiteSpace(intro);
            bool play_loop = !string.IsNullOrWhiteSpace(loop);

            //Stop all playbacks.
            Stop();

            //Play intro?
            #region Intro
            if (play_intro)
            {
                string randomizer = "";

                //Check if the shuffle list contains values.
                if (Shuffle_OneShot != null && Shuffles_OneShot.Count > 0)
                {
                    //Check if the specified one-shot path is a directory.
                    FileAttributes attr = File.GetAttributes(intro);
                    if (attr.HasFlag(FileAttributes.Directory))
                    {
                        //Get all files from the specified one-shot directory.
                        string[] files = GetFiles(intro, "*.wav|*.mp3|*.ogg", SearchOption.TopDirectoryOnly);

                        //Take a number from the shuffle list.
                        int random = Shuffle_OneShot[0];
                        //Remove the number we took.
                        Shuffle_OneShot.RemoveAt(0);

                        //Get the matching file path.
                        randomizer = files[random];
                        //Only use the file name of the path.
                        randomizer = $"\\{Path.GetFileName(randomizer)}";
                    }
                }

                //Go through all one-shot devices.
                for (int i = 0; i < Audio_File_Reader_OneShot.Length; ++i)
                {
                    string file = $"{intro}{randomizer}";
                    if (File.Exists(file) == false)
                        break;

                    //Get the audio file to play. If intro is a directory, we add a "random" audio file to the end of it.
                    Audio_File_Reader_OneShot[i] = new AudioFileReader(file);

                    //Set the volume of the playback.
                    Audio_File_Reader_OneShot[i].Volume = (float)(Playback_Volumes[i] / 100.0);

                    //Create a new playback device.
                    Playback_Devices_OneShot[i] = new WaveOut(WaveCallbackInfo.FunctionCallback());

                    //Set the playback device's output device.
                    Playback_Devices_OneShot[i].DeviceNumber = Playback_Devices[i].ID;

                    //Initialize the playback device's audio.
                    Playback_Devices_OneShot[i].Init(Audio_File_Reader_OneShot[i]);
                }
            }
            #endregion

            //Is there a loop required?
            #region Loop
            if (play_loop)
            {
                string randomizer = "";

                //Check if the shuffle list contains values.
                if (Shuffles_Loop != null && Shuffles_Loop.Count > 0)
                {
                    //Check if the specified one-shot path is a directory.
                    FileAttributes attr = File.GetAttributes(loop);
                    if (attr.HasFlag(FileAttributes.Directory))
                    {
                        //Get all files from the specified loop directory.
                        string[] files = GetFiles(loop, "*.wav|*.mp3|*.ogg", SearchOption.TopDirectoryOnly);

                        //Take a number from the shuffle list.
                        int random = Shuffle_Loop[0];
                        //Remove the number we took.
                        Shuffle_Loop.RemoveAt(0);

                        //Get the matching file path.
                        randomizer = files[random];
                        //Only use the file name of the path.
                        randomizer = $"\\{Path.GetFileName(randomizer)}";
                    }
                }

                //Go through all loop devices.
                for (int i = 0; i < Playback_Devices_Loop.Length; ++i)
                {
                    string file = $"{intro}{randomizer}";
                    if (File.Exists(file) == false)
                        break;

                    //Get the audio file to play. If loop is a directory, we add a "random" audio file to the end of it.
                    Audio_File_Reader_Loop[i] = new AudioFileReader(file);

                    //Set the volume of the playback.
                    Audio_File_Reader_Loop[i].Volume = (float)(Playback_Volumes[i] / 100.0);

                    //Create a new loop stream (for looping).
                    LoopStreams[i] = new LoopStream(Audio_File_Reader_Loop[i]);

                    //Create a new playback device.
                    Playback_Devices_Loop[i] = new WaveOut();

                    //Set the playback device's output device.
                    Playback_Devices_Loop[i].DeviceNumber = Playback_Devices[i].ID;

                    //Initialize the playback device's audio.
                    Playback_Devices_Loop[i].Init(LoopStreams[i]);
                }
            }
            #endregion

            if (play_intro) //Only play the intro...
            {
                if (Playback_Devices_OneShot[0] != null)
                    Playback_Devices_OneShot[0].Play();
                if (Playback_Devices_OneShot[1] != null)
                    Playback_Devices_OneShot[1].Play();

                //Wait for intro to finish then play the loop.
                if (play_loop)
                {
                    Playback_Devices_OneShot[0].PlaybackStopped += (pbss, pbse) =>
                    {
                        if(Playback_Devices_Loop[0] != null)
                            Playback_Devices_Loop[0].Play();
                        if (Playback_Devices_Loop[1] != null)
                            Playback_Devices_Loop[1].Play();
                    };
                }
            }
            else if (play_loop) //No intro to play, only play the loop...
            {
                if (Playback_Devices_Loop[0] != null)
                    Playback_Devices_Loop[0].Play();
                if (Playback_Devices_Loop[1] != null)
                    Playback_Devices_Loop[1].Play();
            }
        }

        /// <summary>
        /// Get all files from the specified directory matching the specified filter.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="filter"></param>
        /// <param name="search_options"></param>
        /// <returns></returns>
        public string[] GetFiles(string directory, string filter, SearchOption search_options)
        {
            // ArrayList will hold all file names
            ArrayList files = new ArrayList();

            // Create an array of filter string
            string[] multiple_filters = filter.Split('|');

            // for each filter find mathing file names
            foreach (string file_filter in multiple_filters)
            {
                // add found file names to array list
                files.AddRange(Directory.GetFiles(directory, file_filter, search_options));
            }

            // returns string array of relevant file names
            return (string[])files.ToArray(typeof(string));
        }

        #endregion

        #region Stop
        /// <summary>
        /// Stop and dispose of all audio playback resources (to prevent memory leak in NAudio).
        /// </summary>
        public void Stop()
        {
            StopWaveOuts();
            DisposeWaveOuts();

            CloseAudioFileReaders();
            DisposeAudioFileReaders();
        }
        #endregion

        #region Cleanup
        /// <summary>
        /// Cleanup all resources.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// Stop all playback.
        /// </summary>
        public void StopWaveOuts()
        {
            for (int i = 0; i < Playback_Devices_OneShot.Length; ++i)
            {
                if (Playback_Devices_OneShot[i] != null)
                    Playback_Devices_OneShot[i].Stop();
            }

            for (int i = 0; i < Playback_Devices_Loop.Length; ++i)
            {
                if (Playback_Devices_Loop[i] != null)
                    Playback_Devices_Loop[i].Stop();
            }
        }

        /// <summary>
        /// Dispose all playback resources.
        /// </summary>
        public void DisposeWaveOuts()
        {
            for (int i = 0; i < Playback_Devices_OneShot.Length; ++i)
            {
                if (Playback_Devices_OneShot[i] != null)
                    Playback_Devices_OneShot[i].Dispose();
                Playback_Devices_OneShot[i] = null;
            }

            for (int i = 0; i < Playback_Devices_Loop.Length; ++i)
            {
                if (Playback_Devices_Loop[i] != null)
                    Playback_Devices_Loop[i].Stop();
                Playback_Devices_Loop[i] = null;
            }
        }

        /// <summary>
        /// Close all audio file readers.
        /// </summary>
        /// <param name="intro"></param>
        /// <param name="loop"></param>
        public void CloseAudioFileReaders()
        {
            for (int i = 0; i < Audio_File_Reader_OneShot.Length; ++i)
            {
                if (Audio_File_Reader_OneShot[i] != null)
                    Audio_File_Reader_OneShot[i].Close();
            }
 
            for (int i = 0; i < Audio_File_Reader_Loop.Length; ++i)
            {
                if (Audio_File_Reader_Loop[i] != null)
                    Audio_File_Reader_Loop[i].Close();
            }
        }

        /// <summary>
        /// Dispose of all audio file readers.
        /// </summary>
        /// <param name="intro"></param>
        /// <param name="loop"></param>
        public void DisposeAudioFileReaders()
        {
            for (int i = 0; i < Audio_File_Reader_Loop.Length; ++i)
            {
                if (Audio_File_Reader_OneShot[i] != null)
                    Audio_File_Reader_OneShot[i].Dispose();
                Audio_File_Reader_OneShot[i] = null;
            }

            for (int i = 0; i < Audio_File_Reader_Loop.Length; ++i)
            {
                if (Audio_File_Reader_Loop[i] != null)
                    Audio_File_Reader_Loop[i].Dispose();
                Audio_File_Reader_Loop[i] = null;

                if (LoopStreams[i] != null)
                    LoopStreams[i].Close();
                LoopStreams[i] = null;
            }
        }
        #endregion

        #region Volume
        private bool mouse_captured = false;
        /// <summary>
        /// Event triggered when the volume bar notices a mouse-down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Volume_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            ProgressBar s = sender as ProgressBar;

            mouse_captured = true;

            var x = e.GetPosition(s).X;
            var ratio = x / s.ActualWidth;
            s.Value = ratio * s.Maximum;
        }

        /// <summary>
        /// Event triggered when the volume bar notices a mouse-move.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Volume_MouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (Mouse.LeftButton == MouseButtonState.Pressed && mouse_captured)
            {
                ProgressBar s = sender as ProgressBar;

                var x = e.GetPosition(s).X;
                var ratio = x / s.ActualWidth;
                s.Value = ratio * s.Maximum;
            }
        }

        /// <summary>
        /// Event triggered when the volume bar notices a mouse-up.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Volume_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            mouse_captured = false;
        }
        #endregion

        /// <summary>
        /// Event triggered when the window is closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Save all settings to file.
            Configuration.Instance.Save();

            //Unregister all keyboard events.
            Keyboard_Hook.KeyDown -= Keyboard_Hook_KeyDown;
            Keyboard_Hook.KeyUp -= Keyboard_Hook_KeyUp;

            //Stop and dispose the keyboard hook.
            Keyboard_Hook.Stop();
            Keyboard_Hook.Dispose();

            //Stop playback and clean-up all resources.
            Stop();

            //Force close.
            Environment.Exit(0);
        }

        /// <summary>
        /// Event triggered when the minimize button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Event triggered when the close button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Event triggered when the underlying window has a mouse-down event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
