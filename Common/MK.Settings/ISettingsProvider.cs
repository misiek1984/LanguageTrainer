namespace MK.Settings
{
    public interface ISettingsProvider
    {
        object this[string settingsKey]
        {
            get;
            set;
        }
        T GetValue<T>(string settingsKey);
        T GetValue<T>(object o, string objectKey, string settingsKey);

        void SetValue(object o, string objectKey, string settingsKey, object value);
        void SetDefault(string settingsKey, object value);

        bool Init(string directory);
        void Save();

        void InjectSettings(object o, string objectKey = null);
        void ExtractSettings(object o, string objectKey = null);

        bool HasSetting(object o, string objectKey, string settingsKey);
    }
}
