using System;
using System.Speech.Synthesis;
using System.Globalization;

using LanguageTrainer.Interfaces;


namespace LanguageTrainer.Logic
{
    public class SpeechEngine : ISpeechEngine
    {
        SpeechSynthesizer Synthesizer { get; set; }

        public SpeechEngine()
        {
            Synthesizer = new SpeechSynthesizer {Volume = 100, Rate = -2};
        }
        public void Speak(string culture, string textToSpeak)
        {
            var tokens = textToSpeak.Split(new[] { '/' });

            var prompt = new PromptBuilder();
            prompt.StartVoice(CultureInfo.GetCultureInfo(culture));

            foreach (var token in tokens)
            {
                prompt.AppendText(token);
                prompt.AppendBreak();
            }

            prompt.EndVoice();

            Synthesizer.SpeakAsync(prompt);
        }
    }
}
