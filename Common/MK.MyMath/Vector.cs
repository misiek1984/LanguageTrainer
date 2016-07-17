using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MK.MyMath
{
    public class VectorI : Vector<int>
    {
    }

    public class VectorL : Vector<long>
    {
    }

    public class VectorF : Vector<float>
    {
    }

    public class VectorD : Vector<double>
    { 
    }

    public class Vector<T> : List<T>
    {
        private Dictionary<int, string> _descriptions;

        private static char _vectorElementsSeperator = ',';
        public static char VectorElementsSeperator
        {
            get { return _vectorElementsSeperator; }
            set { _vectorElementsSeperator = value; }
        }

        private static char _descriptionSeperator = ';';
        public static char DescriptionSeperator
        {
            get { return _descriptionSeperator; }
            set { _descriptionSeperator = value; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            for (var index = 0; index < Count; ++index)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}", this[index]);

                if (_descriptions != null && _descriptions.ContainsKey(index))
                {
                    sb.Append(DescriptionSeperator);
                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0}", _descriptions[index]);
                }

                sb.Append(VectorElementsSeperator).Append(" ");
            }

            sb.Length -= 2;

            return sb.ToString();
        }

        public void SetDescription(int index, string description)
        {
            if(String.IsNullOrEmpty(description))
                return;

            if(_descriptions == null)
                _descriptions = new Dictionary<int, string>();

            if(index >= Count)
                throw new IndexOutOfRangeException();

            _descriptions[index] = description;
        }
    }
}
