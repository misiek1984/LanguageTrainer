using System.Threading.Tasks;
using LanguageTrainer.Entities;

namespace LanguageTrainer.Interfaces
{
    public enum TranslateApi
    {
        All,
        Bing,
        Google
    }

    public interface ITranslator
    {
        Task<string> Translate(string text, LanguageInfo from, LanguageInfo to, TranslateApi api);
    }
}
