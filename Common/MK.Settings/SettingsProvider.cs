using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

using MK.Utilities;

namespace MK.Settings
{
    public class SettingsProvider : ISettingsProvider
    {
        private string _path;
        private Dictionary<string, object> _settings;

        public object this[string settingsKey]
        {
            get
            {
                if (!_settings.ContainsKey(settingsKey))
                    return null;

                return _settings[settingsKey];
            }
            set
            {
                _settings[settingsKey] = value;
            }
        }
        public T GetValue<T>(string settingsKey)
        {
            return (T)this[settingsKey];
        }
        public T GetValue<T>(object o, string objectKey, string settingsKey)
        {
            return (T)this[GetKey(o, objectKey, settingsKey)];
        }

        public void SetValue(object o, string objectKey, string settingsKey, object value)
        {
            this[GetKey(o, objectKey, settingsKey)] = value;
        }
        public void SetDefault(string settingsKey, object value)
        {
            if (!_settings.ContainsKey(settingsKey))
                this[settingsKey] = value;
        }

        public bool Init(string directory)
        {
            directory.NotNullAndEmpty("directory");

            _path = Path.Combine(directory, SettingsConstans.SettingsFileName);
            if (File.Exists(_path))
            {
                try
                {
                    using (Stream s = File.OpenRead(_path))
                    {
                        _settings = (Dictionary<string, object>)new BinaryFormatter().Deserialize(s);
                    }
                }
                catch
                {
                    _settings = new Dictionary<string, object>();
                    return false;
                }
            }
            else
                _settings = new Dictionary<string, object>();

            return true;
        }
        public void Save()
        {
            using (Stream s = File.OpenWrite(_path))
            {
                new BinaryFormatter().Serialize(s, _settings);
            }
        }

        public void InjectSettings(object o, string objectKey = null)
        {
            o.NotNull("o");

            var properties = o.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var p in properties)
            {
                var attr = p.GetCustomAttributes(typeof(SettingsProperty), false);

                if (attr.Length == 0)
                    continue;

                var prop = (SettingsProperty)attr[0];

                if (IsSimple(p))
                {
                    if (this[GetKey(o, objectKey, p)] != null)
                        p.SetValue(o, this[GetKey(o, objectKey ?? prop.Key, p)], null);
                }
                else if (p.PropertyType.IsClass)
                {
                    InjectSettings(p.GetValue(o, null), objectKey ?? prop.Key);
                }
            }
        }
        public void ExtractSettings(object o, string objectKey = null)
        {
            o.NotNull("o");

            var properties = o.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var p in properties)
            {
                var attr = p.GetCustomAttributes(typeof(SettingsProperty), false);

                if (attr.Length == 0)
                    continue;

                var prop = (SettingsProperty)attr[0];

                if (IsSimple(p))
                {
                    var value = p.GetValue(o, null);
                    this[GetKey(o, objectKey ?? prop.Key, p)] = value;
                }
                else if (p.PropertyType.IsClass)
                {
                    ExtractSettings(p.GetValue(o, null), objectKey ?? prop.Key);
                }
            }
        }

        public bool HasSetting(object o, string objectKey, string settingsKey)
        {
            return _settings.ContainsKey(GetKey(o, objectKey, settingsKey));
        }



        private static bool IsSimple(PropertyInfo p)
        {
            return p.PropertyType.IsPrimitive || p.PropertyType == typeof(string) || p.PropertyType.IsEnum ||
                   (p.PropertyType.IsArray &&
                    (p.PropertyType.GetElementType().IsPrimitive || p.PropertyType.GetElementType() == typeof(string))) ||
                    (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        private static string GetKey(object o, string objectKey, string settingsKey)
        {
            return o.GetType().FullName + "." + objectKey + "." + settingsKey;
        }
        private static string GetKey(object o, string objectKey, PropertyInfo p)
        {
            return GetKey(o, objectKey, p.Name);
        }
        //private static string GetKey(string objectKey, PropertyInfo p)
        //{
        //    return p.DeclaringType.FullName + "." + objectKey + "." + p.Name;
        //}
    }
}
