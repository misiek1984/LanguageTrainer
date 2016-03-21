using System.IO;
using System;

namespace LanguageTrainer.Common
{
    public static class Constans
    {
        public const string LiveClientId = "LiveClientId";

        public const string UseEmbeddedHttpServer = "useEmbeddedHttpServer";

        public const string RavenMode = "-Raven";

        public static string EnableTiming = "-Timing";

        public static readonly string DropboxKey = "DropboxKey";

        public static readonly string DropboxSecret = "DropboxSecret";

        public const string ApplicationFolderNameRegex = @".*-appFolderName:(?<Name>[a-zA-F][a-zA-F0-9]+).*";
    }

    public class ApplicationPaths
    {
        private static string _applicationFolderName = "LanguageTrainer";
        public static string ApplicationFolderName
        {
            get { return _applicationFolderName; }
            set
            {
                _applicationFolder = null;
                _additionalSettings = null;
                _ravenDataFolder = null;
                _sqlCeDataFolder = null;
                _sqlCeData = null;
                _applicationFolderName = value;
            }
        }

        private static string _applicationFolder;
        public static string ApplicationFolder
        {
            get
            {
                if (_applicationFolder == null)
                    _applicationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationFolderName);

                return _applicationFolder;
            }
        }

        private static string _additionalSettings;
        public static string AdditionalSettings
        {
            get
            {
                if (_additionalSettings == null)
                    _additionalSettings = Path.Combine(ApplicationFolder, "LanguageTrainer.config");

                return _additionalSettings;
            }
        }

        private static string _ravenDataFolder;
        public static string RavenDataFolder
        {
            get
            {
                if (_ravenDataFolder == null)
                    _ravenDataFolder = Path.Combine(ApplicationFolder, "RavenData");

                return _ravenDataFolder;
            }
        }

        private static string _sqlCeDataFolder;
        public static string SqlCeDataFolder
        {
            get
            {
                if (_sqlCeDataFolder == null)
                    _sqlCeDataFolder = Path.Combine(ApplicationFolder, "SqlCeData");

                return _sqlCeDataFolder;
            }
        }

        private static string _sqlCeData;
        public static string SqlCeData
        {
            get
            {
                if (_sqlCeData == null)
                    _sqlCeData = Path.Combine(SqlCeDataFolder, "Data.sdf");

                return _sqlCeData;
            }
        }
    }
}

