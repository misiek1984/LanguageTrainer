using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Configuration;
using System.Reflection;
using System.Windows.Threading;

using MK.Data;
using MK.Dropbox;
using MK.Live;
using MK.Utilities;
using MK.Logging;
using MK.Settings;
using MK.UI.WPF;
using MK.Data.Xml;

using LanguageTrainer.Interfaces;
using LanguageTrainer.Common;
using LanguageTrainer.VM;
using LanguageTrainer.DataAccess;
using LanguageTrainer.Entities;
using LanguageTrainer.Logic;
using LanguageTrainer.Logic.Translator;

using StructureMap;


namespace LanguageTrainer
{
    public class App : Application
    {
        public App()
        {
            var regex = new Regex(Constans.ApplicationFolderNameRegex, RegexOptions.IgnoreCase);
            var match = regex.Match(Environment.CommandLine);
            if (match.Success)
            {
                Log.LogMessage("Application folder name: " + match.Groups[1].Value);
                ApplicationPaths.ApplicationFolderName = match.Groups[1].Value;
            }


            ServiceProvider.Register<IWindowService, WindowService>();
            ServiceProvider.Register<IDispatcherService, DispatcherService>(true, o => o.Ctor<Dispatcher>("dispatcher").Is(Current.Dispatcher));
            ServiceProvider.Register<IEnumerablePersister<LanguageInfo>, XMLEnumerablePersister<LanguageInfo>>();
            ServiceProvider.Register<IDateTimeProvider, DateTimeProvider>();
            ServiceProvider.Register<ILanguagesProvider, LanguagesProvider>();
            ServiceProvider.Register<IEntityCreator, EntityCreator>();
            ServiceProvider.Register<IPersister<MainEntity>, XMLPersister<MainEntity>>(true, o => o.Ctor<string>("path").Is("."));
            ServiceProvider.Register<ISpeechEngine, SpeechEngine>();
            ServiceProvider.Register<ITranslator, Translator>();
            ServiceProvider.Register<ISpecialSymbolsProvider, SpecialSymbolsProvider>();
            ServiceProvider.Register<IDataAccess, EFDataAccess>();
            ServiceProvider.Register<IMerger, Merger>();
            ServiceProvider.Register<IPersister<DropboxToken>, XMLPersister<DropboxToken>>(true, o => o.Ctor<string>("path").Is(ApplicationPaths.ApplicationFolder));
            ServiceProvider.Register<IDropboxProvider, DropboxProvider>();
            ServiceProvider.Register<IPersister<LiveToken>, XMLPersister<LiveToken>>(true, o => o.Ctor<string>("path").Is(ApplicationPaths.ApplicationFolder));
            ServiceProvider.Register<ILiveProvider, LiveProvider>();
            ServiceProvider.Register<ISettingsProvider, MK.Settings.SettingsProvider>();

            //var con = new Container(_ =>
            //{
            //    _.For<IWindowService>().Use<WindowService>().Singleton();
            //    _.For<IDispatcherService>().Use(new DispatcherService(Current.Dispatcher));
            //    _.For<IDateTimeProvider>().Use<DateTimeProvider>().Singleton();
            //    _.For<IEnumerablePersister<LanguageInfo>>().Use<XMLEnumerablePersister<LanguageInfo>>().Singleton();
            //    _.For<ILanguagesProvider>().Use<LanguagesProvider>().Singleton();
            //    _.For<ISettingsProvider>().Use<MK.Settings.SettingsProvider>().Singleton();
            //    _.For<IEntityCreator>().Use<EntityCreator>().Singleton();
            //    _.For<IPersister<MainEntity>>().Use<XMLPersister<MainEntity>>().Singleton().Ctor<string>("path").Is(".");
            //    _.For<ISpeechEngine>().Use<SpeechEngine>().Singleton();
            //    _.For<ITranslator>().Use<Translator>().Singleton();
            //    _.For<ISpecialSymbolsProvider>().Use<SpecialSymbolsProvider>().Singleton();
            //    _.For<IDataAccess>().Use<EFDataAccess>().Singleton();
            //    _.For<IMerger>().Use<Merger>().Singleton();

            //    _.For<IPersister<DropboxToken>>().Use<XMLPersister<DropboxToken>>().Ctor<string>("path").Is(ApplicationPaths.ApplicationFolder);
            //    _.For<IDropboxProvider>().Use<DropboxProvider>().Singleton();

            //    _.For<IPersister<LiveToken>>().Use<XMLPersister<LiveToken>>().Ctor<string>("path").Is(ApplicationPaths.ApplicationFolder);
            //    _.For<ILiveProvider>().Use<LiveProvider>().Singleton();

            //    _.For<ISettingsProvider>().Use<MK.Settings.SettingsProvider>().Singleton();
            //});

            //ServiceProvider.Use(con);

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ServiceProvider.GetService<IWindowService>().DoBackgroundTask(Init, centerOwner: false);

            ServiceProvider.GetService<IWindowService>().ShowProgress(centerOwner: false);
            ServiceProvider.GetService<IWindowService>().ShowDialog(new MainWindowVM(), canResize: true, initialyVisible: false);
        }

