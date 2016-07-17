using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using MK.UI.WPF.VM;
using MK.Utilities;
using MK.Settings;
using MK.UI.WPF.Controls;
using MK.Logging;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using Orientation = System.Windows.Controls.Orientation;

namespace MK.UI.WPF
{
    public class WindowService : IWindowService
    {
        #region Internal Definitions

        private class Data
        {
            public Window Window { get; set; }
            public ViewModelBase VM { get; set; }
            public bool AllowSettings { get; set; }
            public bool SaveSizeAndPosition { get; set; }
            public string ExtraSettingsKey { get; set; }
        }

        #endregion

        #region Fields & Properties

        private readonly Stack<Data> _windows = new Stack<Data>();

        private readonly InputBindingCollection _globalBindings = new InputBindingCollection();

        public Window MainWindow { get; private set; }

        public bool MainWindowInitialized
        {
            get { return _windows.Any(); }
        }

        #endregion

        #region Constructor

        public WindowService()
        {
            MainWindow = new Window();

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/MK.UI.WPF;component/StandardDataTemplatesDictionary.xaml")
            });
        }

        #endregion

        #region Methods

        public void MakeWindowVisible(ViewModelBase vm)
        {
            vm.NotNull("vm");

            Trace.Assert(Object.ReferenceEquals(_windows.Peek().VM, vm));
            _windows.Peek().Window.Visibility = Visibility.Visible;
        }
        public void CloseWindow(ViewModelBase vm)
        {
            vm.NotNull("vm");
            var peek = _windows.Peek();
            Trace.Assert(Object.ReferenceEquals(peek.VM, vm));
            peek.Window.Close();
        }

        public void AddGlobalCommand(Action action, InputGesture gesture)
        {
            AddGlobalCommand(new CustomCommand(() => action(), () => true), gesture);
        }
        public void AddGlobalCommand(ICommand command, InputGesture gesture)
        {
            var binding = new InputBinding(command, gesture);

            var newGesture = gesture as KeyGesture;
            if (newGesture != null)
            {
                foreach (InputBinding b in _globalBindings)
                {
                    var g = b.Gesture as KeyGesture;
                    if (g != null && g.Key == newGesture.Key && g.Modifiers == newGesture.Modifiers)
                        throw new Exception("Duplicated gesture!");
                }
            }

            _globalBindings.Add(binding);

            if (MainWindow != null)
                MainWindow.InputBindings.Add(binding);
        }

        #endregion

        #region Show Dialog Methods

        public bool? ShowDialog(ViewModelBase vm, 
            bool canResize = false, 
            bool initialyVisible = true, 
            bool allowSettings = true,
            bool saveSizeAndPosition = false,
            bool showOkCancelButtons = false,
            string extraSettingsKey = null)
        {
            vm.NotNull("vm");

            vm = ServiceProvider.Inject(vm);

            bool? res = null;
            var w = MainWindowInitialized ? new Window() : MainWindow;
            
            var mainDockPanel = new DockPanel { LastChildFill = true};

            if(vm.Name != null)
                w.Title = vm.Name;


            w.Closed += w_Closed;
            w.Content = mainDockPanel;

            if (showOkCancelButtons)
            {
                var ok = new Button {Content = Resources.Res.Ok, Margin = new Thickness(2)};
                ok.Click += (sender, args) =>
                    {
                        res = true;
                        w.Close();
                    };

                var cancel = new Button {Content = Resources.Res.Cancel, Margin = new Thickness(2)};
                cancel.Click += (sender, args) =>
                    {
                        res = false;
                        w.Close();
                    };

                var buttonsStackPanel = new StackPanel {Orientation = Orientation.Horizontal};
                buttonsStackPanel.Children.Add(ok);
                buttonsStackPanel.Children.Add(cancel);
                DockPanel.SetDock(buttonsStackPanel, Dock.Bottom);

                mainDockPanel.Children.Add(buttonsStackPanel);
            }

            var content = new ContentControl {Content = vm};
            mainDockPanel.Children.Add(content);
      
            if (_windows.Count > 0)
                w.Owner = _windows.Peek().Window;

            if (!canResize)
                w.ResizeMode = ResizeMode.NoResize;

            if (!initialyVisible)
                w.Visibility = Visibility.Hidden;

            if (!_windows.Any())
            {
                MainWindow.InputBindings.AddRange(_globalBindings);
                MainWindow.Title = String.Format("{0} {1}", vm.Name, System.Reflection.Assembly.GetEntryAssembly().GetName().Version);
            }

            try
            {
                var prv = ServiceProvider.GetService<ISettingsProvider>();
                if (prv != null)
                {

                    if (allowSettings)
                        prv.InjectSettings(vm, extraSettingsKey);

                    if ((saveSizeAndPosition || ReferenceEquals(w, MainWindow)) &&
                        prv.HasSetting(vm, extraSettingsKey, SettingsNames.WindowState))
                    {
                        w.WindowState = prv.GetValue<WindowState>(vm, extraSettingsKey, SettingsNames.WindowState);
                        w.Width = prv.GetValue<double>(vm, extraSettingsKey, SettingsNames.Width);
                        w.Height = prv.GetValue<double>(vm, extraSettingsKey, SettingsNames.Height);
                        w.Top = prv.GetValue<double>(vm, extraSettingsKey, SettingsNames.Top);
                        w.Left = prv.GetValue<double>(vm, extraSettingsKey, SettingsNames.Left);
                    }
                    else
                    {
                        w.SizeToContent = SizeToContent.WidthAndHeight;
                        w.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }

            _windows.Push(new Data
                {
                    Window = w,
                    VM = vm, 
                    AllowSettings =  allowSettings,
                    SaveSizeAndPosition = saveSizeAndPosition,
                    ExtraSettingsKey =  extraSettingsKey
                });

            if(vm.BeforeShow())
                w.ShowDialog();

            return res;
        }

        public bool ShowError(object obj, bool askForContinuation = false)
        {
            var buttons = askForContinuation ? MessageBoxButton.OKCancel : MessageBoxButton.OK;
            var result = true;

            if (obj is Exception)
            {
                var exception = (Exception)obj;
                var ex = exception.ToString();
                var res = ex.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                var sb = new StringBuilder();
                for (int i = 0; i < res.Length && i < 20; ++i)
                    sb.AppendLine(res[i]);

                result = MessageBox.Show(sb.ToString(), String.Empty, buttons, MessageBoxImage.Error) == MessageBoxResult.OK;
                Log.LogException(exception);
            }
            else
            {
                result = MessageBox.Show(obj.ToString(), String.Empty, buttons, MessageBoxImage.Error) == MessageBoxResult.OK;
                Log.LogError(obj.ToString());
            }

            return result;
        }

        public void ShowMessage(string msg)
        {
            MessageBox.Show(msg, String.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool ShowQuestion(string msg)
        {
            return MessageBox.Show(msg, String.Empty, MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK;
        }

        #endregion
        #region File & Directory Methods

        public void ShowDirectory(string dir)
        {
            if (Directory.Exists(dir))
            {
                var prc = new System.Diagnostics.Process {StartInfo = {FileName = dir}};
                prc.Start();
            }
        }

        public void ShowWebPage(Uri address, bool useBrowser = true)
        {
            if (useBrowser)
            {
                var prc = new Process { StartInfo = { FileName = address.AbsoluteUri } };
                prc.Start();
            }
            else
            {
                var content = new ContentControl { Content = new WebBrowserVM(null, address.ToString()) };

                var mainDockPanel = new DockPanel { LastChildFill = true };
                mainDockPanel.Children.Add(content);

                var w = new Window();
                w.Content = mainDockPanel;

                w.Show();
            }
        }

        public string ShowFolderBrowser(string initialDirectory, bool restoreDirectory = true)
        {
            string result = null;
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (Directory.Exists(initialDirectory))
                    dlg.SelectedPath = initialDirectory;

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    result = dlg.SelectedPath;
            }

            return result;
        }

        public string[] ShowOpenFileBrowser(string initialDirectory, bool multiselect = false, bool restoreDirectory = true)
        {
            string[] result = null;
            using (var dlg = new System.Windows.Forms.OpenFileDialog())
            {
                dlg.CheckFileExists = true;
                dlg.CheckPathExists = true;
                dlg.Multiselect = multiselect;
                dlg.RestoreDirectory = restoreDirectory;

                if (!String.IsNullOrEmpty(initialDirectory))
                    dlg.InitialDirectory = File.Exists(initialDirectory) ? Path.GetDirectoryName(initialDirectory) : initialDirectory;

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    result = dlg.FileNames;
            }

            return result;
        }

        public string ShowSaveFileBrowser(string initialDirectory, bool restoreDirectory = true)
        {
            string result = null;
            using (var dlg = new System.Windows.Forms.SaveFileDialog())
            {
                dlg.CheckPathExists = true;
                dlg.RestoreDirectory = restoreDirectory;

                if (!String.IsNullOrEmpty(initialDirectory))
                    dlg.InitialDirectory = File.Exists(initialDirectory) ? Path.GetDirectoryName(initialDirectory) : initialDirectory;

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    result = dlg.FileName;
            }

            return result;
        }

        #endregion
        #region Background Task Methods

        private bool _isBackgroundTaskActive;

        private WaitWindow _currentBackgoundTaskWindow;

        public TOutput DoBackgroundTask<TInput, TOutput>(Func<TInput, TOutput> task, Action cancelAction = null,
                                                         TInput parameter = default(TInput), bool centerOwner = true,
                                                         bool returnIfBackgroundTaskIsActive = false,
                                                         bool handleExceptions = true)
        {
            if (_isBackgroundTaskActive)
            {
                if (returnIfBackgroundTaskIsActive)
                    return default(TOutput);

                throw new Exception("Background task is active!");
            }

            
            _isBackgroundTaskActive = true;

            _currentBackgoundTaskWindow = new WaitWindow(p => task((TInput) p), cancelAction, parameter, handleExceptions);

            if (MainWindowInitialized)
                _currentBackgoundTaskWindow.Owner = MainWindow;

            _currentBackgoundTaskWindow.WindowStartupLocation = centerOwner
                                                                    ? WindowStartupLocation.CenterOwner
                                                                    : WindowStartupLocation.CenterScreen;

            _currentBackgoundTaskWindow.InputBindings.AddRange(_globalBindings);
            _currentBackgoundTaskWindow.ShowDialog();

            var res = default(TOutput);
            if (!(_currentBackgoundTaskWindow.Result is Exception))
                res = (TOutput)_currentBackgoundTaskWindow.Result;

            _currentBackgoundTaskWindow = null;
            _isBackgroundTaskActive = false;

            return res;
        }

        public TOutput DoBackgroundTask<TOutput>(Func<CancellationToken, TOutput> task, bool centerOwner = true, bool returnIfBackgroundTaskIsActive = false, bool handleExceptions = true)
        {
            var cts = new CancellationTokenSource();
            return DoBackgroundTask(task, cts.Cancel, cts.Token, centerOwner, returnIfBackgroundTaskIsActive, handleExceptions);
        }
        public TOutput DoBackgroundTask<TOutput>(Func<TOutput> task, Action cancelAction = null, bool centerOwner = true, bool returnIfBackgroundTaskIsActive = false, bool handleExceptions = true)
        {
            return DoBackgroundTask<object, TOutput>(p => task(), cancelAction, null, centerOwner, returnIfBackgroundTaskIsActive, handleExceptions);
        }

        public void DoBackgroundTask(Action task, Action cancelAction = null, bool centerOwner = true, bool returnIfBackgroundTaskIsActive = false, bool handleExceptions = true)
        {
            DoBackgroundTask<object, object>(
                p =>
                {
                    task();
                    return null;
                }, cancelAction, null, centerOwner, returnIfBackgroundTaskIsActive, handleExceptions);
        }
        public void DoBackgroundTask(Action<CancellationToken> task, bool centerOwner = true, bool returnIfBackgroundTaskIsActive = false, bool handleExceptions = true)
        {
            var cts = new CancellationTokenSource();
            DoBackgroundTask<CancellationToken, object>(p =>
                {
                    task(p);
                    return null;
                }, cts.Cancel, cts.Token, centerOwner, returnIfBackgroundTaskIsActive, handleExceptions);
        }

        public void ReportProgress(string msg)
        {
            if (!_isBackgroundTaskActive)
                throw new Exception("Background task is not active!");

            _currentBackgoundTaskWindow.Message = msg;
        }

        #endregion

        #region Progress Bar

        private bool _isProgressBarActive = false;

        private WaitWindow _currentProgressBarWindow;

        private readonly ManualResetEvent _event = new ManualResetEvent(false);

        public void ShowProgress(bool centerOwner = true)
        {
            if (_isProgressBarActive)
                throw new Exception("Progress Bar is active!");

            _isProgressBarActive = true;

            var thread = new Thread(() =>
            {
                _currentProgressBarWindow = new WaitWindow((param) => _event.WaitOne(), null, null);
                _currentProgressBarWindow.Closed += (sender1, e1) => _currentProgressBarWindow.Dispatcher.InvokeShutdown();

                if (MainWindowInitialized)
                    _currentBackgoundTaskWindow.Owner = MainWindow;

                _currentProgressBarWindow.WindowStartupLocation = centerOwner
                                                                        ? WindowStartupLocation.CenterOwner
                                                                        : WindowStartupLocation.CenterScreen;

                _currentProgressBarWindow.Show();
                Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            _isProgressBarActive = true;
        }

        public void HideProgress()
        {
            _event.Set();

            _currentBackgoundTaskWindow = null;
            _isProgressBarActive = false;
        }

        #endregion

        #region Events Handlers

        private void w_Closed(object sender, EventArgs e)
        {
            var peek = _windows.Peek();

            if (ReferenceEquals(peek.Window, sender))
            {
                peek.VM.BeforeClose();
                if (peek.AllowSettings)
                {
                    var prv = ServiceProvider.GetService<ISettingsProvider>();
                    if(prv != null)
                        prv.ExtractSettings(peek.VM, peek.ExtraSettingsKey);
                }

                _windows.Pop();
            }

            if (peek.SaveSizeAndPosition || ReferenceEquals(peek.Window, MainWindow))
            {
                var prv = ServiceProvider.GetService<ISettingsProvider>();
                if (prv != null)
                {
                    prv.SetValue(peek.VM, peek.ExtraSettingsKey, SettingsNames.WindowState, peek.Window.WindowState);
                    prv.SetValue(peek.VM, peek.ExtraSettingsKey, SettingsNames.Width, peek.Window.Width);
                    prv.SetValue(peek.VM, peek.ExtraSettingsKey, SettingsNames.Height, peek.Window.Height);
                    prv.SetValue(peek.VM, peek.ExtraSettingsKey, SettingsNames.Top, peek.Window.Top);
                    prv.SetValue(peek.VM, peek.ExtraSettingsKey, SettingsNames.Left, peek.Window.Left);
                    prv.Save();
                }
            }
        }

        #endregion
    }
}
