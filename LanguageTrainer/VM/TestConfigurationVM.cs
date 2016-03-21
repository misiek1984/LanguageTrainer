using System;
using System.Collections.Generic;
using System.Windows;
using System.Collections.ObjectModel;

using LanguageTrainer.Logic.LearningAlgorithms;

using MK.Utilities;
using MK.Settings;
using MK.UI.WPF;

using LanguageTrainer.Entities;
using LanguageTrainer.Common;
using LanguageTrainer.Interfaces;

namespace LanguageTrainer.VM
{
    public class TestConfigurationVM : ViewModelBase
    {
        #region Fields

        private IExpressionsChooser _defaultChooser;
        private IExpressionsChooser _flashCardsAlgorithmChooser;

        private ILearningAlgorithm _defaultLearningAlgorithm;
        private ILearningAlgorithm _flashCardsLearningAlgorithm;

        #endregion
        #region Properties

        private bool _allExpressions;
        [SettingsProperty]
        public bool AllExpressions
        {
            get { return _allExpressions; }
            set
            {
                if (_allExpressions != value)
                {
                    _allExpressions = value;
                    Notify("AllExpressions");
                }
            }
        }

        private bool _allExpressionsWithoutNew;
        [SettingsProperty]
        public bool AllExpressionsWithoutNew
        {
            get { return _allExpressionsWithoutNew; }
            set
            {
                if (_allExpressionsWithoutNew != value)
                {
                    _allExpressionsWithoutNew = value;
                    Notify("AllExpressionsWithoutNew");
                }
            }
        }

        private bool _onlyNewExpressions;
        [SettingsProperty]
        public bool OnlyNewExpressions
        {
            get { return _onlyNewExpressions; }
            set
            {
                if (_onlyNewExpressions != value)
                {
                    _onlyNewExpressions = value;
                    Notify("OnlyNewExpressions");
                }
            }
        }



        private bool _expressionsWithLastWrongAnswer;
        [SettingsProperty]
        public bool ExpressionsWithLastWrongAnswer
        {
            get { return _expressionsWithLastWrongAnswer; }
            set
            {
                if (_expressionsWithLastWrongAnswer != value)
                {
                    _expressionsWithLastWrongAnswer = value;
                    Notify("ExpressionsWithLastWrongAnswer");
                }
            }
        }

        private bool _onlySelected;
        [SettingsProperty]
        public bool OnlySelected
        {
            get { return _onlySelected; }
            set
            {
                if (_onlySelected != value)
                {
                    _onlySelected = value;
                    Notify("OnlySelected");
                }
            }
        }

        private bool _recentlyUsed;
        [SettingsProperty]
        public bool RecentlyUsed
        {
            get { return _recentlyUsed; }
            set
            {
                if (value != _recentlyUsed)
                {
                    _recentlyUsed = value;
                    Notify("RecentlyUsed");
                }
            }
        }

        private bool _reverse;
        [SettingsProperty]
        public bool Reverse
        {
            get { return _reverse; }
            set
            {
                if (_reverse != value)
                {
                    _reverse = value;
                    Notify("Reverse");
                }
            }
        }

        private bool _checkTheSpelling;
        [SettingsProperty]
        public bool CheckTheSpelling
        {
            get { return _checkTheSpelling; }
            set
            {
                if (_checkTheSpelling != value)
                {
                    _checkTheSpelling = value;
                    Notify("CheckTheSpelling");
                }
            }
        }



        private bool _flashCardsAlgorithm;
        [SettingsProperty]
        public bool FlashCardsAlgorithm
        {
            get { return _flashCardsAlgorithm; }
            set
            {
                if (value != _flashCardsAlgorithm)
                {
                    _flashCardsAlgorithm = value;
                    Notify(() => FlashCardsAlgorithm);
                }
            }
        }

        private List<LanguageVM> _availableLanguages;
        public List<LanguageVM> AvailableLanguages
        {
            get
            {
                if (_availableLanguages == null)
                {
                    _availableLanguages = new List<LanguageVM>();

                    foreach (var li in LanguagesProvider.Languages)
                        _availableLanguages.Add(new LanguageVM(this) { Lang = li.Id, Name = li.Name });
                }

                return _availableLanguages;
            }
        }

