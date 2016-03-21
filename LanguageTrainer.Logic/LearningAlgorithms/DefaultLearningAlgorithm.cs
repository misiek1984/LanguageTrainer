using LanguageTrainer.Entities;
using LanguageTrainer.Interfaces;
using MK.Utilities;

namespace LanguageTrainer.Logic.LearningAlgorithms
{
    public class DefaultLearningAlgorithm : ILearningAlgorithm
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public DefaultLearningAlgorithm(IDateTimeProvider dateTimeProvider)
        {
            dateTimeProvider.NotNull("dateTimeProvider");
            _dateTimeProvider = dateTimeProvider;
        }

        public void Mark(bool answer, ExpressionEntity entity, TestConfiguration config)
        {
            entity[config.Language].RecentlyUsed = _dateTimeProvider.Current;

            if (config.CheckTheSpelling)
            {
                if (config.Reverse)
                    entity[config.Language].ReverseSpellingWasLastAnswerGood = answer;
                else
                    entity[config.Language].SpellingWasLastAnswerGood = answer;
            }
            else
            {
                if (config.Reverse)
                    entity[config.Language].ReverseWasLastAnswerGood = answer;
                else
                    entity[config.Language].WasLastAnswerGood = answer;
            }

            if (answer)
            {
                if (config.CheckTheSpelling)
                {
                    if (config.Reverse)
                        entity[config.Language].ReverseSpellingGoodAnswers++;
                    else
                        entity[config.Language].SpellingGoodAnswers++;
                }
                else
                {
                    if (config.Reverse)
                        entity[config.Language].ReverseGoodAnswers++;
                    else
                        entity[config.Language].GoodAnswers++;
                }
            }
            else
            {
                if (config.CheckTheSpelling)
                {
                    if (config.Reverse)
                        entity[config.Language].ReverseSpellingBadAnswers++;
                    else
                        entity[config.Language].SpellingBadAnswers++;
                }
                else
                {
                    if (config.Reverse)
                        entity[config.Language].ReverseBadAnswers++;
                    else
                        entity[config.Language].BadAnswers++;
                }
            }
        }
    }
}
