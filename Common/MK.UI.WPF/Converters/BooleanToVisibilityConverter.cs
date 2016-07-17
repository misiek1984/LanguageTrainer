using System;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace MK.UI.WPF.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        [Flags]
        private enum Config
        {
            Reverse = 1,
            HiddenInsteadOfCollapse = 2
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flag = false;

            if (value is bool)
            {
                flag = (bool)value;
            }

            if (parameter != null)
            {
                Config c = (Config)Int32.Parse((string)parameter);

                if ((c & Config.Reverse) != 0 )
                {
                    flag = !flag;
                }

                if ((c & Config.HiddenInsteadOfCollapse) != 0)
                {
                    return flag ? Visibility.Visible : Visibility.Hidden;
                }
            }

            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var back = value is Visibility && (Visibility)value == Visibility.Visible;

            if (parameter != null)
            {
                if ((bool)parameter)
                {
                    back = !back;
                }
            }

            return back;
        }
    }
}
