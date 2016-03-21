using System;
using System.Linq;
using System.Net;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Microsoft;

using MK.Logging;

using LanguageTrainer.Entities;
using LanguageTrainer.Interfaces;

namespace LanguageTrainer.Logic.Translator
{
    public class Translator : ITranslator
    {
        private bool _googleTranslatorEnabled;
        private bool _bingTranslatorEnabled;

        private TranslatorContainer _bingTranslator;
        //private GoogleTranslator _googleTranslator;

        private uint _bingTimeout = 0;
        private uint _googleTimeout = 0;

        public Translator()
        {
            var res = ConfigurationManager.AppSettings["BingTranslatorTimeout"];
            UInt32.TryParse(res, out _bingTimeout);

            res = ConfigurationManager.AppSettings["GoogleTranslatorTimeout"];
            UInt32.TryParse(res, out _googleTimeout);

            var res2 = ConfigurationManager.AppSettings["BingTranslatorEnabled"];
            Boolean.TryParse(res2, out _bingTranslatorEnabled);

            res2 = ConfigurationManager.AppSettings["GoogleTranslatorEnabled"];
            Boolean.TryParse(res2, out _googleTranslatorEnabled);
        }


        public async Task<string> Translate(string text, LanguageInfo from, LanguageInfo to, TranslateApi api)
        {
            if (String.IsNullOrWhiteSpace(text))
                return null;

            string bingResult = null;
            string googleResult = null;

            await Task.Factory.StartNew(() => bingResult = TranslateWithBing(text, @from, to, api));
            await Task.Factory.StartNew(() => googleResult = TranslateWithGoogle(text, @from, to, api));

            if (bingResult == null)
                return googleResult;

            if (googleResult == null)
                return bingResult;

            return bingResult + Environment.NewLine + googleResult;
        }

        private string TranslateWithGoogle(string text, LanguageInfo from, LanguageInfo to, TranslateApi api)
        {
            //if (!_googleTranslatorEnabled)
            //    return null;

            //if (api != TranslateApi.Google && api != TranslateApi.All)
            //    return null;

            //string result;

            //try
            //{
            //    if (_googleTranslator == null)
            //        _googleTranslator = new GoogleTranslator();

            //    _googleTranslator.SourceLanguage = @from.GoogleTranslatorCode;
            //    _googleTranslator.TargetLanguage = to.GoogleTranslatorCode;
            //    _googleTranslator.SourceText = text;
            //    _googleTranslator.Timeout = (int)_googleTimeout;

            //    _googleTranslator.Translate();
            //    result = _googleTranslator.Translation;
            //}
            //catch (Exception ex)
            //{
            //    Log.LogException(ex);
            //    result = Resources.Res.TranslationError;
            //}

            //return Resources.Res.Google + result;

            return null;
        }

        private string TranslateWithBing(string text, LanguageInfo from, LanguageInfo to, TranslateApi api)
        {
            if (!_bingTranslatorEnabled)
                return null;

            if (api != TranslateApi.Bing && api != TranslateApi.All)
                return null;

            string result;

            try
            {
                if (_bingTranslator == null)
                {
                    _bingTranslator =
                        new TranslatorContainer(new Uri(ConfigurationManager.AppSettings["TranslatorAddress"]))
                            {
                                Credentials = new NetworkCredential(
                                    ConfigurationManager.AppSettings["TranslatorAccount"],
                                    ConfigurationManager.AppSettings["TranslatorAccountKey"])
                            };

                    _bingTranslator.Timeout = (int)_bingTimeout;
                }

                var translationResult =
                    _bingTranslator.Translate(text, to.BingTranslatorCode, @from.BingTranslatorCode).ToList();

                result = translationResult.Select(t => t.Text).Aggregate((current, next) => current + ", " + next);
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
                result = Resources.Res.TranslationError;
            }

            return Resources.Res.Bing + result;
        }
    }
}

