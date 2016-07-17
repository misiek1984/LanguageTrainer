using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

using MK.Utilities;

namespace MK.Data
{
    public abstract partial class DataEntity : INotifyPropertyChanged, IDataErrorInfo
    {
        private string _name;
        public virtual string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    Notify("Name");
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        #region Data Management

        private List<IEnumerable<DataEntity>> _collections = new List<IEnumerable<DataEntity>>();
        private Dictionary<string, object> _data = new Dictionary<string, object>();

        private bool _isDirty;

        public T GetData<T>(string key)
        {
            object data;
            if (_data.TryGetValue(key, out data))
                return (T)data;
            
            return default(T);
        }
        public void SetData(string key, object val)
        {
            object oldValue;
            if (_data.TryGetValue(key, out oldValue))
            {
                if (val != null && val.GetType().IsValueType)
                {
                    if (!val.Equals(oldValue))
                        _isDirty = true;
                }
                else if (val != oldValue)
                    _isDirty = true;
            }
            else
                MakeDirty();

            _data[key] = val;
        }

        public void SetClean()
        {
            _isDirty = false;

            foreach (var c in _collections)
                foreach (var e in c)
                    e.SetClean();
        }
        public void MakeDirty()
        {
            _isDirty = true;
        }

        public void RegisterCollection(IEnumerable<DataEntity> collection)
        {
            _collections.Add(collection);
        }
        public void UnregisterCollection(IEnumerable<DataEntity> collection)
        {
            _collections.Remove(collection);
        }

        public bool CheckIfIsDirty()
        {
            if (_isDirty)
                return true;

            foreach (var c in _collections)
                foreach (var e in c)
                    if (e.CheckIfIsDirty())
                        return true;

            return _isDirty;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string property)
        {
            property.NotNull("property");
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
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

        private Dictionary<string, string> _errors = new Dictionary<string, string>();

        [XmlIgnore]
        public virtual bool IsValid { get { return _errors.Count == 0; } }

        [XmlIgnore]
        public virtual string Error
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
            msg.NotNull("msg");

            _errors[columnName] = msg;
            Notify(columnName);
        }

        public void ClearError(string columnName)
        {
            columnName.NotNull("columnName");

            if (_errors.Count > 0)
            {
                _errors.Remove(columnName);
                Notify(columnName);
            }
        }


        public void AddError<T>(Expression<Func<T>> expression, string msg)
        {
            var name = expression.GetMemberName();
            AddError(name, msg);
        }

        public void ClearError<T>(Expression<Func<T>> expression)
        {
            var name = expression.GetMemberName();
            ClearError(name);
        }

        public void AddErrorForCaller([CallerMemberName]string caller = null, string msg = null)
        {
            AddError(caller, msg);
        }

        public void ClearErrorForCCaller([CallerMemberName]string caller = null)
        {
            ClearError(caller);
        }

        #endregion
    }
}
