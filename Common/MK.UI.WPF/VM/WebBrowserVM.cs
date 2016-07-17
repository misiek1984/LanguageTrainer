namespace MK.UI.WPF.VM
{
    public class WebBrowserVM : ViewModelBase
    {
        #region Fields & Properties

        public override string Name { get; set; }

        private string _NavigateTo;
        public string NavigateTo
        {
            get { return _NavigateTo; }
            set
            {
                if (_NavigateTo != value)
                {
                    _NavigateTo = value;
                    Notify(() => NavigateTo);
                }
            }
        }
        #endregion

        #region Constructor

        public WebBrowserVM(ViewModelBase parent, string url)
            : base(parent)
        {
            NavigateTo = url;
        }

        #endregion
    }
}

