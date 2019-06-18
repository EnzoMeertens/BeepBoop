using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BeepBoop
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        private Playback_Item Playback_Item_Reference { get; set; }
        public Playback_Item Playback_Item_Temp { get; set; }

        public EditWindow(Playback_Item item)
        {
            InitializeComponent();

            //Set the window title to something descriptive.
            Title = $"Edit [{(item.Modifier == Modifier.Numpad ? string.Empty : $"{item.Modifier} + ")}Numpad {(item.Number - 1) % 9 + 1}]";
            
            //Store an object reference of the specified item.
            Playback_Item_Reference = item;

            //Set a temporary playback item to the specified item.
            Playback_Item_Temp = item.Copy();

            //Set the data context.
            DataContext = this;
        }

        #region Text box double click event
        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox s = sender as TextBox;

            FileFolderDialog folderOrFileDialog = new FileFolderDialog();
            folderOrFileDialog.Dialog.ValidateNames = false;
            folderOrFileDialog.Dialog.CheckFileExists = false;
            folderOrFileDialog.Dialog.CheckPathExists = true;
            folderOrFileDialog.Dialog.Filter = (string)s.Tag == "Image" ? "Image Files (*.bmp, *.png, *.jpg, *.jpeg, *.gif)|*.bmp;*.png;*.jpg;*.jpeg;*.gif" : "Audio Files (*.wav, *.mp3, *.ogg)|*.wav;*.mp3;*.ogg|Use file name \".dir\" to use randomizer|";


            // Always default to Folder Selection.
            folderOrFileDialog.Dialog.FileName = ".dir";

            if (folderOrFileDialog.ShowDialog().Value == false)
                return;

            TextBox textBox = sender as TextBox;
            if (textBox != null)
                textBox.Text = folderOrFileDialog.SelectedPath;
        }
        #endregion

        #region Button clear events
        private void Button_Clear_OneShot_Click(object sender, RoutedEventArgs e)
        {
            Playback_Item_Temp.One_Shot = string.Empty;
        }

        private void Button_Clear_Loop_Click(object sender, RoutedEventArgs e)
        {
            Playback_Item_Temp.Loop = string.Empty;
        }

        private void Button_Clear_Image_Click(object sender, RoutedEventArgs e)
        {
            Playback_Item_Temp.Image = string.Empty;
        }
        #endregion

        #region Button done event
        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            Playback_Item_Reference.One_Shot = Playback_Item_Temp.One_Shot;
            Playback_Item_Reference.Loop = Playback_Item_Temp.Loop;
            Playback_Item_Reference.Image = Playback_Item_Temp.Image;

            this.Close();
        }
        #endregion
    }
}
