using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LanguageTrainer.Entities;

namespace LanguageTrainer.Interfaces
{
    public interface ISpeechEngine
    {
        void Speak(string culture, string textToSpeak);
    }
}
