using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace BeepBoop.Converters
{
    public class ArrayItemSelectorConverter : MarkupExtension, IMultiValueConverter
    {
        private static ArrayItemSelectorConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
                _converter = new ArrayItemSelectorConverter();

            return _converter;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //Check given arguments first.
            if (!(values.Length > 1) || !(values[1] is int))
                throw new ArgumentException("given values not correct");

            object return_value = (values[0] as ObservableCollection<Playback_Item>)[(int)values[1]];

            if(return_value == null)
                return DependencyProperty.UnsetValue;

            return return_value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
