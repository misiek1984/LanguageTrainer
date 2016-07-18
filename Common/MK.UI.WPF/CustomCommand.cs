using System;
using System.Threading.Tasks;
using System.Windows.Input;

using MK.Utilities;

namespace MK.UI.WPF
{
    public abstract class BaseCustomCommand : ICommand
    {
        #region Events

        public delegate bool CanExecuteHandler();

        public event EventHandler CanExecuteChanged;

        #endregion

        #region Properties

        protected CanExecuteHandler CanExecuteAction { get; set; }

        protected bool HandleExceptions { get; set; }

        protected ViewModelBase Parent { get; set; }

        protected bool AutoCanExecute { get; set; }

        protected bool InProgress { get; set; }

        public string GroupName { get; protected  set; }

        #endregion

        #region Constructor

        public BaseCustomCommand(
            CanExecuteHandler canExecuteAction = null, 
            bool handleExceptions = false,
            ViewModelBase parent = null, 
            bool autoCanExecute = false,
            string groupName = null)
        {
            CanExecuteAction = canExecuteAction;
            HandleExceptions = handleExceptions;
            Parent = parent;
            AutoCanExecute = autoCanExecute;
            GroupName = groupName;

            if (AutoCanExecute)
            {
                if (canExecuteAction != null)
                    CanExecuteAction = () => !InProgress && canExecuteAction();
                else
                    CanExecuteAction = () => !InProgress;
            }
            else
                CanExecuteAction = canExecuteAction;
        }

        #endregion

        #region Methods

        public void RefreshCanExecute()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, new EventArgs());
            }
        }

        protected string GetOperationName(Delegate action)
        {
            return action.Method.Name;
        }

        #endregion

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            if(CanExecuteAction != null)
                return CanExecuteAction();

            return true;
        }

        public abstract void Execute(object parameter);

        #endregion
    }
    public class CustomCommand : BaseCustomCommand
    {
        #region Events

        public delegate void ExecuteHandler();
        public delegate void ExecuteHandlerWithParam(object param);

        #endregion

        #region Properties

        private ExecuteHandler ExecuteAction { get; set; }
        private ExecuteHandlerWithParam ExecuteActionWithParam { get; set; }

        #endregion

        #region Constructor

        public CustomCommand(
            ExecuteHandler executeAction, 
            CanExecuteHandler canExecuteAction = null,
            bool handleExceptions = false, 
            ViewModelBase parent = null,
            bool autoCanExecute = false,
            string groupName = null)
            : base(canExecuteAction, handleExceptions, parent, autoCanExecute, groupName)
        {
            executeAction.NotNull("execute");
            ExecuteAction = executeAction;
        }

        public CustomCommand(
            ExecuteHandlerWithParam executeAction, 
            CanExecuteHandler canExecuteAction = null,
            bool handleExceptions = false, 
            ViewModelBase parent = null,
            bool autoCanExecute = false,
            string groupName = null)
            : base(canExecuteAction, handleExceptions, parent, autoCanExecute, groupName)
        {
            executeAction.NotNull("execute");
            ExecuteActionWithParam = executeAction;
        }

        #endregion

        #region Methods

        public override void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            if (Parent != null)
                Parent.StartOperation(false, groupName: GroupName);

            var subOperation = GetOperationName(ExecuteAction != null ? (Delegate) ExecuteAction : (Delegate) ExecuteActionWithParam);

            InProgress = true;
            if (AutoCanExecute)
                RefreshCanExecute();

            Logging.Log.StartTiming(subOperation);
            try
            {
                if (ExecuteAction != null)
                    ExecuteAction();
                else if (ExecuteActionWithParam != null)
                    ExecuteActionWithParam(parameter);
            }
            catch (Exception ex)
            {
                if (HandleExceptions)
                    ServiceProvider.GetService<IWindowService>().ShowError(ex);
                else
                    throw;
            }
            finally
            {
                Logging.Log.StopTiming(subOperation);
                InProgress = false;
            }

            if (Parent != null)
                Parent.StopOperation(false, groupName: GroupName);

            RefreshCanExecute();
        }

        #endregion
    }

    public class AsyncCustomCommand : BaseCustomCommand
    {
        #region Events

        public delegate Task ExecuteAsyncHandler();
        public delegate Task ExecuteAsyncHandlerWithParam(object param);
        
        #endregion

        #region Properties

        private ExecuteAsyncHandler ExecuteAsyncAction { get; set; }
        private ExecuteAsyncHandlerWithParam ExecuteAsyncActionWithParam { get; set; }

        #endregion

        #region Constructor

        public AsyncCustomCommand(
            ExecuteAsyncHandler executeAction, 
            CanExecuteHandler canExecuteAction = null,
            bool handleExceptions = false, 
            ViewModelBase parent = null,
            bool autoCanExecute = false,
            string groupName = null)
            : base(canExecuteAction, handleExceptions, parent, autoCanExecute, groupName)
        {
            executeAction.NotNull("execute");
            ExecuteAsyncAction = executeAction;
        }

        public AsyncCustomCommand(
            ExecuteAsyncHandlerWithParam executeAction, 
            CanExecuteHandler canExecuteAction = null, 
            bool handleExceptions = false, 
            ViewModelBase parent = null,
            bool autoCanExecute = false,
            string groupName = null)
            : base(canExecuteAction, handleExceptions, parent, autoCanExecute, groupName)
        {
            executeAction.NotNull("execute");
            ExecuteAsyncActionWithParam = executeAction;
        }

        #endregion

        #region Methods

        public async override void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            if (Parent != null)
                Parent.StartOperation(true, groupName: GroupName);

            var subOperation = GetOperationName(ExecuteAsyncAction != null ? (Delegate)ExecuteAsyncAction : (Delegate)ExecuteAsyncActionWithParam);

            InProgress = true;
            if (AutoCanExecute)
                RefreshCanExecute();

            Logging.Log.StartTiming(subOperation);
            try
            {
                if (ExecuteAsyncAction != null)
                    await ExecuteAsyncAction();
                else if (ExecuteAsyncActionWithParam != null)
                    await ExecuteAsyncActionWithParam(parameter);
            }
            catch (Exception ex)
            {
                if (HandleExceptions)
                    ServiceProvider.GetService<IWindowService>().ShowError(ex);
                else
                    throw;
            }
            finally
            {
                Logging.Log.StopTiming(subOperation);
                InProgress = false;
            }

            if (Parent != null)
                Parent.StopOperation(true, groupName: GroupName);

            RefreshCanExecute();
        }


        #endregion
    }
}

