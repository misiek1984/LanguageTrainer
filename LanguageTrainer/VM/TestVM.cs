using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.ComponentModel;

using MK.Settings;
using MK.Utilities;
using MK.UI.WPF;

using LanguageTrainer.Entities;
using LanguageTrainer.Interfaces;

namespace LanguageTrainer.VM
{
    public class TestVM : ViewModelBase
    {
        #region Fields & Properties

        #region Translate Panel

        public TranslateVM TranslatePanel { get; set; }

        private bool _isTranslationExpanderExpanded;
        [SettingsProperty]
        public bool IsTranslationExpanderExpanded
        {
            get { return _isTranslationExpanderExpanded; }
            set
            {
                if (value != _isTranslationExpanderExpanded)
                {
                    _isTranslationExpanderExpanded = value;
                    Notify(() => IsTranslationExpanderExpanded);
                }
            }
        }

        #endregion

        #region Statistics

        private int GoodAnswers { get; set; }
        private int BadAnswers { get; set; }

        #endregion

        #region Expressions

        public ExpressionEntity Current
        {
            get { return ExpressionsView.CurrentItem as ExpressionEntity; }
        }

        private IList<ExpressionEntity> Expressions { get; set; }

        private ICollectionView ExpressionsView { get; set; }

        private int LastPosition { get; set; }

        #endregion

        #region Configuration

        private bool CanEvaluate { get; set; }

        public TestConfiguration Config { get; set; }

        public LanguageInfo LanguageInfo { get; set; }

        private ILearningAlgorithm Algorithm { get; set; }

        #endregion

        #region Services

        public ISpecialSymbolsProvider SpecialSymbolsProvider { get; set; }
        public ISpeechEngine SpeechEngine { get; set; }
        public ILanguagesProvider LanguagesProvider { get; set; }
        public IDataAccess DataAccess { get; set; }

        #endregion

        public override string Name { get { return Resources.Lbl.Test; } }

        public string Title
        {
            get { return String.Format("{0}/{1}", ExpressionsView.CurrentPosition + 1, Expressions.Count); }
        }

        public string Expression
        {
            get { return Config.Reverse ? Current[Config.Language].Translation : Current.Expression; }
            set
            {
                if (Config.Reverse)
                {
                    Current[Config.Language].Translation = value;
                }
                else
                {
                    Current.Expression = value;
                }
            }
        }

        public string Translation
        {
            get { return Config.Reverse ? Current.Expression : Current[Config.Language].Translation; }
            set
            {
                if (Config.Reverse)
                {
                    Current.Expression = value;
                }
                else
                {
                    Current[Config.Language].Translation = value;
                }
            }
        }

        public string Translation2
        {
            get { return Current[Config.Language].Translation2; }
            set { Current[Config.Language].Translation2 = value; }
        }

        public string Category
        {
            get { return Current.Category; }
        }

        private string _spelling;
        public string Spelling
        {
            get { return _spelling; }
            set
            {
                _spelling = value;
                Notify(() => Spelling);
            }
        }

        private bool _canShowAnswer;
        public bool CanShowAnswer
        {
            get { return _canShowAnswer; }
            set
            {
                _canShowAnswer = value;
                Notify(() => CanShowAnswer);
            }
        }

        private bool _showEvaluationButtons = true;
        public bool ShowEvaluationButtons
        {
            get { return _showEvaluationButtons && CanEvaluate; }
            set
            {
                _showEvaluationButtons = value;
                Notify(() => ShowEvaluationButtons);
            }
        }

        public string SpecialSymbols
        {
            get { return SpecialSymbolsProvider.SpecialSymbols; }
        }

        #endregion

        #region Commands

        private CustomCommand _nextCommand;
        public CustomCommand NextCommand
        {
            get
            {
                if (_nextCommand == null)
                    _nextCommand = new CustomCommand(() => Next(null), () => true);

                return _nextCommand;
            }
        }

        private CustomCommand _prevCommand;
        public CustomCommand PrevCommand
        {
            get
            {
                if (_prevCommand == null)
                    _prevCommand = new CustomCommand(() => Prev(), () => true);

                return _prevCommand;
            }
        }

        private CustomCommand _showAnswerCommand;
        public CustomCommand ShowAnswerCommand
        {
            get
            {
                if (_showAnswerCommand == null)
                    _showAnswerCommand = new CustomCommand(() => CanShowAnswer = true, () => true);

                return _showAnswerCommand;
            }
        }

