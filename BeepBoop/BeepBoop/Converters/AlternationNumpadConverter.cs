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
    public class AlternationNumpadConverter : MarkupExtension, IValueConverter
    {
        private static AlternationNumpadConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new AlternationNumpadConverter();
            }
            return _converter;
        }

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int return_value = 0;

            switch(value)
            {
                case 0:
                    return_value = 6;
                    break;
                case 1:
                    return_value = 7;
                    break;
                case 2:
                    return_value = 8;
                    break;
                case 3:
                    return_value = 3;
                    break;
                case 4:
                    return_value = 4;
                    break;
                case 5:
                    return_value = 5;
                    break;
                case 6:
                    return_value = 0;
                    break;
                case 7:
                    return_value = 1;
                    break;
                case 8:
                    return_value = 2;
                    break;
            }

            if (int.TryParse(parameter as string, out int p))
                return_value += p;

            return return_value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
        #endregion
    }
}
