using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace BeepBoop.Converters
{
    public class StringFormatConverter : MarkupExtension, IValueConverter
    {
        private static StringFormatConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
                _converter = new StringFormatConverter();

            return _converter;
        }

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value.ToString()))
                return DependencyProperty.UnsetValue;

            if (string.IsNullOrWhiteSpace(parameter as string))
                return value;

            return string.Format(parameter as string, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
        #endregion
    }
}
