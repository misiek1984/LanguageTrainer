using System.IO;

using LanguageTrainer.Common;
using LanguageTrainer.Interfaces;

namespace LanguageTrainer.Logic
{
    public class SpecialSymbolsProvider : ISpecialSymbolsProvider
    {
        public string SpecialSymbols { get; private set; }

        public SpecialSymbolsProvider()
        {
            var file = Path.Combine(ApplicationPaths.ApplicationFolder, "SpecialSymbols");

            if (File.Exists(file))
                SpecialSymbols = File.ReadAllText(file);
            else
            {
                SpecialSymbols = Resources.Res.SpecialSymbols;
                File.WriteAllText(file, SpecialSymbols);
            }
        }
    }
}
