using System;
using System.Collections.Generic;
using System.Linq;

using MK.Utilities;

using LanguageTrainer.Entities;
using LanguageTrainer.Interfaces;

namespace LanguageTrainer.Logic
{
    public class Merger : IMerger
    {
        private readonly IDataAccess _dataAccess;
        private readonly IDateTimeProvider _dateTimeProvider;

        public Merger(IDataAccess dataAccess, IDateTimeProvider dateTimeProvider)
        {
            dataAccess.NotNull("dataAccess");
            dateTimeProvider.NotNull("dateTimeProvider");

            _dataAccess = dataAccess;
            _dateTimeProvider = dateTimeProvider;
        }
        
        public int Merge(MainEntity toMerge, MergerConfiguration conf)
        {
            toMerge.NotNull("mainEntity");

            var oldExpressions = _dataAccess.GetAllExpressions();

            if (conf.ImportStatisticsForOld)
                UpdateStatistics(toMerge.Expressions, oldExpressions);

            MergeExpressionsWithUnknownTranslations(toMerge.Expressions, oldExpressions, conf);

            return MergeUnknownExpressions(toMerge.Expressions, oldExpressions, conf);
        }

        private void UpdateStatistics(List<ExpressionEntity> toMerge, IEnumerable<ExpressionEntity> oldExpressions)
        {
            var toSave = new List<ExpressionEntity>();

            foreach (var ex in toMerge)
            {
                var oldEx = oldExpressions.Where(oex => oex.Equals(ex)).FirstOrDefault();

                if (oldEx != null)
                {
                    foreach (var t in ex.Translations)
                    {
                        var res = (from ot in oldEx.Translations
                                   where ot.Language == t.Language
                                   select ot).FirstOrDefault();

                        CopyStatistics(t, res);
                    }

                    toSave.Add(oldEx);
                }
            }

            _dataAccess.Save(toSave);
        }

        private int MergeUnknownExpressions(IEnumerable<ExpressionEntity> toMerge, IEnumerable<ExpressionEntity> oldExpressions, MergerConfiguration conf)
        {
            var unknownExpressions = from ex in toMerge
                                     where !oldExpressions.Any(oex => String.Equals(oex.Expression, ex.Expression, StringComparison.InvariantCultureIgnoreCase))
                                     select ex;

            foreach (var u in unknownExpressions)
            {
                if(!conf.ImportCreationDate)
                    u.CreationDate = _dateTimeProvider.Current;

                var toRemove = new List<TranslationEntity>();
                foreach (var t in u.Translations)
                {
                    if (String.IsNullOrWhiteSpace(t.Translation) && String.IsNullOrWhiteSpace(t.Translation2))
                        toRemove.Add(t);
                    else
                    {
                        if (!conf.ImportRecentlyUsedDate)
                            t.RecentlyUsed = null;

                        if (!conf.ImportDefinedDate)
                            t.Defined = _dateTimeProvider.Current;

                        if (!conf.ImportStatisticsForNew)
                        {
                            ClearStatistics(t);
                        }
                    }
                }

                toRemove.ForEach(t => u.Translations.Remove(t));
            }

            _dataAccess.Save(unknownExpressions);
            return unknownExpressions.Count();
        }

        private void MergeExpressionsWithUnknownTranslations(IEnumerable<ExpressionEntity> toMerge, IEnumerable<ExpressionEntity> oldExpressions, MergerConfiguration conf)
        {
            var withUnknownTranslation = from ex in toMerge
                                         where
                                            oldExpressions.Any(oldEx => String.Equals(oldEx.Expression, ex.Expression, StringComparison.InvariantCultureIgnoreCase))
                                            &&
                                            !oldExpressions.Contains(ex)
                                         select ex;

            foreach (var ex in withUnknownTranslation)
            {
                var res = oldExpressions.Where(oldEx => String.Equals(ex.Expression, oldEx.Expression, StringComparison.InvariantCultureIgnoreCase));

                foreach (var oldEx in res)
                    MergeTranslations(ex, oldEx, conf);

                _dataAccess.Save(res);
            }
        }

        private void MergeTranslations(ExpressionEntity source, ExpressionEntity dest, MergerConfiguration conf)
        {
            foreach (var sourceTran in source.Translations)
            {
                var destTran = dest.Translations.Where(t => t.Language == sourceTran.Language).FirstOrDefault();

                if (destTran == null)
                {
                    if (!conf.ImportStatisticsForNew)
                    {
                        ClearStatistics(sourceTran);
                    }

                    dest.Translations.Add(sourceTran);
                }
                else
                {
                    if (conf.ImportStatisticsForNew)
                    {
                        CopyStatistics(sourceTran, destTran);
                    }
                    else
                    {
                        ClearStatistics(destTran);
                    }

                    destTran.Translation = sourceTran.Translation;
                    destTran.Translation2 = sourceTran.Translation2;
                }
            }
        }

        private static void ClearStatistics(TranslationEntity e)
        {
            e.BadAnswers = 0;
            e.GoodAnswers = 0;
            e.WasLastAnswerGood = true;

            e.SpellingBadAnswers = 0;
            e.SpellingGoodAnswers = 0;
            e.SpellingWasLastAnswerGood = true;

            e.ReverseBadAnswers = 0;
            e.ReverseGoodAnswers = 0;
            e.ReverseWasLastAnswerGood = true;

            e.ReverseSpellingBadAnswers = 0;
            e.ReverseSpellingGoodAnswers = 0;
            e.ReverseSpellingWasLastAnswerGood = true;
        }

        private static void CopyStatistics(TranslationEntity source, TranslationEntity dest)
        {
            dest.BadAnswers = source.BadAnswers;
            dest.GoodAnswers = source.GoodAnswers;
            dest.WasLastAnswerGood = source.WasLastAnswerGood;

            dest.SpellingBadAnswers = source.SpellingBadAnswers;
            dest.SpellingGoodAnswers = source.SpellingGoodAnswers;
            dest.SpellingWasLastAnswerGood = source.SpellingWasLastAnswerGood;

            dest.ReverseBadAnswers = source.ReverseBadAnswers;
            dest.ReverseGoodAnswers = source.ReverseGoodAnswers;
            dest.ReverseWasLastAnswerGood = source.ReverseWasLastAnswerGood;

            dest.ReverseSpellingBadAnswers = source.ReverseSpellingBadAnswers;
            dest.ReverseSpellingGoodAnswers = source.ReverseSpellingGoodAnswers;
            dest.ReverseSpellingWasLastAnswerGood = source.ReverseSpellingWasLastAnswerGood;
        }
    }
}
