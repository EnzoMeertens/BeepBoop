using System;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Windows.Interop;

namespace BeepBoop
{
    public class FileFolderDialog : CommonDialog
    {
        public OpenFileDialog Dialog { get; set; } = new OpenFileDialog();

        public new bool? ShowDialog()
        {
            //Set validate names to false otherwise windows will not let you select "Select Folder".
            Dialog.ValidateNames = false;
            Dialog.CheckFileExists = false;
            Dialog.CheckPathExists = true;

            try
            {
                //Set initial directory (used when dialog.FileName is set from outside).
                if (!string.IsNullOrEmpty(Dialog.FileName))
                    Dialog.InitialDirectory = Directory.Exists(Dialog.FileName) ? Dialog.FileName : Path.GetDirectoryName(Dialog.FileName);
            }
            catch (Exception ex)
            {
                return false;
            }

            // Always default to Select Folder
            Dialog.FileName = ".dir";

            return Dialog.ShowDialog();
        }

        /// <summary>
        // Helper property. Parses FilePath into either folder path (if Select Folder is set)
        // or returns file path
        /// </summary>
        public string SelectedPath
        {
            get
            {
                try
                {
                    if (Dialog.FileName != null && (Dialog.FileName.EndsWith(".dir") || !File.Exists(Dialog.FileName)) && !Directory.Exists(Dialog.FileName))
                        return Path.GetDirectoryName(Dialog.FileName);

                    return Dialog.FileName;
                }
                catch (Exception ex)
                {
                    return Dialog.FileName;
                }
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    Dialog.FileName = value;
                }
            }
        }

        /// <summary>
        /// When multiple files are selected returns them as semi-colon seprated string
        /// </summary>
        public string SelectedPaths
        {
            get
            {
                if (Dialog.FileNames == null || Dialog.FileNames.Length <= 1) return null;

                StringBuilder sb = new StringBuilder();

                foreach (string fileName in Dialog.FileNames)
                {
                    try
                    {
                        if (File.Exists(fileName))
                            sb.Append(fileName + ";");
                    }
                    catch (Exception ex)
                    {
                        // Go to next
                    }
                }
                return sb.ToString();
            }
        }

        public override void Reset()
        {
            Dialog.Reset();
        }

        protected override bool RunDialog(IntPtr hwndOwner)
        {
            return true;
        }
    }
}
