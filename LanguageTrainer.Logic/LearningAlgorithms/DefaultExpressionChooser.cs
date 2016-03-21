using System;
using System.Collections.Generic;
using System.Linq;

using MK.MyMath;
using MK.Utilities;

using LanguageTrainer.Common;
using LanguageTrainer.Entities;
using LanguageTrainer.Interfaces;

namespace LanguageTrainer.Logic.LearningAlgorithms
{
    public class DefaultExpressionChooser : IExpressionsChooser
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDataAccess _dataAccess;

        public DefaultExpressionChooser(IDateTimeProvider dateTimeProvider, IDataAccess dataAccess)
        {
            dateTimeProvider.NotNull("dataTimeProvider");
            dataAccess.NotNull("dataAccess");

            _dateTimeProvider = dateTimeProvider;
            _dataAccess = dataAccess;
        }

        public IList<ExpressionEntity> SelectExpressions(TestConfiguration config)
        {
            var dt = _dateTimeProvider.Current;
            var filteredExpressions = from e in _dataAccess.GetAllExpressions()
                                      where
                                           e[config.Language] != null &&
                                           !String.IsNullOrEmpty(e[config.Language].Translation) &&
                                           (config.Category == Resources.Lbl.All || e.Category == config.Category) &&

                                           (config.TypeOfExpressions != TypeOfExpressions.WithLastWrongAnswer || 
                                            !config.CheckTheSpelling && !config.Reverse && !e[config.Language].WasLastAnswerGood ||
                                            !config.CheckTheSpelling && config.Reverse && !e[config.Language].ReverseWasLastAnswerGood ||
                                            config.CheckTheSpelling && !config.Reverse && !e[config.Language].SpellingWasLastAnswerGood ||
                                            config.CheckTheSpelling && config.Reverse && !e[config.Language].ReverseSpellingWasLastAnswerGood) &&

                                           (!config.OnlySelected || e[config.Language].IsSelected) &&
                                           (config.RecentlyUsed || e[config.Language].RecentlyUsed == null || !e[config.Language].RecentlyUsed.Value.IsTheSameDate(dt)) &&

                                           (config.TypeOfExpressions != TypeOfExpressions.New || 
                                            !config.CheckTheSpelling && config.Reverse && e[config.Language].ReverseMeaningTotalAnswers == 0 ||
                                            !config.CheckTheSpelling && !config.Reverse && e[config.Language].MeaningTotalAnswers == 0 ||
                                             config.CheckTheSpelling && config.Reverse && e[config.Language].ReverseSpellingTotalAnswers == 0 ||
                                             config.CheckTheSpelling && !config.Reverse && e[config.Language].SpellingTotalAnswers == 0) &&
                                           
                                           (config.TypeOfExpressions !=  TypeOfExpressions.AllWithoutNew ||
                                            !config.CheckTheSpelling && config.Reverse && e[config.Language].ReverseMeaningTotalAnswers != 0 ||
                                            !config.CheckTheSpelling && !config.Reverse && e[config.Language].MeaningTotalAnswers != 0 ||
                                             config.CheckTheSpelling && config.Reverse && e[config.Language].ReverseSpellingTotalAnswers != 0 ||
                                             config.CheckTheSpelling && !config.Reverse && e[config.Language].SpellingTotalAnswers != 0) &&
                                           
                                        (config.CreatedFromDate == null || e.CreationDate >= config.CreatedFromDate) &&
                                        (config.CreateToDate == null || e.CreationDate < config.CreateToDate)
                                      select e;

            var data = new List<Tuple<double, ExpressionEntity>>();
            foreach (var e in filteredExpressions)
            {
                int total;
                if (config.CheckTheSpelling)
                {
                    if (config.Reverse)
                        total = e[config.Language].ReverseSpellingGoodAnswers;
                    else
                        total = e[config.Language].SpellingGoodAnswers;
                }
                else
                {
                    if (config.Reverse)
                        total = e[config.Language].ReverseGoodAnswers;
                    else
                        total = e[config.Language].GoodAnswers;
                }

                data.Add(new Tuple<double, ExpressionEntity>(1 / (double)(total == 0 ? 1 : total), e));
            }

            return MyMath.PickSomeInRandomOrderWithWeights(data, config.NumberOfExpressionsInTest);
        }
    }
}
