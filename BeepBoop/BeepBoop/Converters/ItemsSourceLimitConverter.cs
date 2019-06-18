using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace BeepBoop.Converters
{
    public class ItemsSourceLimitConverter : MarkupExtension, IValueConverter
    {
        private static ItemsSourceLimitConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
                _converter = new ItemsSourceLimitConverter();

            return _converter;
        }

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] parameters = null;

            if (parameter != null)
                parameters = Regex.Split(parameter.ToString(), @"(?<!\\),");

            var val = value as IEnumerable;

            if (val == null)
                return value;

            int take = 1;
            if (parameters.Length >= 1 && int.TryParse(parameters[0] as string, out take) == false)
                return value;

            if (take < 1)
                return value;

            var list = new List<object>();

            int count = 0;

            int skip = 0;
            if (parameters.Length >= 2)
                skip = int.TryParse(parameters[1] as string, out skip) ? skip : 0;

            foreach (var li in val.Cast<object>().Skip(skip))
            {
                if (++count > take)
                    break;
                list.Add(li);
            }

            return list;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
        #endregion
    }
}
