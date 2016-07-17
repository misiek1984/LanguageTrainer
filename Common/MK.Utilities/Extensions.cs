using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace MK.Utilities
{
    public static class Extensions
    {
        public static void NotNull(this object o, string arg)
        {
            if (o == null)
            {
                throw new ArgumentNullException(arg);
            }
        }

        public static void NotNullAndEmpty(this string o, string arg)
        {
            if (String.IsNullOrEmpty(o))
            {
                throw new ArgumentException(arg);
            }
        }

        public static object ToDbNull(this object o)
        {
            if (o == null)
                return DBNull.Value;

            return o;
        }

        public static void PathExists(this string p)
        {
            if (!File.Exists(p) && !Directory.Exists(p))
                throw new Exception(String.Format(Resources.Res.PathNotExists, p));
        }

        public static void PathExistsOrNullOrEmpty(this string p)
        {
            if(!string.IsNullOrEmpty(p))
                PathExists(p);
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            collection.NotNull("collection");

            foreach (var o in collection)
                action(o);
        }

        public static void ForEach<T>(this IEnumerable collection, Action<T> action)
        {
            collection.NotNull("collection");

            foreach (var o in collection)
                action((T)o);
        }

        public static bool IsTheSameDate(this DateTime dt1, DateTime dt2)
        {
            return dt1.Date == dt2.Date;
        }

        public static ObservableCollection<T> AsObservable<T>(this IEnumerable<T> enumerable)
        {
            return new ObservableCollection<T>(enumerable);
        }


        public static string Dump(this object o, int depth = 0, string indent = "\t", string tab = "\t")
        {
            var sb = new StringBuilder();

            using (var sw = new StringWriter(sb))
            {
                ObjectDumper.Write(o, depth, sw, indent, tab);
            }

            return sb.ToString();
        }
        public static string DumpXml(this object o)
        {
            return Serialize(o, SerializationFormat.XmlSerializer);
        }
        public static string DumpSoap(this object o)
        {
            return Serialize(o, SerializationFormat.SoapFormatter);
        }
        public static string DumpDataContract(this object o)
        {
            return Serialize(o, SerializationFormat.DataContractSerializer);
        }


        public static string Serialize(this object o, SerializationFormat format)
        {
            var sb = new StringBuilder();

            switch (format)
            {
                case SerializationFormat.ObjectDumper:
                    using (var sw = new StringWriter(sb))
                    {
                        ObjectDumper.Write(o, 0, sw);
                    }
                    break;

                case SerializationFormat.XmlSerializer:
                    using (var sw = new StringWriter(sb))
                    {
                        var serializer = new XmlSerializer(o.GetType());
                        serializer.Serialize(sw, o);
                    }
                    break;

                case SerializationFormat.BinaryFormatter:
                    using (var ms = new MemoryStream())
                    {
                        var formatter = new BinaryFormatter();
                        formatter.Serialize(ms, 0);

                        sb.Append(Encoding.UTF8.GetString(ms.ToArray()));
                    }
                    break;

                case SerializationFormat.DataContractSerializer:
                    using (var ms = new MemoryStream())
                    {
                        var serializer = new DataContractSerializer(o.GetType());
                        serializer.WriteObject(ms, o);

                        sb.Append(Encoding.UTF8.GetString(ms.ToArray()));
                    }
                    break;

                case SerializationFormat.SoapFormatter:
                    using (var ms = new MemoryStream())
                    {
                        var serializer = new SoapFormatter();
                        serializer.Serialize(ms, o);

                        sb.Append(Encoding.UTF8.GetString(ms.ToArray()));
                    }

                    break;
            }
            return sb.ToString();
        }
        public static T Deserialize<T>(this string s, SerializationFormat format)
        {
            switch (format)
            {
                case SerializationFormat.ObjectDumper:
                    throw new NotSupportedException();

                case SerializationFormat.XmlSerializer:
                    using (var sr = new StringReader(s))
                    {
                        var serializer = new XmlSerializer(typeof (T));
                        return (T) serializer.Deserialize(sr);
                    }

                case SerializationFormat.BinaryFormatter:
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(s)))
                    {
                        var formatter = new BinaryFormatter();
                        return (T) formatter.Deserialize(ms);
                    }

                case SerializationFormat.DataContractSerializer:
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(s)))
                    {
                        var serializer = new DataContractSerializer(typeof (T));
                        return (T) serializer.ReadObject(ms);

                    }

                case SerializationFormat.SoapFormatter:
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(s)))
                    {
                        var serializer = new SoapFormatter();
                        return (T) serializer.Deserialize(ms);
                    }

                default:
                    throw new NotSupportedException();
            }
        }


        public static ImageFormat ImageFormatFromString(this string format)
        {
            var type = typeof(ImageFormat);
            var properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public);

            var res = properties.SingleOrDefault(p => p.Name.Equals(format, StringComparison.InvariantCultureIgnoreCase));

            return res == null ? null : (ImageFormat)res.GetValue(null, null);
        }
        public static string StringFromImageFormat(this ImageFormat format)
        {
            var type = typeof(ImageFormat);
            var properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public);
            var res = properties.SingleOrDefault(p =>
                {
                    var value = p.GetValue(null, null);
                    return Equals(value, format);
                });
            
            return res == null ? null : res.Name;
        }

        /// <summary>
        /// Based on https://stackoverflow.com/questions/703281/getting-path-relative-to-the-current-working-directory
        /// </summary>
        /// <param name="path"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        public static string GetRelativePath(this string path, string root)
        {
            var pathUri = new Uri(path);
            // Folders must end in a slash
            if (!root.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
            {
                root += Path.DirectorySeparatorChar;
            }
            var folderUri = new Uri(root);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }


        /// <summary>
        /// http://www.hanselman.com/blog/CommentView.aspx?guid=fde45b51-9d12-46fd-b877-da6172fe1791
        /// </summary>
        public static string CustomToString(this object obj, string format, IFormatProvider formatProvider = null)
		{
			var sb = new StringBuilder();
            var type = obj.GetType();
            var reg = new Regex(@"({)([^}]+)(})", RegexOptions.IgnoreCase);
            var mc = reg.Matches(format);
            var startIndex = 0;
			foreach(Match m in mc)
			{
                var g = m.Groups[2]; //it's second in the match between { and }
                var length = g.Index - startIndex - 1;
				sb.Append(format.Substring(startIndex,length));

                string toGet;
                var toFormat = String.Empty;
                var formatIndex = g.Value.IndexOf(":", StringComparison.Ordinal); //formatting would be to the right of a :
				if (formatIndex == -1) //no formatting, no worries
				{
					toGet = g.Value;
				}
				else //pickup the formatting
				{
					toGet = g.Value.Substring(0,formatIndex);
					toFormat = g.Value.Substring(formatIndex+1);
				}

				//first try properties
                var retrievedProperty = type.GetProperty(toGet);
				Type retrievedType = null;
				object retrievedObject = null;
				if(retrievedProperty != null)
				{
					retrievedType = retrievedProperty.PropertyType;
					retrievedObject  = retrievedProperty.GetValue(obj,null);
				}
				else //try fields
				{
                    var retrievedField = type.GetField(toGet);
					if (retrievedField != null)
					{
						retrievedType = retrievedField.FieldType;
						retrievedObject = retrievedField.GetValue(obj);
					}
				}

				if (retrievedType != null ) //Cool, we found something
				{
                    string result;
					if(toFormat == String.Empty) //no format info
					{
						result = retrievedType.InvokeMember("ToString",
							BindingFlags.Public | BindingFlags.NonPublic | 
							BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.IgnoreCase
							,null,retrievedObject,null) as string;
					}
					else //format info
					{
						result = retrievedType.InvokeMember("ToString",
							BindingFlags.Public | BindingFlags.NonPublic | 
							BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.IgnoreCase
							,null,retrievedObject,new object[]{toFormat,formatProvider}) as string;
					}
					sb.Append(result);
				}
				else //didn't find a property with that name, so be gracious and put it back
				{
					sb.Append("{");
					sb.Append(g.Value);
					sb.Append("}");
				}
				startIndex = g.Index + g.Length +1 ;
			}
			if (startIndex < format.Length) //include the rest (end) of the string
			{
				sb.Append(format.Substring(startIndex));
			}
			return sb.ToString();
		}



        public static string GetMemberName<T>(this Expression<Func<T>> expression)
        {
            var body = expression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("Lambda expression should be a member expression");

            var constant = body.Expression as ConstantExpression;
            if (constant == null)
                throw new ArgumentException("Lambda expression body should be a constant expression");

            return body.Member.Name;
        }
    }
}
