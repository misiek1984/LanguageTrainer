//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;


//namespace LanguageTrainer.Logic.Translator
//{
//    /// <summary>
//    /// http://www.codeproject.com/Articles/12711/Google-Translator
//    /// </summary>
//    public class GoogleTranslator : RavSoft.WebResourceProvider
//    {
//        #region Constructor

//        /// <summary>
//        /// Initializes a new instance of the <see cref="GoogleTranslator"/> class.
//        /// </summary>
//        public GoogleTranslator()
//        {
//            this.SourceLanguage = "English";
//            this.TargetLanguage = "French";
//            this.Referer = "http://translate.google.com/";
//        }

//        #endregion

//        #region Properties

//        /// <summary>
//        /// Gets or sets the source language.
//        /// </summary>
//        /// <value>The source language.</value>
//        public string SourceLanguage
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// Gets or sets the target language.
//        /// </summary>
//        /// <value>The target language.</value>
//        public string TargetLanguage
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// Gets or sets the source text.
//        /// </summary>
//        /// <value>The source text.</value>
//        public string SourceText
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// Gets the translation.
//        /// </summary>
//        /// <value>The translated text.</value>
//        public string Translation
//        {
//            get;
//            private set;
//        }

//        /// <summary>
//        /// Gets the url used to speak the translation.
//        /// </summary>
//        /// <value>The url used to speak the translation.</value>
//        public string TranslationSpeakUrl
//        {
//            get;
//            private set;
//        }

//        /// <summary>
//        /// Gets the time taken to perform the translation.
//        /// </summary>
//        /// <value>The time taken to perform the translation.</value>
//        public TimeSpan TranslationTime
//        {
//            get;
//            private set;
//        }

//        /// <summary>
//        /// Gets the supported languages.
//        /// </summary>
//        public static IEnumerable<string> Languages
//        {
//            get
//            {
//                GoogleTranslator.EnsureInitialized();
//                return GoogleTranslator._languageModeMap.Keys.OrderBy(p => p);
//            }
//        }

//        #endregion

//        #region Public methods

//        /// <summary>
//        /// Attempts to translate the text.
//        /// </summary>
//        public void Translate()
//        {
//            // Validate source and target languages
//            if (string.IsNullOrEmpty(this.SourceLanguage) ||
//                string.IsNullOrEmpty(this.TargetLanguage) ||
//                this.SourceLanguage.Trim().Equals(this.TargetLanguage.Trim()))
//            {
//                throw new Exception("An invalid source or target language was specified.");
//            }

//            // Delegate to base class
//            DateTime start = DateTime.Now;
//            this.fetchResource();
//            this.TranslationTime = DateTime.Now - start;
//        }

//        #endregion

//        #region WebResourceProvider implementation

//        /// <summary>
//        /// Returns the url to be fetched.
//        /// </summary>
//        /// <returns>The url to be fetched.</returns>
//        protected override string getFetchUrl()
//        {
//            string url = string.Format("http://translate.google.com/translate_a/t?client=t&sl={0}&tl={1}&ie=UTF-8&oe=UTF-8&q={2}",
//                                        GoogleTranslator.LanguageEnumToIdentifier(this.SourceLanguage),
//                                        GoogleTranslator.LanguageEnumToIdentifier(this.TargetLanguage),
//                                        HttpUtility.UrlEncode(this.SourceText));

//            return url;
//        }

//        /// <summary>
//        /// Parses the fetched content.
//        /// </summary>
//        protected override void parseContent()
//        {
//            // Initialize the parser
//            this.Translation = string.Empty;
//            string strContent = this.Content;

//            RavSoft.StringParser parser = new RavSoft.StringParser(strContent);

//            // Extract the translation
//            string strTranslation = string.Empty;
//            if (parser.skipToEndOf("[[[\""))
//            {
//                bool morePhrasesRemaining = true;
//                do
//                {
//                    string translatedPhrase = null;
//                    if (parser.extractTo("\",\"", ref translatedPhrase))
//                    {
//                        strTranslation += translatedPhrase;
//                    }
//                    morePhrasesRemaining = parser.skipToEndOf(",\"\",\"\"],[\"");
//                }
//                while (morePhrasesRemaining);
//            }
//            this.Translation = strTranslation.Replace(" .", ".").Replace(" ?", "?").Replace(" ,", ",").Replace(" ;", ";").Replace(" !", "!");

//            // Set the translation speak url
//            this.TranslationSpeakUrl = string.Format("http://translate.google.com/translate_tts?ie=UTF-8&tl={0}&q={1}",
//                                                      GoogleTranslator.LanguageEnumToIdentifier(this.TargetLanguage),
//                                                      HttpUtility.UrlEncode(this.Translation));

//            if (String.IsNullOrWhiteSpace(Translation))
//                Translation = ErrorMsg;
//        }

//        protected override bool continueFetching()
//        {
//            return base.continueFetching();
//        }

//        protected override string getPostData()
//        {
//            return base.getPostData();
//        }

//        #endregion

//        #region Private methods

//        /// <summary>
//        /// Converts a language to its identifier.
//        /// </summary>
//        /// <param name="language">The language."</param>
//        /// <returns>The identifier or <see cref="string.Empty"/> if none.</returns>
//        private static string LanguageEnumToIdentifier
//            (string language)
//        {
//            string mode = string.Empty;
//            GoogleTranslator.EnsureInitialized();
//            GoogleTranslator._languageModeMap.TryGetValue(language, out mode);
//            return mode;
//        }

