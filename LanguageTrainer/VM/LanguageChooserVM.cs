using System;
using System.Linq;
using System.Collections.ObjectModel;

using MK.Settings;
using MK.UI.WPF;
using MK.Utilities;

using LanguageTrainer.Interfaces;

namespace LanguageTrainer.VM
{
    public class LanguageChooserVM : ViewModelBase
    {
        #region Properties

        public ILanguagesProvider LanguagesProvider { get; set; }

        private ObservableCollection<LanguageVM> _AvailableLanguages;
        public ObservableCollection<LanguageVM> AvailableLanguages
        {
            get
            {
                if (_AvailableLanguages == null)
                {
                    _AvailableLanguages = new ObservableCollection<VM.LanguageVM>();
                    foreach (var li in LanguagesProvider.Languages)
                        AvailableLanguages.Add(new LanguageVM(this) { Lang = li.Id, Name = li.Name });
                }

                return _AvailableLanguages;
            }
        }
       
        [SettingsProperty]
        public int SelectedLanguages
        {
            get
            {
                int flag = 0;
                foreach (var ln in AvailableLanguages)
                    if(ln.IsSelected)
                        flag |= ln.Lang;

                return flag;
            }
            set
            {
                foreach (var li in LanguagesProvider.Languages)
                    if((value & li.Id) != 0)
                        AvailableLanguages.First(l => l.Lang == li.Id).IsSelected = true;
            }
        }

        #endregion

        #region Constructor

        public LanguageChooserVM(MainWindowVM mainWindowVM)
            :base(mainWindowVM)
        {
        }

        #endregion
    }
}
