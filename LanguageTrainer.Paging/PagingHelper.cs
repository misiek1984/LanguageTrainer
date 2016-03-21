using System;
using System.ComponentModel;

using MK.Utilities;
using MK.UI.WPF;

namespace LanguageTrainer.Paging
{
    public class PagingHelper: INotifyPropertyChanged
    {
        #region Events

        public event EventHandler PageChanged;

        #endregion

        #region Properties

        private IPagingClient Client { get; set; }

        private int _currentPageIndex = PagingConstans.FirstPageIndex;
        public int CurrentPageIndex
        {
            get { return _currentPageIndex; }
            private set
            {
                _currentPageIndex = value;
                if (_currentPageIndex < PagingConstans.FirstPageIndex)
                    _currentPageIndex = PagingConstans.FirstPageIndex;

                if (_currentPageIndex > NumberOfPages)
                    _currentPageIndex = NumberOfPages;

                OnPageChanged();
                RefreshPagingProperties();
            }
        }

        public int NumberOfPages
        {
            get
            {
                var res = Client.NumberOfItems / PagingConstans.MaxPageSize + (Client.NumberOfItems % PagingConstans.MaxPageSize == 0 ? 0 : 1);

                return res == 0 ? 1 : res;
            }
        }

        #endregion

        #region Constructor

        public PagingHelper(IPagingClient client)
        {
            client.NotNull("client");
            Client = client;
        }

        #endregion

        #region Commands

        private CustomCommand _nextPageCommand;
        public CustomCommand NextPageCommand
        {
            get
            {
                if (_nextPageCommand == null)
                    _nextPageCommand = new CustomCommand(p => NextPage(p), () => true);

                return _nextPageCommand;
            }
        }
        private void NextPage(object parameter)
        {
            int i;
            if (Int32.TryParse((string)parameter, out i))
                MovePage(i);
            else
                NextPage();
        }

        private CustomCommand _prevPageCommand;
        public CustomCommand PrevPageCommand
        {
            get
            {
                if (_prevPageCommand == null)
                    _prevPageCommand = new CustomCommand(p => PrevPage(p), () => true);

                return _prevPageCommand;
            }
        }
        private void PrevPage(object parameter)
        {
            int i;
            if (Int32.TryParse((string)parameter, out i))
                MovePage(i);
            else
                PrevPage();
        }

        private CustomCommand _firstPageCommand;
        public CustomCommand FirstPageCommand
        {
            get
            {
                if (_firstPageCommand == null)
                    _firstPageCommand = new CustomCommand(() => FirstPage(), () => true);

                return _firstPageCommand;
            }
        }

        private CustomCommand _lastPageCommand;
        public CustomCommand LastPageCommand
        {
            get
            {
                if (_lastPageCommand == null)
                    _lastPageCommand = new CustomCommand(() => LastPage(), () => true);

                return _lastPageCommand;
            }
        }

        #endregion

        #region Methods

        public bool NextPage()
        {
            return MovePage(1);
        }
        public bool PrevPage()
        {
            return MovePage(-1);
        }

        public bool FirstPage()
        {
            if (CurrentPageIndex == PagingConstans.FirstPageIndex)
                return false;

            CurrentPageIndex = PagingConstans.FirstPageIndex;
            return true;
        }
        public bool LastPage()
        {
            if (CurrentPageIndex == NumberOfPages)
                return false;

            CurrentPageIndex = NumberOfPages;
            return true;
        }

        public bool MovePage(int step)
        {
            if (CurrentPageIndex == PagingConstans.FirstPageIndex && step < 0)
                return false;

            if (CurrentPageIndex == NumberOfPages && step > 0)
                return false;

            CurrentPageIndex += step;
            return true;
        }

        public void MoveToItem(int index)
        {
            CurrentPageIndex = index / PagingConstans.MaxPageSize + 1;
        }

        public void RefreshPagingProperties()
        {
            if (_currentPageIndex < PagingConstans.FirstPageIndex)
                _currentPageIndex = PagingConstans.FirstPageIndex;

            if (_currentPageIndex > NumberOfPages)
                _currentPageIndex = NumberOfPages;

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentPageIndex"));
                PropertyChanged(this, new PropertyChangedEventArgs("NumberOfPages"));
            }
        }

        private void OnPageChanged()
        {
            if (PageChanged != null)
                PageChanged(this, new EventArgs());
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
