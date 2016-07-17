using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MK.Utilities
{
    public class DescriptionsProvider<T>
    {
        #region Fields & Properties

        public const char DefaultMappingFileDescriptionSeperator = ';';

        public Dictionary<T, string> Descriptions{ get; private set; }

        #endregion

        #region Constructor

        public DescriptionsProvider()
        {
            Descriptions = new Dictionary<T, string>();
        }

        #endregion

        #region Public Methods

        public void Parse(string path, char seperator = DefaultMappingFileDescriptionSeperator)
        {
            Descriptions.Clear();

            if (String.IsNullOrEmpty(path) || !File.Exists(path))
                return;

            Parse(File.ReadAllLines(path), seperator);
        }

        public void Parse(string[] lines, char seperator = DefaultMappingFileDescriptionSeperator)
        {
            lines.NotNull("lines");

            Descriptions.Clear();

            var currentLineNumber = 0;
            foreach (var line in lines)
            {
                currentLineNumber++;

                ParseLine(line, currentLineNumber, seperator);
            }
        }

        public string GetDescription(T value)
        {
            string res;
            Descriptions.TryGetValue(value, out res);
            return res;
        }

        #endregion
        #region Private Methods

        private void ParseLine(string line, int currentLineNumber, char seperator)
        {
            var res = line.Split(seperator);

            if (res.Length != 2)
                throw new Exception(String.Format("{0} line has wrong format", currentLineNumber));

            object key;
            if (typeof(T) == typeof (double))
            {
                double doubleKey;
                if (!double.TryParse(res[0], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out doubleKey))
                    throw new Exception(
                        String.Format("{0} in {1} line is not a proper double number", line, currentLineNumber));
                key = doubleKey;
            }
            else if (typeof(T) == typeof(float))
            {
                float floatKey;
                if (!float.TryParse(res[0], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out floatKey))
                    throw new Exception(
                        String.Format("{0} in {1} line is not a float float number", line, currentLineNumber));
                key = floatKey;
            }
            else if (typeof(T) == typeof(int))
            {
                int intKey;
                if (!int.TryParse(res[0], NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out intKey))
                    throw new Exception(
                        String.Format("{0} in {1} line is not a proper int number", line, currentLineNumber));
                key = intKey;
            }
            else if (typeof(T) == typeof(long))
            {
                long longKey;
                if (!long.TryParse(res[0], NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out longKey))
                    throw new Exception(
                        String.Format("{0} in {1} line is not a proper long number", line, currentLineNumber));
                key = longKey;
            }
            else if (typeof(T) == typeof(string))
            {
                key = res[0];
            }
            else 
                throw new NotSupportedException(String.Format("'{0}' is not supported!", typeof(T).FullName));

            string value;
            if (Descriptions.TryGetValue((T)key, out value) && value != res[1])
                throw new Exception(String.Format("The key '{0}' is associated with two different descriptions: '{1}', '{2}'", (object)key, value, res[1]));

            Descriptions[(T)key] = res[1];
        }

        #endregion
    }
}
