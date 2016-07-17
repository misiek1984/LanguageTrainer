using System;
using System.Windows.Data;
using System.Globalization;

namespace MK.UI.WPF.Converters
{
    [ValueConversion(typeof (int), typeof (bool))]
    public class BaseIsBitSetConverter : IValueConverter
    {
        #region Fields

        private readonly bool _and;

        #endregion

        #region Constructor

        public BaseIsBitSetConverter(bool and)
        {
            _and = and;
        }

        #endregion

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof (bool))
                throw new Exception("The target must be a boolean!");

            if (value == null)
                return false;

            if (parameter == null)
                return false;

            long number;
            if (!long.TryParse(value.ToString(), out number))
                throw new InvalidOperationException("The input value must be numeric!");

            long flag;
            if (!long.TryParse(parameter.ToString(), out flag))
                throw new InvalidOperationException("The parameter (flag) must be numeric!");

            if (flag < 0)
                flag = ~(-flag);

            if (flag == 0)
                return false;

            for (var index = 0; index < 64; ++index)
            {
                var c = 1L << index;

                if ((c & flag) == 0)
                    continue;

                if (_and)
                {
                    if ((c & number) == 0)
                        return false;
                }
                else
                {
                    if ((c & number) != 0)
                        return true;
                }
            }

            if (_and)
                return true;
         
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