//        /// <summary>
//        /// Ensures the translator has been initialized.
//        /// </summary>
//        private static void EnsureInitialized()
//        {
//            if (GoogleTranslator._languageModeMap == null)
//            {
//                GoogleTranslator._languageModeMap = new Dictionary<string, string>();
//                GoogleTranslator._languageModeMap.Add("Afrikaans", "af");
//                GoogleTranslator._languageModeMap.Add("Albanian", "sq");
//                GoogleTranslator._languageModeMap.Add("Arabic", "ar");
//                GoogleTranslator._languageModeMap.Add("Armenian", "hy");
//                GoogleTranslator._languageModeMap.Add("Azerbaijani", "az");
//                GoogleTranslator._languageModeMap.Add("Basque", "eu");
//                GoogleTranslator._languageModeMap.Add("Belarusian", "be");
//                GoogleTranslator._languageModeMap.Add("Bengali", "bn");
//                GoogleTranslator._languageModeMap.Add("Bulgarian", "bg");
//                GoogleTranslator._languageModeMap.Add("Catalan", "ca");
//                GoogleTranslator._languageModeMap.Add("Chinese", "zh-CN");
//                GoogleTranslator._languageModeMap.Add("Croatian", "hr");
//                GoogleTranslator._languageModeMap.Add("Czech", "cs");
//                GoogleTranslator._languageModeMap.Add("Danish", "da");
//                GoogleTranslator._languageModeMap.Add("Dutch", "nl");
//                GoogleTranslator._languageModeMap.Add("English", "en");
//                GoogleTranslator._languageModeMap.Add("Esperanto", "eo");
//                GoogleTranslator._languageModeMap.Add("Estonian", "et");
//                GoogleTranslator._languageModeMap.Add("Filipino", "tl");
//                GoogleTranslator._languageModeMap.Add("Finnish", "fi");
//                GoogleTranslator._languageModeMap.Add("French", "fr");
//                GoogleTranslator._languageModeMap.Add("Galician", "gl");
//                GoogleTranslator._languageModeMap.Add("German", "de");
//                GoogleTranslator._languageModeMap.Add("Georgian", "ka");
//                GoogleTranslator._languageModeMap.Add("Greek", "el");
//                GoogleTranslator._languageModeMap.Add("Haitian Creole", "ht");
//                GoogleTranslator._languageModeMap.Add("Hebrew", "iw");
//                GoogleTranslator._languageModeMap.Add("Hindi", "hi");
//                GoogleTranslator._languageModeMap.Add("Hungarian", "hu");
//                GoogleTranslator._languageModeMap.Add("Icelandic", "is");
//                GoogleTranslator._languageModeMap.Add("Indonesian", "id");
//                GoogleTranslator._languageModeMap.Add("Irish", "ga");
//                GoogleTranslator._languageModeMap.Add("Italian", "it");
//                GoogleTranslator._languageModeMap.Add("Japanese", "ja");
//                GoogleTranslator._languageModeMap.Add("Korean", "ko");
//                GoogleTranslator._languageModeMap.Add("Lao", "lo");
//                GoogleTranslator._languageModeMap.Add("Latin", "la");
//                GoogleTranslator._languageModeMap.Add("Latvian", "lv");
//                GoogleTranslator._languageModeMap.Add("Lithuanian", "lt");
//                GoogleTranslator._languageModeMap.Add("Macedonian", "mk");
//                GoogleTranslator._languageModeMap.Add("Malay", "ms");
//                GoogleTranslator._languageModeMap.Add("Maltese", "mt");
//                GoogleTranslator._languageModeMap.Add("Norwegian", "no");
//                GoogleTranslator._languageModeMap.Add("Persian", "fa");
//                GoogleTranslator._languageModeMap.Add("Polish", "pl");
//                GoogleTranslator._languageModeMap.Add("Portuguese", "pt");
//                GoogleTranslator._languageModeMap.Add("Romanian", "ro");
//                GoogleTranslator._languageModeMap.Add("Russian", "ru");
//                GoogleTranslator._languageModeMap.Add("Serbian", "sr");
//                GoogleTranslator._languageModeMap.Add("Slovak", "sk");
//                GoogleTranslator._languageModeMap.Add("Slovenian", "sl");
//                GoogleTranslator._languageModeMap.Add("Spanish", "es");
//                GoogleTranslator._languageModeMap.Add("Swahili", "sw");
//                GoogleTranslator._languageModeMap.Add("Swedish", "sv");
//                GoogleTranslator._languageModeMap.Add("Tamil", "ta");
//                GoogleTranslator._languageModeMap.Add("Telugu", "te");
//                GoogleTranslator._languageModeMap.Add("Thai", "th");
//                GoogleTranslator._languageModeMap.Add("Turkish", "tr");
//                GoogleTranslator._languageModeMap.Add("Ukrainian", "uk");
//                GoogleTranslator._languageModeMap.Add("Urdu", "ur");
//                GoogleTranslator._languageModeMap.Add("Vietnamese", "vi");
//                GoogleTranslator._languageModeMap.Add("Welsh", "cy");
//                GoogleTranslator._languageModeMap.Add("Yiddish", "yi");
//            }
//        }

//        #endregion

//        #region Fields

//        /// <summary>
//        /// The language to translation mode map.
//        /// </summary>
//        private static Dictionary<string, string> _languageModeMap;

//        #endregion
//    }
//}
