using System.Collections.Generic;
using LanguageTrainer.Entities;

namespace LanguageTrainer.Interfaces
{
    public interface ILanguagesProvider
    {
        IEnumerable<LanguageInfo> AllLanguages { get; }

        IEnumerable<LanguageInfo> Languages { get; }

        LanguageInfo GetInfo(int id);

        LanguageInfo TryGetInfo(int id);
    }
}
