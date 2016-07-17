using System;

namespace MK.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingsProperty: Attribute
    {
        public string Key { get; private set; }

        public SettingsProperty()
        {}

        public SettingsProperty(string key)
        {
            Key = key;
        }
    }
}
