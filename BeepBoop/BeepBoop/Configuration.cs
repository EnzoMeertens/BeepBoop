using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace BeepBoop
{
    public enum Modifier
    {
        Numpad = 0,
        Ctrl = 1,
        Alt = 2,
    }

    /// <summary>
    /// Class containing all application configuration settings.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// The ini-file name.
        /// </summary>
        public const string INI_FILENAME = "Configuration.ini";

        #region File handling
        private static FileIniDataParser fileIniData;
        private static IniData parsedData;
        private static string _file;
        #endregion

        #region Singleton
        private static Configuration _instance = null;
        /// <summary>
        /// Singleton instance of the IniFile.
        /// </summary>
        public static Configuration Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Configuration(INI_FILENAME);

                return _instance;
            }
        }
        #endregion

        #region Application settings
        /// <summary>
        /// The start-up location of the application window.
        /// </summary>
        public Point Location { get; set; } = new Point(0, 0);

        /// <summary>
        /// Boolean value indicating whether or not keys are fully captured (not passed through).
        /// </summary>
        public bool Capture_Keys { get; set; } = false;

        /// <summary>
        /// The first physical playback device's name.
        /// </summary>
        public string Playback_Device1 { get; set; } = string.Empty;

        /// <summary>
        /// The first physical playback device's volume.
        /// </summary>
        public float Playback_Volume1 { get; set; } = 65;

        /// <summary>
        /// The second physical playback device's name.
        /// </summary>
        public string Playback_Device2 { get; set; } = string.Empty;

        /// <summary>
        /// The second physical playback device's volume.
        /// </summary>
        public float Playback_Volume2 { get; set; } = 65;
        #endregion

        /// <summary>
        /// Hidden constructor.
        /// </summary>
        private Configuration() { }

        /// <summary>
        /// Hidden constructor.
        /// </summary>
        /// <param name="file"></param>
        private Configuration(string file)
        {
            _file = file;

            fileIniData = new FileIniDataParser();
            fileIniData.Parser.Configuration.CommentString = "#";

            //Create a virgin file if needed, otherwise read from existing file.
            if (CreateFile(_file))
                parsedData = VirginData;
            else
                parsedData = fileIniData.ReadFile(_file);

            //Load application settings from file.
            Capture_Keys  = parsedData["Settings"]["Capture_Keys"] == "1";
            Location = Point.Parse(parsedData["Settings"]["Location"]);
            Playback_Device1 = parsedData["Settings"]["Playback_Device1"];
            if (float.TryParse(parsedData["Settings"]["Playback_Volume1"], out float volume1))
                Playback_Volume1 = volume1;
            Playback_Device2 = parsedData["Settings"]["Playback_Device2"];
            if (float.TryParse(parsedData["Settings"]["Playback_Volume2"], out float volume2))
                Playback_Volume2 = volume2;
        }

        /// <summary>
        /// Save configuration to file.
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            try
            {
                //Save application settings to file.
                parsedData["Settings"]["Capture_Keys"] = Capture_Keys ? "1" : "0";
                parsedData["Settings"]["Location"] = $"{Location.X},{Location.Y}";
                parsedData["Settings"]["Playback_Device1"] = Playback_Device1;
                parsedData["Settings"]["Playback_Volume1"] = Playback_Volume1.ToString("0");
                parsedData["Settings"]["Playback_Device2"] = Playback_Device2;
                parsedData["Settings"]["Playback_Volume2"] = Playback_Volume2.ToString("0");

                //Write all playback items to file.
                for(int i = 0; i < Playback_Items.Count; ++i)
                {
                    string section = Enum.GetName(typeof(Modifier), i / 9);

                    parsedData[section][$"One_Shot{(i % 9) + 1}"] = Playback_Items[i].One_Shot;
                    parsedData[section][$"Loop{(i % 9) + 1}"] = Playback_Items[i].Loop;
                    parsedData[section][$"Image{(i % 9) + 1}"] = Playback_Items[i].Image;
                }

                //Write to file.
                fileIniData.WriteFile(_file, parsedData);

                return true;
            }
            catch(Exception exc)
            {
                if (System.Windows.MessageBox.Show($"Saving of {INI_FILENAME} failed \n\n{exc.Message}\n\n Retry?", "Something went wrong.", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                    return Save();

                return false;
            }
        }

        /// <summary>
        /// All playback items.
        /// </summary>
        /// <returns></returns>
        public List<Playback_Item> Playback_Items
        {
            get
            {
                List<Playback_Item> playback_items = new List<Playback_Item>();

                playback_items.AddRange(Playback_Items_Numpad);
                playback_items.AddRange(Playback_Items_Ctrl);
                playback_items.AddRange(Playback_Items_Alt);

                return playback_items;
            }
        }

        /// <summary>
        /// Create file with "virgin" data.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool CreateFile(string file)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine(Configuration.INI_FILENAME + " not found, creating...");

                try
                {
                    fileIniData.WriteFile(file, VirginData);
                    Console.WriteLine(Configuration.INI_FILENAME + " created.");
                }
                catch (Exception exc) { Console.WriteLine(Configuration.INI_FILENAME + " creation failed: " + exc.Message); }

                return true;
            }

            return false;
        }

        private List<Playback_Item> _playback_items_numpad;
        /// <summary>
        /// All playback items when no modifier is used.
        /// </summary>
        public List<Playback_Item> Playback_Items_Numpad
        {
            get
            {
                if (_playback_items_numpad != null)
                    return _playback_items_numpad;

                _playback_items_numpad = new List<Playback_Item>();

                for (int i = 1; i <= 9; i++)
                {
                    Playback_Item audio = new Playback_Item(i + (int)Modifier.Numpad * 9);
                    audio.Modifier = Modifier.Numpad;
                    audio.One_Shot = parsedData["Numpad"]["One_Shot" + i];
                    audio.Loop = parsedData["Numpad"]["Loop" + i];
                    audio.Image = parsedData["Numpad"]["Image" + i];
                    _playback_items_numpad.Add(audio);
                }

                return _playback_items_numpad;
            }
        }

        private List<Playback_Item> _playback_items_ctrl;
        /// <summary>
        /// All playback items for the ctrl-modifier.
        /// </summary>
        public List<Playback_Item> Playback_Items_Ctrl
        {
            get
            {
                if (_playback_items_ctrl != null)
                    return _playback_items_ctrl;

                _playback_items_ctrl = new List<Playback_Item>();

                for (int i = 1; i <= 9; i++)
                {
                    Playback_Item audio = new Playback_Item(i + (int)Modifier.Ctrl * 9);
                    audio.Modifier = Modifier.Ctrl;
                    audio.One_Shot = parsedData["Ctrl"]["One_Shot" + i];
                    audio.Loop = parsedData["Ctrl"]["Loop" + i];
                    audio.Image = parsedData["Ctrl"]["Image" + i];
                    _playback_items_ctrl.Add(audio);
                }

                return _playback_items_ctrl;
            }
        }

        private List<Playback_Item> _playback_items_alt;
        /// <summary>
        /// All playback items for the alt-modifier.
        /// </summary>
        public List<Playback_Item> Playback_Items_Alt
        {
            get
            {
                if (_playback_items_alt != null)
                    return _playback_items_alt;

                _playback_items_alt = new List<Playback_Item>();

                for (int i = 1; i <= 9; i++)
                {
                    Playback_Item audio = new Playback_Item(i + (int)Modifier.Alt * 9);
                    audio.Modifier = Modifier.Alt;
                    audio.One_Shot = parsedData["Alt"]["One_Shot" + i];
                    audio.Loop = parsedData["Alt"]["Loop" + i];
                    audio.Image = parsedData["Alt"]["Image" + i];
                    _playback_items_alt.Add(audio);
                }

                return _playback_items_alt;
            }
        }

        private IniData VirginData
        {
            get
            {
                IniData virginData = new IniData();

                virginData.Configuration.CommentString = "#";

                virginData.Sections.AddSection("Settings");
                virginData.Sections.GetSectionData("Settings").Comments.Add("The following values are used for certain settings.");
                virginData["Settings"].AddKey("Capture_Keys", "0");
                virginData["Settings"].AddKey("Location", "0,0");
                virginData["Settings"].AddKey("Playback_Device1", "0");
                virginData["Settings"].AddKey("Playback_Volume1", "65");
                virginData["Settings"].AddKey("Playback_Device2", "1");
                virginData["Settings"].AddKey("Playback_Volume2", "65");

                virginData.Sections.AddSection("Numpad");
                virginData.Sections.GetSectionData("Numpad").Comments.Add("The following values are used when not using a modifier key.");

                for (int i = 1; i <= 9; i++)
                {
                    virginData["Numpad"].AddKey("One_Shot" + i, "");
                    virginData["Numpad"].AddKey("Loop" + i, "");
                    virginData["Numpad"].AddKey("Image" + i, "");
                }

                virginData.Sections.AddSection("Ctrl");
                virginData.Sections.GetSectionData("Ctrl").Comments.Add("The following values are used when using either the right or the left Ctrl key.");

                for (int i = 1; i <= 9; i++)
                {
                    virginData["Ctrl"].AddKey("One_Shot" + i, "");
                    virginData["Ctrl"].AddKey("Loop" + i, "");
                    virginData["Ctrl"].AddKey("Image" + i, "");
                }

                virginData.Sections.AddSection("Alt");
                virginData.Sections.GetSectionData("Alt").Comments.Add("The following values are used when using either the right or the left alt key.");

                for (int i = 1; i <= 9; i++)
                {
                    virginData["Alt"].AddKey("One_Shot" + i, "");
                    virginData["Alt"].AddKey("Loop" + i, "");
                    virginData["Alt"].AddKey("Image" + i, "");
                }

                return virginData;
            }
        }
    }
}
