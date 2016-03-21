using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using MK.Data;
using MK.Utilities;

using LanguageTrainer.Common;
using LanguageTrainer.Entities;
using LanguageTrainer.Interfaces;
using LanguageTrainer.Resources;

namespace LanguageTrainer.Logic
{
    public class LanguagesProvider : ILanguagesProvider
    {
        private readonly IEnumerable<LanguageInfo> _languages;

        public IEnumerable<LanguageInfo> AllLanguages
        {
            get { return _languages; }
        }

        public IEnumerable<LanguageInfo> Languages
        {
            get { return _languages.Where(li => !li.IsMain); }
        }

        public LanguagesProvider(IEnumerablePersister<LanguageInfo> persister)
        {
            persister.NotNull("persister");

            var file = Path.Combine(ApplicationPaths.ApplicationFolder, "Languages.xml");

            if (File.Exists(file))
            {
                _languages = persister.ImportFromFile(file);

                if (_languages.Any(li => (li.Id & (li.Id - 1)) != 0))
                    throw new Exception("All identifiers of languages should be a pow of two!");

                if (_languages.Count(li => li.IsMain) != 1)
                    throw new Exception("There must be exactly one main language!");

                if(_languages.GroupBy(li => li.Id).Any(g => g.Count() > 1))
                    throw new Exception("Languages must be unique!");
            }
            else
            {
                _languages = new List<LanguageInfo>
                    {
                        new LanguageInfo
                            {
                                BingTranslatorCode = "pl",
                                GoogleTranslatorCode = "pl",
                                Culture = "pl-PL",
                                IsMain = true,
                                Name = Res.Polish,
                            },
                        new LanguageInfo
                            {
                                Id = 1,
                                BingTranslatorCode = "de",
                                GoogleTranslatorCode = "de",
                                Culture = "de-DE",
                                Name = Res.German,
                                IconSource = Path.Combine(ApplicationPaths.ApplicationFolder, "German.png")
                            },
                        new LanguageInfo
                            {
                                Id = 2,
                                BingTranslatorCode = "en",
                                GoogleTranslatorCode = "en",
                                Culture = "en-GB",
                                Name = Res.English,
                                IconSource = Path.Combine(ApplicationPaths.ApplicationFolder, "English.png")
                            },
                        new LanguageInfo
                            {
                                Id = 4,
                                BingTranslatorCode = "zh-CHS",
                                GoogleTranslatorCode = "zh-CN",
                                Culture = "zh-Hans",
                                Name = Res.Chinese,
                                UseTranslation2 = true,
                                IconSource = Path.Combine(ApplicationPaths.ApplicationFolder, "Chinese.png")
                            },
                    };

                persister.ExportToFile(file, _languages);

                Res.German1.Save(Path.Combine(ApplicationPaths.ApplicationFolder, "German.png"), ImageFormat.Png);
                Res.English1.Save(Path.Combine(ApplicationPaths.ApplicationFolder, "English.png"), ImageFormat.Png);
                Res.Chinese1.Save(Path.Combine(ApplicationPaths.ApplicationFolder, "Chinese.png"), ImageFormat.Png);
            }
        }

        public LanguageInfo GetInfo(int id)
        {
            return _languages.First(li => li.Id == id);
        }

        public LanguageInfo TryGetInfo(int id)
        {
            return _languages.FirstOrDefault(li => li.Id == id);
        }
    }
}
