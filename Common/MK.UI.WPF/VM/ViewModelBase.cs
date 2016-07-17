using System;
using System.Linq;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Windows.Input;
using MK.Utilities;

namespace MK.UI.WPF
{
    public class ViewModelBase : INotifyPropertyChanged, IDataErrorInfo
    {
        #region Properties

        public IWindowService WindowService { get; set; }

        public ViewModelBase Parent { get; set; }

        private bool SupressNotifyPropagation { get; set; }

        private string _Name = null;
        public virtual string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    Notify("Name");
                }
            }
        }

        private bool _isSelected;
        public virtual bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                Notify("IsSelected");
            }
        }

        #endregion

        #region Constructors

        public ViewModelBase()
        {
        }

        public ViewModelBase(ViewModelBase parent, bool supressNotifyPropagation = false)
        {
            this.Parent = parent;
            SupressNotifyPropagation = supressNotifyPropagation;
        }

        #endregion

        #region Methods

        public virtual bool BeforeShow()
        {
            return true;
        }

        public virtual void BeforeClose()
        {}

        public void Close(ViewModelBase vm)
        {
            WindowService.CloseWindow(vm);
        }

        public override string ToString()
        {
            return Name;
        }

        public void RefreshCommands(bool refreshChildren = false, bool refreshParent = false, string groupName = null)
        {
            var properties = GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var p in properties)
            {
                if (p.PropertyType == typeof (ICommand)
                    ||
                    p.PropertyType.IsSubclassOf(typeof(BaseCustomCommand)))
                {
                    var bcc = p.GetValue(this) as BaseCustomCommand;
                    if (bcc != null && bcc.GroupName == groupName)
                        bcc.RefreshCanExecute();
                }
            }

            if (refreshChildren)
            {
                foreach (var p in properties)
                {
                    if (p.PropertyType.IsSubclassOf(typeof (ViewModelBase)))
                    {
                        var bcc = p.GetValue(this) as ViewModelBase;
                        if (bcc != null)
                            bcc.RefreshCommands(true, false, groupName);
                    }
                }
            }

            if (refreshParent)
            {
                if(Parent != null)
                    Parent.RefreshCommands(false, true, groupName);
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(ViewModelBase vm, string property)
        {
            InternalOnPropertyChanged(vm, property);

            if (Parent != null && !SupressNotifyPropagation)
                Parent.OnPropertyChanged(vm, property);
        }

        protected virtual void InternalOnPropertyChanged(ViewModelBase vm, string property)
        {
        }

        public void Notify(string property)
        {
            property.NotNull("property");
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            }

            OnPropertyChanged(this, property);
        }

        public void Notify<T>(Expression<Func<T>> expression)
        {
            var name = expression.GetMemberName();
            Notify(name);
        }

        public void NotifyForCaller([CallerMemberName]string caller = null)
        {
            Notify(caller);
        }

        #endregion
        #region IDataErrorInfo Members

        public enum StandardErrors
        {
            WrongPath,
            CannotBeEmpty,
            MustBeGreaterThanZero,
            MustBeGreaterOrEqualZero,
            MaxCannotBeSmallerThanMin
        }

        private Dictionary<string, string> _errors = new Dictionary<string, string>();

        public bool IsValid { get { return _errors.Count == 0; } }

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                string msg;
                if (_errors.TryGetValue(columnName, out msg))
                    return msg;

                return null;
            }
        }

        public virtual void Validate()
        { }

        public void AddError(string columnName, string msg)
        {
            columnName.NotNull("columnName");
            msg.NotNull("error");

            _errors[columnName] = msg;
            Notify(columnName);
        }
        public void AddError(string columnName, StandardErrors error)
        {
            columnName.NotNull("columnName");
            error.NotNull("error");

            var type = typeof (Resources.Res);
            _errors[columnName] =
                (string)type.GetProperty(error.ToString(), BindingFlags.Static | BindingFlags.Public).GetValue(null, null);

            Notify(columnName);
        }
        public void AddError<T>(Expression<Func<T>> expression, string msg)
        {
            var body = expression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("Lambda expression should be a member expression");

            var constant = body.Expression as ConstantExpression;
            if (constant == null)
                throw new ArgumentException("Lambda expression body should be a constant expression");

            AddError(body.Member.Name, msg);
        }
        public void AddError<T>(Expression<Func<T>> expression, StandardErrors error)
        {
            var body = expression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("Lambda expression should be a member expression");

            var constant = body.Expression as ConstantExpression;
            if (constant == null)
                throw new ArgumentException("Lambda expression body should be a constant expression");

            AddError(body.Member.Name, error);
        }

        public void ClearError(string columnName)
        {
            columnName.NotNull("columnName");

            _errors.Remove(columnName);
            Notify(columnName);
        }
        public void ClearError<T>(Expression<Func<T>> expression)
        {
            var body = expression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("Lambda expression should be a member expression");

            var constant = body.Expression as ConstantExpression;
            if (constant == null)
                throw new ArgumentException("Lambda expression body should be a constant expression");

            ClearError(body.Member.Name);
        }
        public void ClearErrors()
        {
            var columns = _errors.Select(error => error.Key).ToList();
            _errors.Clear();

            foreach (var column in columns)
                Notify(column);
        }

        #endregion

        #region Mouse

        private CustomCommand _mouseDownCommand = null;
        public CustomCommand MouseDownCommand
        {
            get
            {
                if (_mouseDownCommand == null)
                    _mouseDownCommand = new CustomCommand(InnerMouseDown, () => true);

                return _mouseDownCommand;
            }
        }

        private CustomCommand _mouseUpCommand = null;
        public CustomCommand MouseUpCommand
        {
            get
            {
                if (_mouseUpCommand == null)
                    _mouseUpCommand = new CustomCommand(InnerMouseUp, () => true);

                return _mouseUpCommand;
            }
        }

        private CustomCommand _mouseMoveCommand = null;
        public CustomCommand MouseMoveCommand
        {
            get
            {
                if (_mouseMoveCommand == null)
                    _mouseMoveCommand = new CustomCommand(InnerMouseMove, () => true);

                return _mouseMoveCommand;
            }
        }

        private CustomCommand _mouseWheelCommand = null;
        public CustomCommand MouseWheelCommand
        {
            get
            {
                if (_mouseWheelCommand == null)
                    _mouseWheelCommand = new CustomCommand(InnerMouseWheel, () => true);

                return _mouseWheelCommand;
            }
        }



        protected virtual void MouseDown(MouseButtonEventArgs args)
        {
        }
        private void InnerMouseDown(object param)
        {
            MouseDown((MouseButtonEventArgs)param);
        }

        protected virtual void MouseUp(MouseButtonEventArgs args)
        {
        }
        private void InnerMouseUp(object param)
        {
            MouseUp((MouseButtonEventArgs)param);
        }

        protected virtual void MouseMove(MouseEventArgs args)
        {
        }
        private void InnerMouseMove(object param)
        {
            MouseMove((MouseEventArgs)param);
        }

        protected virtual void MouseWheel(MouseWheelEventArgs args)
        {
        }
        private void InnerMouseWheel(object param)
        {
            MouseWheel((MouseWheelEventArgs)param);
        }

        #endregion

        #region Cancel

        public virtual string CancelCommandGroupName
        {
            get { return null; }
        }

        private CustomCommand _cancelCommand = null;
        public CustomCommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                    _cancelCommand = new CustomCommand(InnerCancel, InnerCanCancel, groupName: CancelCommandGroupName);

                return _cancelCommand;
            }
        }

        protected virtual void Cancel()
        {
            StopOperation(groupName: CancelCommandGroupName);
        }
        protected virtual bool CanCancel()
        {
            return CheckIfOperationInProgress(CancelCommandGroupName);
        }

        private void InnerCancel()
        {
            Cancel();
        }
        private bool InnerCanCancel()
        {
            return CanCancel();
        }

        #endregion

        #region Operations

        private readonly object _lock = new object();

        private readonly Dictionary<string, CancellationTokenSource> _tokenSources = new Dictionary<string, CancellationTokenSource>();
        private readonly HashSet<string> _operations = new HashSet<string>(); 



        protected CancellationTokenSource TokenSource
        {
            get { return GetTokenSource(null); }
        }

        protected CancellationTokenSource GetTokenSource(string groupName)
        {
            lock (_lock)
            {
                if (groupName == null)
                    groupName = String.Empty;

                CancellationTokenSource cts;
                _tokenSources.TryGetValue(groupName, out cts);
                return cts;
            }
        }

 

        protected bool IsOperationInProgress
        {
            get { return CheckIfOperationInProgress(null); }
        }

        protected bool CheckIfOperationInProgress(string groupName)
        {
            lock (_lock)
            {
                if (groupName == null)
                    groupName = String.Empty;

                return _operations.Contains(groupName);
            }
        }



        protected internal void StartOperation(bool async = false, bool refreshCommands = true, bool refreshChildren = false, bool refreshParent = false, string groupName = null)
        {
            lock (_lock)
            {
                if (groupName == null)
                    groupName = String.Empty;

                if (CheckIfOperationInProgress(groupName))
                    throw new InvalidOperationException("Async operation already in progress!");

                if(async)
                    _tokenSources.Add(groupName, new CancellationTokenSource());

                _operations.Add(groupName);
            }

            if (refreshCommands)
                RefreshCommands(refreshChildren, refreshParent, groupName);
        }

        protected internal void StopOperation(bool refreshCommands = true, bool refreshChildren = false, bool refreshParent = false, string groupName = null)
        {
            lock (_lock)
            {
                if (groupName == null)
                    groupName = String.Empty;

                if (!CheckIfOperationInProgress(groupName))
                    return;
                
                var cts = GetTokenSource(groupName);
                if (cts != null)
                {
                    cts.Cancel();
                    cts.Dispose();

                    _tokenSources.Remove(groupName);
                }

                _operations.Remove(groupName);
            }

            if (refreshCommands)
                RefreshCommands(refreshChildren, refreshParent, groupName);
        }

        #endregion
    }
}