        private int _selectedLanguage = 1;
        [SettingsProperty]
        public  int SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                if (value != _selectedLanguage)
                {
                    _selectedLanguage = value;
                    Notify("SelectedLanguage");
                }
            }
        }

        private ObservableCollection<string> _availableCategories;
        public ObservableCollection<string> AvailableCategories
        {
            get
            {
                if (_availableCategories == null || _availableCategories.Count == 0)
                {
                    _availableCategories = new ObservableCollection<string>();
                    _availableCategories.Add(Resources.Lbl.All);
                    DataAccess.GetCategories().ForEach(c => _availableCategories.Add(c));
                }

                return _availableCategories;
            }
        }

        private string _selectedCategory = Resources.Lbl.All;
        [SettingsProperty]
        public string SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    Notify("SelectedCategory");
                }
            }
        }

        private int _numberOfExpressionsInTest = 5;
        [SettingsProperty]
        public string NumberOfExpressionsInTest
        {
            get { return _numberOfExpressionsInTest.ToString(); }
            set
            {
                int i;
                if (Int32.TryParse(value, out i))
                {
                    if (i < 5)
                        _numberOfExpressionsInTest = 5;
                    else if (i > 200)
                        _numberOfExpressionsInTest = 200;
                    else
                        _numberOfExpressionsInTest = i;

                    Notify("NumberOfExpressionsInTest");
                }
            }
        }

        private DateTime? _CreatedFromDate;
        [SettingsProperty]
        public DateTime? CreatedFromDate
        {
            get { return _CreatedFromDate; }
            set
            {
                if (_CreatedFromDate != value)
                {
                    _CreatedFromDate = value;
                    Notify(() => CreatedToDate);
                }
            }
        }

        private DateTime? _CreatedToDate;
        [SettingsProperty]
        public DateTime? CreatedToDate
        {
            get { return _CreatedToDate; }
            set
            {
                if (_CreatedToDate != value)
                {
                    _CreatedToDate = value;
                    Notify(() => CreatedToDate);
                }
            }
        }

        public IDataAccess DataAccess { get; set; }

        public IDateTimeProvider DateTimeProvider { get; set; }

        public ILanguagesProvider LanguagesProvider { get; set; }

        #endregion

        #region Constructor

        public TestConfigurationVM(MainWindowVM mainWindowVM)
            : base(mainWindowVM)
        {
            AllExpressions = true;
        }

        #endregion

        #region Commands

        private CustomCommand _startTrainingCommand;
        public CustomCommand StartTrainingCommand
        {
            get
            {
                if (_startTrainingCommand == null)
                    _startTrainingCommand = new CustomCommand(() => ExecuteStartTraining(true), () => true);

                return _startTrainingCommand;
            }
        }

        private CustomCommand _startTrainingCommandWithoutEvaluation;
        public CustomCommand StartTrainingCommandWithoutEvaluation
        {
            get
            {
                if (_startTrainingCommandWithoutEvaluation == null)
                    _startTrainingCommandWithoutEvaluation = new CustomCommand(() => ExecuteStartTraining(false), () => true);

                return _startTrainingCommandWithoutEvaluation;
            }
        }

        #endregion

        #region Methods

        public void RefreshCategories()
        {
            _availableCategories = null;
            Notify(() => AvailableCategories);
        }

        private TestConfiguration GetTestConfiguration()
        {
            var config = new TestConfiguration
                {
                    Language = SelectedLanguage,
                    NumberOfExpressionsInTest = _numberOfExpressionsInTest,
                    Category = SelectedCategory,

                    OnlySelected = OnlySelected,
                    RecentlyUsed = RecentlyUsed,

                    CheckTheSpelling = CheckTheSpelling,
                    Reverse = Reverse,

                    CreatedFromDate = CreatedFromDate,
                    CreateToDate = CreatedToDate
                };

            if(AllExpressions)
                config.TypeOfExpressions = TypeOfExpressions.All;
            else if (AllExpressionsWithoutNew)
                config.TypeOfExpressions = TypeOfExpressions.AllWithoutNew;
            else if (OnlyNewExpressions)
                config.TypeOfExpressions = TypeOfExpressions.New;
            else if (ExpressionsWithLastWrongAnswer)
                config.TypeOfExpressions = TypeOfExpressions.WithLastWrongAnswer;
            return config;
        }

        private void ExecuteStartTraining(bool canEvaluate)
        {
            ((MainWindowVM) Parent).Save();

            var config = GetTestConfiguration();

            ((MainWindowVM) Parent).ExpressionsVisibility = Visibility.Hidden;

            try
            {
                if (_defaultChooser == null)
                {
                    _defaultChooser = new DefaultExpressionChooser(DateTimeProvider, DataAccess);
                    _flashCardsAlgorithmChooser = new FlashCardsExpressionsChooser(DateTimeProvider, DataAccess);

                    _defaultLearningAlgorithm = new DefaultLearningAlgorithm(DateTimeProvider);
                    _flashCardsLearningAlgorithm = new FlashCardsLearningAlgorithm();
                }

                WindowService.ShowDialog(
                    new TestVM(
                        canEvaluate,
                        config,
                        FlashCardsAlgorithm ? _flashCardsAlgorithmChooser : _defaultChooser,
                        FlashCardsAlgorithm ? _flashCardsLearningAlgorithm : _defaultLearningAlgorithm));
            }
            catch (Exception ex)
            {
                WindowService.ShowError(ex);
            }

            ((MainWindowVM) Parent).ExpressionsVisibility = Visibility.Visible;
            ((MainWindowVM) Parent).RebuildList();
        }

        #endregion
    }
}
