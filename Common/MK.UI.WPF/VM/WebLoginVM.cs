using System;
using System.Threading.Tasks;

using MK.Utilities;

namespace MK.UI.WPF.VM
{
    public class WebLoginVM : ViewModelBase
    {
        #region Fields & Properties

        private Func<Uri, Task<bool>> _callbackLogin;

        private Func<Uri, bool> _isFinalUrl;

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

        private AsyncCustomCommand _NavigatedCommand;
        public AsyncCustomCommand NavigatedCommand
        {
            get
            {
                if (_NavigatedCommand == null)
                    _NavigatedCommand = new AsyncCustomCommand(async p =>
                        {
                            var uri = p as Uri;

                            if (uri == null)
                                return;

                            if (_isFinalUrl != null && !_isFinalUrl(uri))
                                return;

                            if (uri.AbsoluteUri == NavigateTo)
                                return;

                            if (await _callbackLogin(p as Uri))
                                WindowService.CloseWindow(this);
                        }, 
                        () => true);

                return _NavigatedCommand;
            }
        }

        #endregion

        #region Constructor

        public WebLoginVM(ViewModelBase parent, string url, Func<Uri, Task<bool>> callbackLogin, Func<Uri, bool> isFinalUrl = null)
            : base(parent)
        {
            NavigateTo = url;
            _callbackLogin = callbackLogin;
            _isFinalUrl = isFinalUrl;
        }

        #endregion
    }
}

