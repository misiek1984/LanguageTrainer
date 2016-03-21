using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using LanguageTrainer.Entities;
using LanguageTrainer.Interfaces;

using MK.UI.WPF;
using MK.Settings;
using MK.UI.WPF.VM;
using MK.Utilities;

namespace LanguageTrainer.VM
{
    public class TranslateVM : ViewModelBase
    {
        #region Properties

        private int _from;
        [SettingsProperty]
        public int From
        {
            get { return _from; }
            set
            {
                if (value != _from)
                {
                    _from = value;
                    Notify(() => From);
                }
            }
        }

        private int _to;
        [SettingsProperty]
        public int To
        {
            get { return _to; }
            set
            {
                if (value != _to)
                {
                    _to = value;
                    Notify(() => To);
                }
            }
        }


        private string _expressionToTranslate;
        [SettingsProperty]
        public string ExpressionToTranslate
        {
            get { return _expressionToTranslate; }
            set
            {
                if (value != _expressionToTranslate)
                {
                    _expressionToTranslate = value;
                    TranslationResult = null;
                    Notify(() => ExpressionToTranslate);
                }
            }
        }

        private string _translationResult;
        [SettingsProperty]
        public string TranslationResult
        {
            get { return _translationResult; }
            set
            {
                if (value != _translationResult)
                {
                    _translationResult = value;
                    Notify(() => TranslationResult);
                }
            }
        }

        private IEnumerable<LanguageInfo> _availableLanguages;
        public IEnumerable<LanguageInfo> AvailableLanguages
        {
            get
            {
                if (_availableLanguages == null)
                    _availableLanguages = LanguagesProvider.AllLanguages;
                
                return _availableLanguages;
            }
        }



        public ITranslator Translator { get; set; }
        public ILanguagesProvider LanguagesProvider { get; set; }

        #endregion

        #region Commands

        private CustomCommand _goToGoogleTranslate;
        public CustomCommand GoToGoogleTranslate
        {
            get
            {
                if (_goToGoogleTranslate == null)
                    _goToGoogleTranslate = new CustomCommand(
                        () =>
                            {
                                var from = LanguagesProvider.GetInfo(From).GoogleTranslatorCode;
                                var to = LanguagesProvider.GetInfo(To).GoogleTranslatorCode;

                                if (String.IsNullOrEmpty(ExpressionToTranslate))
                                    WindowService.ShowWebPage(new Uri("https://translate.google.com/"), true);
                                else
                                {
                                    var url = String.Format("https://translate.google.com/#{0}/{1}/{2}", from, to, ExpressionToTranslate);
                                    WindowService.ShowWebPage(new Uri(url), true);
                                }
                            },
                        () => true);

                return _goToGoogleTranslate;
            }
        }

        private AsyncCustomCommand _translateCommand;
        public AsyncCustomCommand TranslateCommand
        {
            get
            {
                if (_translateCommand == null)
                    _translateCommand = new AsyncCustomCommand(Translate, autoCanExecute: true);

                return _translateCommand;
            }
        }
        private async Task Translate()
        {
            var from = LanguagesProvider.GetInfo(From);
            var to = LanguagesProvider.GetInfo(To);
            TranslationResult = await Translator.Translate(ExpressionToTranslate, from, to, TranslateApi.All);
        }

        private CustomCommand _swapLanguagesCommand;
        public CustomCommand SwapLanguagesCommand
        {
            get
            {
                if (_swapLanguagesCommand == null)
                    _swapLanguagesCommand = new CustomCommand(ExecuteSwapLanguagesCommand, () => true);

                return _swapLanguagesCommand;
            }
        }
        private void ExecuteSwapLanguagesCommand()
        {
            var temp = From;
            From = To;
            To = temp;
        }

        #endregion
    }
}
