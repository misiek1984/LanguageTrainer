using System;
using System.Threading;
using System.Windows.Input;

namespace MK.UI.WPF
{
    public interface IWindowService
    {
        System.Windows.Window MainWindow { get; }

        void AddGlobalCommand(Action action, InputGesture gesture);
        void AddGlobalCommand(ICommand command, InputGesture gesture);

        bool? ShowDialog(ViewModelBase vm, 
            bool canResize = false, 
            bool initialyVisible = true, 
            bool allowSettings = true, 
            bool saveSizeAndPosition = false,
            bool showOkCancelButtons = false,
            string extraSettingsKey = null);

        bool ShowError(object obj, bool askForContinuation = false);
        void ShowMessage(string msg);
        bool ShowQuestion(string msg);
        void ShowDirectory(string direcotry);
        void ShowWebPage(Uri address, bool useBrowser = true);

        string ShowFolderBrowser(string initialDirectory, bool restoreDirectory = true);
        string[] ShowOpenFileBrowser(string initialDirectory, bool multiselect = false, bool restoreDirectory = true);
        string ShowSaveFileBrowser(string initialDirectory, bool restoreDirectory = true);

        TOutput DoBackgroundTask<TInput, TOutput>(Func<TInput, TOutput> task, Action cancelAction = null, TInput parameter = default(TInput), bool centerOwner = true, bool returnIfBackgroundTaskIsActive = false, bool handleExceptions = true);
        TOutput DoBackgroundTask<TOutput>(Func<CancellationToken, TOutput> task, bool centerOwner = true, bool returnIfBackgroundTaskIsActive = false, bool handleExceptions = true);
        TOutput DoBackgroundTask<TOutput>(Func<TOutput> task, Action cancelAction = null, bool centerOwner = true, bool returnIfBackgroundTaskIsActive = false, bool handleExceptions = true);

        void DoBackgroundTask(Action task, Action cancelAction = null, bool centerOwner = true, bool returnIfBackgroundTaskIsActive = false, bool handleExceptions = true);
        void DoBackgroundTask(Action<CancellationToken> task, bool centerOwner = true, bool returnIfBackgroundTaskIsActive = false, bool handleExceptions = true);
        
        void ReportProgress(string msg);

        void ShowProgress(bool centerOwner = true);
        void HideProgress();

        void MakeWindowVisible(ViewModelBase vm);
        void CloseWindow(ViewModelBase vm);
    }
}
