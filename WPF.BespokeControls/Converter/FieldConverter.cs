using Log4Net.Extensions.Configuration.Implementation.LogObjects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace WPF.BespokeControls.Converter
{
    public class FieldConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<LogLineField> fields = value as IEnumerable<LogLineField>;

            if (fields != null
                && parameter != null)
            {
                LogLineField field = fields.FirstOrDefault(f => f.Name == parameter.ToString());

                if (field != null)
                {
                    return field.Content;
                }

                return false;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
