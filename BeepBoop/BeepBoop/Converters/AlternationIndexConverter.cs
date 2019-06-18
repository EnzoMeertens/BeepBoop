using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace BeepBoop.Converters
{
    public class AlternationIndexConverter : MarkupExtension, IValueConverter
    {
        private static AlternationIndexConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new AlternationIndexConverter();
            }
            return _converter;
        }

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool valid = int.TryParse(parameter as string, out int p);
            if (parameter == null || valid == false)
                parameter = 0;
            return (int)value + p;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
        #endregion
    }
}
