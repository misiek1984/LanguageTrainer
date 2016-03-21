using System;
using System.Windows;

using MK.Utilities;
using MK.UI.WPF;

using LanguageTrainer.Entities;
using LanguageTrainer.Interfaces;

namespace LanguageTrainer.VM
{
    public class TranslationVM : TypedVM<TranslationEntity>
    {
        #region Properties

        public LanguageInfo _LanguageInfo;
        public LanguageInfo LanguageInfo
        {
            get
            {
                if(_LanguageInfo == null)
                    _LanguageInfo = LanguagesProvider.GetInfo(Entity.Language);

                return _LanguageInfo;
            }
        }

        public override bool IsSelected
        {
            get { return Entity.IsSelected; }
            set
            {
                Entity.IsSelected = value;
                Notify(() => IsSelected);
            }
        }

        public int Language
        {
            get { return Entity.Language; }
        }

        public string Translation
        {
            get
            {
                return Entity.Translation;
            }
            set
            {
                if (Entity.Translation != value)
                {
                    if (Entity.Defined == null && String.IsNullOrEmpty(Entity.Translation) && !String.IsNullOrEmpty(value))
                    {
                        Entity.Defined = DateTimeProvider.Current;
                    }

                    Entity.Translation = value;
                    Notify(() => Translation);                   
                }
            }
        }

        public string Translation2
        {
            get
            {
                return Entity.Translation2;
            }
            set
            {
                if (Entity.Translation2 != value)
                {
                    Entity.Translation2 = value;
                    Notify(() => Translation2);
                }
            }
        }

        public string Statistic
        {
            get
            {
                return String.Format("{0}/{1}", Entity.GoodAnswers, Entity.BadAnswers);
            }
        }
        public string ReverseStatistic
        {
            get
            {
                return String.Format("{0}/{1}", Entity.ReverseGoodAnswers, Entity.ReverseBadAnswers);
            }
        }
        public string SpellingStatistic
        {
            get
            {
                return String.Format("{0}/{1}", Entity.SpellingGoodAnswers, Entity.SpellingBadAnswers);
            }
        }
        public string ReverseSpellingStatistic
        {
            get
            {
                return String.Format("{0}/{1}", Entity.ReverseSpellingGoodAnswers, Entity.ReverseSpellingBadAnswers);
            }
        }

        public bool WasLastAnswerGood
        {
            get
            {
                return Entity.WasLastAnswerGood;
            }
        }
        public bool ReverseWasLastAnswerGood
        {
            get
            {
                return Entity.ReverseWasLastAnswerGood;
            }
        }
        public bool SpellingWasLastAnswerGood
        {
            get
            {
                return Entity.SpellingWasLastAnswerGood;
            }
        }
        public bool ReverseSpellingWasLastAnswerGood
        {
            get
            {
                return Entity.ReverseSpellingWasLastAnswerGood;
            }
        }

        public DateTime? RecentlyUsed
        {
            get { return Entity.RecentlyUsed; }
        }

        public string IconSource
        {
            get { return LanguageInfo.IconSource; }
        }

        public Visibility TranslationVisibility
        {
            get
            {
                var mainVM = ((MainWindowVM) Parent.Parent);

                if (!mainVM.ShowEmptyTranslations && String.IsNullOrEmpty(Translation))
                    return Visibility.Collapsed;

                return (mainVM.Languages.SelectedLanguages & Language) != 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }



        public ILanguagesProvider LanguagesProvider { get; set; }
        public ISpeechEngine SpeechEngine { get; set; }
        public IDateTimeProvider DateTimeProvider { get; set; }
        public IDataAccess DataAccess { get; set; }

        #endregion

        #region Commands

        public CustomCommand SpeakCommand
        {
            get
            {
                return new CustomCommand(() => ExecuteSpeak(), () => true);
            }
        }

        private void ExecuteSpeak()
        {
            SpeechEngine.Speak(LanguageInfo.Culture, Translation);
        }

        #endregion

        #region Constructor

        public TranslationVM(ExpressionVM parent, TranslationEntity translation)
            : base(parent)
        {
            translation.NotNull("expression");
            Entity = translation;
        }

        #endregion

        #region Methods

        public override void Validate()
        {
            if (Parent is ExpressionVM)
            {
                if (!DataAccess.IsTranslationUnique(Entity))
                    AddError("Translation", Resources.Msg.NotUnique);
                else
                    ClearError("Translation");
            }
        }

        #endregion
    }
}

