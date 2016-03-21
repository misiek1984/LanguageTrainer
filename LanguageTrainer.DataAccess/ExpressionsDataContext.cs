using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;

using MK.Utilities;

using LanguageTrainer.Entities;
using LanguageTrainer.Interfaces;

namespace LanguageTrainer.DataAccess
{
    public class ExpressionsDataContext : DbContext, IDataAccess
    {
        #region Fields & Properties

        private readonly IDateTimeProvider _dateTimeProvider;

        public DbSet<ExpressionEntity> Expressions { get; set; }
        public DbSet<TranslationEntity> Translations { get; set; }

        #endregion

        #region Constructor

        public ExpressionsDataContext()
        { }

        public ExpressionsDataContext(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        #endregion

        #region IDataAccess Members

        public int NumberOfExpressions
        {
            get { return Expressions.Count(); }
        }

        public int GetNumberOfExpressions(GetPageParameters parameters)
        {
            return Find(Expressions, parameters, false).Count();
        }

        public void Save(ExpressionEntity ex)
        {
            Save(new[] { ex });
        }

        public void Save(IEnumerable<ExpressionEntity> expressions)
        {
            foreach (var ex in expressions)
            {
                if (ex.CheckIfIsDirty())
                {
                    ex.UpdateDate = _dateTimeProvider.Current;

                    if (ex.Id == 0)
                    {
                        Expressions.Add(ex);
                    }
                    else
                    {
                        var temp = FilterNotYetSavedTranslations(ex);

                        Entry(ex).State = EntityState.Modified;

                        ex.Translations.AddRange(temp);

                        foreach (var t in ex.Translations.Where(t => t.Id != 0 && t.Deleted).ToList())
                            Entry(t).State = EntityState.Deleted;

                        foreach (var t in ex.Translations.Where(t => t.Id != 0 && !t.Deleted).ToList())
                            Entry(t).State = EntityState.Modified;
                    }

                    ex.SetClean();
                }
            }

            SaveChanges();
        }

        public void Remove(ExpressionEntity ex)
        {
            Expressions.Attach(ex);
            Expressions.Remove(ex);
            SaveChanges();
        }

        public bool IsExpressionUnique(ExpressionEntity expressionToCheck)
        {
            expressionToCheck.NotNull("expressionToCheck");

            return Expressions.Any(e => e.Expression == expressionToCheck.Expression);
        }

        public bool IsTranslationUnique(TranslationEntity translationToCheck)
        {
            translationToCheck.NotNull("translationToCheck");

            return Translations.Any(t => t.Translation == translationToCheck.Translation);
        }

        public IEnumerable<ExpressionEntity> GetPage(GetPageParameters parameters)
        {
            var expressions = Find(Expressions, parameters);
            expressions = expressions.Skip(parameters.Index*parameters.PageSize).Take(parameters.PageSize);


            var res = expressions.ToList();

            foreach (var ex in res)
                ex.SetClean();

            return res;
        }

        public IEnumerable<ExpressionEntity> GetPage(int index, int pageSize, bool showEmpty = true, bool onlySelected = false, int lang = -1)
        {
            return
                GetPage(new GetPageParameters
                    {
                        Index = index,
                        PageSize = pageSize,
                        ShowEmpty = showEmpty,
                        OnlySelected = onlySelected,
                        Lang = lang
                    });
        }

        public IEnumerable<ExpressionEntity> GetAllExpressions(DateTime? since = null)
        {
            var expressions = Expressions.Include(e => e.Translations);

            if (since != null)
                expressions = expressions.Where(e => e.UpdateDate >= since);

            var res = expressions.ToList();

            foreach (var ex in res)
                ex.SetClean();

            return res;
        }

        public IList<string> GetCategories()
        {
            return (from e in Expressions
                    where e.Category != null
                    select e.Category).Distinct().ToList();
        }

        public IList<ExpressionEntity> Find(bool matchWholeWord, string textToFind, string category, int lang = -1)
        {
            var expressions = Find(Expressions,
                        new GetPageParameters
                            {
                                MatchWholeWord = matchWholeWord,
                                TextToFind = textToFind,
                                Category = category,
                                Lang = lang
                            });

            var res = expressions.ToList();

            foreach (var ex in res)
                ex.SetClean();

            return res;
        }

        public IDictionary<int, int> CountExpressionsByLanguage()
        {
            var c = Expressions.
                SelectMany(e => e.Translations).
                Where(t => !String.IsNullOrEmpty(t.Translation)).
                GroupBy(t => t.Language).
                Select(g => new { g.Key, Count = g.Count() }).
                ToDictionary(g => g.Key, g => g.Count);

            return c;
        }

        public IDictionary<int, int> CountSelectedByLanguage()
        {
            var c = Expressions.
                SelectMany(e => e.Translations).
                Where(t => t.IsSelected).
                GroupBy(t => t.Language).
                Select(g => new { g.Key, Count = g.Count() }).
                ToDictionary(g => g.Key, g => g.Count);

            return c;
        }

        public void Configure(string option, object value)
        {
        }

        #endregion
        #region Private Methods

        private IEnumerable<TranslationEntity> FilterNotYetSavedTranslations(ExpressionEntity ex)
        {
            var res = new List<TranslationEntity>();

            for (var i = 0; i < ex.Translations.Count; )
            {
                var t = ex.Translations[i];

                if (t.Id == 0)
                {
                    if (!t.Deleted)
                        res.Add(t);

                    ex.Translations.Remove(t);
                }
                else
                {
                    i++;
                }
            }

            return res;
        }

        private IQueryable<ExpressionEntity> Find(IQueryable<ExpressionEntity> expressions, GetPageParameters parameters,
                                                  bool order = true)
        {
            var res = expressions.Include(e => e.Translations);

            if (!String.IsNullOrEmpty(parameters.TextToFind))
            {
                if (parameters.MatchWholeWord)
                {
                    res =
                        res.
                            Where(
                                e => e.Expression == parameters.TextToFind
                                     ||
                                     e.Translations.Any(t => t.Translation == parameters.TextToFind));
                }
                else
                {
                    res =
                        res.
                            Where(
                                e => e.Expression != null && e.Expression.Contains(parameters.TextToFind)
                                     ||
                                     e.Translations.Any(
                                         t => t.Translation != null && t.Translation.Contains(parameters.TextToFind)));
                }
            }

            if (!String.IsNullOrEmpty(parameters.Category))
                res = res.Where(e => e.Category == parameters.Category);

            if (parameters.Lang > 0)
            {
                res = res.Where(ex => ex.Translations.Any(t => (t.Language & parameters.Lang) != 0));

                if (!parameters.ShowEmpty)
                    res = res.Where(ex => ex.Translations.Any(t => !String.IsNullOrEmpty(t.Translation)));
            }
            else
            {
                res = res.Where(ex => ex.Translations.Count == 0);
            }

            if (order)
                res = res.OrderBy(e => e.Expression);

            return res;
        }

        #endregion
    }
}
