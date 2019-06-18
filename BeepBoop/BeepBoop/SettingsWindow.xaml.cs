using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        /// <summary>
        /// Do not allow keys to pass through.
        /// </summary>
        public bool Catch_Keys = false;

        private static SettingsWindow _instance = null;
        /// <summary>
        /// Singleton instance of settings.
        /// </summary>
        public static SettingsWindow Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SettingsWindow();

                return _instance;
            }
        }

        private SettingsWindow()
        {
            InitializeComponent();
        }
    }
}