        private static void TestServices()
        {
            ServiceProvider.Get<IWindowService>();
            ServiceProvider.Get<IDispatcherService>();
            ServiceProvider.Get<IDateTimeProvider>();
            ServiceProvider.Get<IEnumerablePersister<LanguageInfo>>();
            ServiceProvider.Get<ILanguagesProvider>();
            ServiceProvider.Get<ISettingsProvider>();
            ServiceProvider.Get<IEntityCreator>();
            ServiceProvider.Get<IPersister<MainEntity>>();
            ServiceProvider.Get<ISpeechEngine>();
            ServiceProvider.Get<ITranslator>();
            ServiceProvider.Get<ISpecialSymbolsProvider>();
            ServiceProvider.Get<IDataAccess>();
            ServiceProvider.Get<IMerger>();
            ServiceProvider.Get<IPersister<DropboxToken>>();
            ServiceProvider.Get<IDropboxProvider>();
            ServiceProvider.Get<IPersister<LiveToken>>();
            ServiceProvider.Get<ILiveProvider>();
            ServiceProvider.Get<ISettingsProvider>();
        }

        private void Init()
        {
            Log.EnableTimning = Environment.CommandLine.Contains(Constans.EnableTiming);
            Log.StartTiming("Initialization");
            Log.LogMessage(Environment.CommandLine);

            Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/DataTemplatesDictionary.xaml")
            });
            Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/Styles.xaml") });

            if (!Directory.Exists(ApplicationPaths.ApplicationFolder))
                Directory.CreateDirectory(ApplicationPaths.ApplicationFolder);

            var settings = ConfigurationManager.ConnectionStrings["ExpressionsDataContext"];
            var fi = typeof(ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            fi.SetValue(settings, false);
            settings.ConnectionString = "Data Source=" + ApplicationPaths.SqlCeData;

            InitServices();
            TestServices();

            Log.StopTiming("Initialization");
        }

        private void InitServices()
        {
            Log.StartTiming("Init Services");

            if (Environment.CommandLine.Contains(Constans.RavenMode))
                InitRavenDB();
            else
            {
                if (!Directory.Exists(ApplicationPaths.SqlCeDataFolder))
                    Directory.CreateDirectory(ApplicationPaths.SqlCeDataFolder);

            }

            InitDropbox();
            InitLive();
            InitSettings();

            Log.StopTiming("Init Services");
        }
        private void InitRavenDB()
        {
            throw new Exception("RavenDB is not supported!");
            //Log.StartTiming("RavenDB Initialization");
            //if (!Directory.Exists(ApplicationPaths.RavenDataFolder))
            //    Directory.CreateDirectory(ApplicationPaths.RavenDataFolder);

            //var useEmbeddedHttpServer = ConfigurationManager.AppSettings[Constans.UseEmbeddedHttpServer];
            //var raven = new RavenDBDataAccess(ServiceProvider.GetService<IDateTimeProvider>());

            //try
            //{
            //    if (useEmbeddedHttpServer != null)
            //        raven.Init(ApplicationPaths.RavenDataFolder, Boolean.Parse(useEmbeddedHttpServer));
            //    else
            //        raven.Init(ApplicationPaths.RavenDataFolder);
            //}
            //catch (Exception ex)
            //{
            //    Log.LogException(ex);
            //    ServiceProvider.GetService<IWindowService>().ShowMessage(ex.ToString());
            //    Shutdown();
            //}

            //ServiceProvider.RegisterService(raven);
            //Log.StopTiming("RavenDB Initialization");
        }

        private async static void InitDropbox()
        {
            try
            {
                var prv = ServiceProvider.GetService<IDropboxProvider>();
                await prv.Init(ConfigurationManager.AppSettings[Constans.DropboxKey],
                             ConfigurationManager.AppSettings[Constans.DropboxSecret], true);
            }
            catch (Exception ex)
            {
                ServiceProvider.GetService<IWindowService>().ShowError(ex);
            }
        }
        private async static void InitLive()
        {
            try
            {
                var prv = ServiceProvider.GetService<ILiveProvider>();
                await prv.Init(ConfigurationManager.AppSettings[Constans.LiveClientId]);
            }
            catch (Exception ex)
            {
                ServiceProvider.GetService<IWindowService>().ShowError(ex);
            }
        }
        private static void InitSettings()
        {
            Log.StartTiming("Settings Initialization");

            var prv = ServiceProvider.GetService<ISettingsProvider>();

            if (!prv.Init(ApplicationPaths.ApplicationFolder))
                ServiceProvider.GetService<IWindowService>().ShowMessage(LanguageTrainer.Resources.Msg.CannotLoadSettings);

            prv.SetDefault(SettingsNames.WindowState, WindowState.Normal);
            prv.SetDefault(SettingsNames.Width, 800.0);
            prv.SetDefault(SettingsNames.Height, 600.0);
            prv.SetDefault(SettingsNames.Top, 0.0);
            prv.SetDefault(SettingsNames.Left, 0.0);

            Log.StopTiming("Settings Initialization");
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ServiceProvider.GetService<IWindowService>().ShowError(e.ExceptionObject);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (!e.Handled)
            {
                ServiceProvider.GetService<IWindowService>().ShowError(e.Exception);
            }
        }

        [STAThread]
        public static void Main()
        {
            new App().Run();
        }
    }
}
