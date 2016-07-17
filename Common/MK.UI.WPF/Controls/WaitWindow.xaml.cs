using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;

using MK.Utilities;

namespace MK.UI.WPF.Controls
{
	public partial class WaitWindow : Window, INotifyPropertyChanged
    {
        #region Properties

        private BackgroundWorker Worker { get; set; }
        private Func<object, object> Task { get; set; }
        private Action CancelAction { get; set; }
        private object Parameter { get; set; }
        private bool HandleExceptions { get; set; }
        

        public object Result { get; private set; }

        private string _Message;
        public string Message
        {
            get { return _Message; }
            set
            {
                if (value != _Message)
                {
                    _Message = value;
                    Notify("Message");
                }
            }
        }
                
        #endregion

        #region Constructor

        public WaitWindow(Func<object, object> task, Action cancelAction, object parameter, bool handleExceptions = true)
		{
            task.NotNull("task");

			InitializeComponent();

            Parameter = parameter;
            Task = task;
            CancelAction = cancelAction;
            HandleExceptions = handleExceptions;

            Worker = new BackgroundWorker();
            Worker.DoWork += DoWork;
            Worker.RunWorkerCompleted += RunWorkerCompleted;

            Loaded += Window_Loaded;
		}

        #endregion

        #region Event Handlers

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (HandleExceptions && e.Result is Exception)
                ServiceProvider.GetService<IWindowService>().ShowError(e.Result as Exception);

            Result = e.Result;
            Close();
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                e.Result = Task(e.Argument);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Worker.RunWorkerAsync(Parameter);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && CancelAction != null)
                CancelAction();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void Notify(string property)
        {
            property.NotNull("property");
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion
    }
}