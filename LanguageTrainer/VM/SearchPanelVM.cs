using System.Collections.Generic;
using System.Collections.ObjectModel;

using MK.Utilities;
using MK.UI.WPF;
using MK.Settings;

using LanguageTrainer.Entities;
using LanguageTrainer.Common;
using LanguageTrainer.Interfaces;

namespace LanguageTrainer.VM
{
    public class SearchPanelVM : ViewModelBase
    {
        #region Properties

        [SettingsProperty]
        public bool MatchWholeWord { get; set; }

        [SettingsProperty]
        public string TextToFind { get; set; }

        [SettingsProperty]
        public string SelectedCategory { get; set; }

        private ObservableCollection<string> _availableCategories;
        public ObservableCollection<string> AvailableCategories
        {
            get
            {
                if (_availableCategories == null || _availableCategories.Count == 0)
                {
                    _availableCategories = new ObservableCollection<string> { Resources.Lbl.All };
                    DataAccess.GetCategories().ForEach(c => _availableCategories.Add(c));
                }

                return _availableCategories;
            }
        }



        public IDataAccess DataAccess { get; set; }

        #endregion

        #region Commands

        private CustomCommand _findExpressionsCommand;
        public CustomCommand FindExpressionsCommand
        {
            get
            {
                if (_findExpressionsCommand == null)
                    _findExpressionsCommand = new CustomCommand(FindExpressions, () => true);

                return _findExpressionsCommand;
            }
        }

        #endregion

        #region Constructor

        public SearchPanelVM(MainWindowVM parent)
            : base(parent)
        {
            SelectedCategory = Resources.Lbl.All;
        }

        #endregion

        #region Methods

        public void RefreshCategories()
        {
            _availableCategories = null;
            Notify(() => AvailableCategories);
        }

        private void FindExpressions()
        {
            ((MainWindowVM)Parent).Save();
            ((MainWindowVM)Parent).RebuildList(true, true);
        }

        #endregion
    }
}