        private CustomCommand _showAllTranslations;
        public CustomCommand ShowAllTranslations
        {
            get
            {
                if (_showAllTranslations == null)
                    _showAllTranslations = new CustomCommand(() =>
                        {
                            WindowService.ShowDialog(ServiceProvider.Inject(new ExpressionVM(null, Current)));
                        },
                        () => true);

                return _showAllTranslations;
            }
        }

        private CustomCommand _goodAnswerCommand;
        public CustomCommand GoodAnswerCommand
        {
            get
            {
                if (_goodAnswerCommand == null)
                    _goodAnswerCommand = new CustomCommand(() => Next(true), () => true);

                return _goodAnswerCommand;
            }
        }

        private CustomCommand _badAnswerCommand;
        public CustomCommand BadAnswerCommand
        {
            get
            {
                if (_badAnswerCommand == null)
                    _badAnswerCommand = new CustomCommand(() => Next(false), () => true);

                return _badAnswerCommand;
            }
        }

        public CustomCommand SpeakCommand
        {
            get
            {
                return new CustomCommand(() =>
                    {
                        SpeechEngine.Speak(LanguageInfo.Culture, Current[Config.Language].Translation);
                        if(!Config.Reverse)
                            ShowAnswerCommand.Execute(null);
                    }, () => true);
            }
        }

        #endregion

        #region Constructor

        public TestVM(bool canEvaluate, TestConfiguration config, IExpressionsChooser chooser, ILearningAlgorithm algorithm)
        {
            config.NotNull("config");
            chooser.NotNull("chooser");
            algorithm.NotNull("algorithm");
            
            CanEvaluate = canEvaluate;
            Config = config;
            Algorithm = algorithm;

            Expressions = chooser.SelectExpressions(Config);
            ExpressionsView = CollectionViewSource.GetDefaultView(Expressions);
        }

        #endregion

        #region Methods

        public override bool BeforeShow()
        {
            LanguageInfo = LanguagesProvider.GetInfo(Config.Language);

            TranslatePanel = ServiceProvider.Inject(new TranslateVM()
            {
                From =
                        LanguagesProvider.AllLanguages.Single(li => li.IsMain).Id,
                To =
                        LanguagesProvider.Languages.Single(li => li.Id == Config.Language).Id
            });

            if (Expressions.Count == 0)
            {
                WindowService.ShowMessage(Resources.Msg.NoExpressionsFound);
                Close(this);
                return false;
            }

            return true;
        }

        public override void BeforeClose()
        {
            try
            {
                DataAccess.Save(Expressions);
            }
            catch (Exception ex)
            {
                WindowService.ShowError(ex);
            }
        }

        private void Next(bool? lastAnswer)
        {
            UpdateStatistics(lastAnswer);

            ExpressionsView.MoveCurrentToNext();

            if (ExpressionsView.CurrentPosition >= LastPosition)
            {
                CanShowAnswer = false;
                ShowEvaluationButtons = true;
                LastPosition = ExpressionsView.CurrentPosition;
            }
            else
            {
                CanShowAnswer = true;
                ShowEvaluationButtons = false;
            }

            if (CheckIfLast())
                return;

            Spelling = null;

            Notify(() => Expression);
            Notify(() => Translation);
            Notify(() => Translation2);
            Notify(() => Title);
            Notify(() => Category);
        }
        private void Prev()
        {
            if (ExpressionsView.CurrentPosition <= 0)
                return;

            CanShowAnswer = true;
            ShowEvaluationButtons = false;
            ExpressionsView.MoveCurrentToPrevious();

            Notify(() => Expression);
            Notify(() => Translation);
            Notify(() => Translation2);
            Notify(() => Title);
            Notify(() => Category);
        }

        private void UpdateStatistics(bool? lastAnswer)
        {
            if (lastAnswer.HasValue)
            {
                if (lastAnswer.Value)
                {
                    Algorithm.Mark(true, Current, Config);
                    GoodAnswers++;
                }
                else
                {
                    Algorithm.Mark(false, Current, Config);
                    BadAnswers++;
                }
            }
        }

        private bool CheckIfLast()
        {
            if (Current == null)
            {
                Close(this);

                if (CanEvaluate)
                {
                    WindowService.ShowMessage(String.Format(Resources.Msg.Summary, GoodAnswers, BadAnswers, Expressions.Count));
                }

                return true;
            }

            return false;
        }

        #endregion
    }
}
