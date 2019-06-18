using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BeepBoop
{
    /// <summary>
    /// Playback item descriptor.
    /// </summary>
    public class Playback_Item : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Property string indexer.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object this[string propertyName]
        {
            get
            {
                Type myType = typeof(Playback_Item);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                return myPropInfo.GetValue(this, null);
            }

            set
            {
                Type myType = typeof(Playback_Item);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);
            }
        }

        /// <summary>
        /// The number of this playback item.
        /// </summary>
        public int Number { get; set; }

        public string ModifiedNumber { get { return $"{Modifier}+{Number - ((int)Modifier * 9)}"; } }

        /// <summary>
        /// The modifier for this playback item.
        /// </summary>
        public Modifier Modifier { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="number"></param>
        public Playback_Item(int number)
        {
            Number = number;
        }

        private string _one_shot = string.Empty;
        /// <summary>
        /// The one-shot filename.
        /// </summary>
        public string One_Shot
        {
            get { return _one_shot; }
            set
            {
                _one_shot = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("One_Shot"));
            }
        }

        private string _loop = string.Empty;
        /// <summary>
        /// The loop filename.
        /// </summary>
        public string Loop
        {
            get { return _loop; }
            set
            {
                _loop = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Loop"));
            }
        }

        private string _image = string.Empty;
        /// <summary>
        /// The image filename.
        /// </summary>
        public string Image
        {
            get { return _image; }
            set
            {
                _image = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Image"));
            }
        }

        /// <summary>
        /// Create an unreferenced copy of this playback item.
        /// </summary>
        /// <returns></returns>
        public Playback_Item Copy()
        {
            return (Playback_Item)this.MemberwiseClone();
        }
    }
}
