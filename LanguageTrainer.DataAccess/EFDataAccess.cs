using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using LanguageTrainer.DataAccess.Migrations;

using MK.Utilities;
using MK.Logging;

using LanguageTrainer.Entities;
using LanguageTrainer.Interfaces;

namespace LanguageTrainer.DataAccess
{
    public class EFDataAccess : IDataAccess
    {
        #region Fields

        private readonly IDateTimeProvider _dateTimeProvider;

        #endregion

        #region Constructor

        public EFDataAccess(IDateTimeProvider dateTimeProvider)
        {
            dateTimeProvider.NotNull("dateTimeProvider");
            _dateTimeProvider = dateTimeProvider;

            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<ExpressionsDataContext, Configuration>());

            using (var context = GetContext())
            {
                context.Database.CreateIfNotExists();
                
                var res = context.Database.SqlQuery<int>(
                    "SELECT 1 FROM INFORMATION_SCHEMA.INDEXES WHERE INDEX_NAME LIKE '%ExpressionEntityIdIndex%'");

                if (!res.Any())
                    context.Database.ExecuteSqlCommand(
                        "CREATE INDEX [ExpressionEntityIdIndex] ON [TranslationEntities] ([ExpressionEntity_Id])");
            }
        }

        #endregion

        #region IDataAccess Members

        public int NumberOfExpressions
        {
            get
            {
                int res;
                Log.StartTiming();
                using (var context = GetContext())
                {
                    res = context.NumberOfExpressions;
                }
                Log.StopTiming();
                return res;
            }
        }

        public int GetNumberOfExpressions(GetPageParameters parameters)
        {
            int res;
            Log.StartTiming();
            using (var context = GetContext())
            {
                res = context.GetNumberOfExpressions(parameters);
            }
            Log.StopTiming();
            return res;
        }

        public void Save(ExpressionEntity ex)
        {
            Log.StartTiming();
            using (var context = GetContext())
            {
                context.Save(ex);
            }
            Log.StopTiming();
        }

        public void Save(IEnumerable<ExpressionEntity> expressions)
        {
            Log.StartTiming();
            using (var context = GetContext())
            {
                context.Save(expressions);
            }
            Log.StopTiming();
        }

        public void Remove(ExpressionEntity ex)
        {
            Log.StartTiming();
            using (var context = GetContext())
            {
                context.Remove(ex);
            }
            Log.StopTiming();
        }

        public bool IsExpressionUnique(ExpressionEntity expressionToCheck)
        {
            bool res;

            Log.StartTiming();
            using (var context = GetContext())
            {
                res = context.IsExpressionUnique(expressionToCheck);
            }
            Log.StopTiming();

            return res;
        }

        public bool IsTranslationUnique(TranslationEntity expressionToCheck)
        {
            bool res;

            Log.StartTiming();
            using (var context = GetContext())
            {
                res = context.IsTranslationUnique(expressionToCheck);
            }
            Log.StopTiming();

            return res;
        }

        public IEnumerable<ExpressionEntity> GetPage(GetPageParameters parameters)
        {
            IEnumerable<ExpressionEntity> res;

            Log.StartTiming();
            using (var context = GetContext())
            {
                res = context.GetPage(parameters);
            }
            Log.StopTiming();

            return res;
        }

        public IEnumerable<ExpressionEntity> GetPage(int index, int pageSize, bool showEmpty = true, bool onlySelected = false, int lang = -1)
        {
            IEnumerable<ExpressionEntity> res; 

            Log.StartTiming();
            using (var context = GetContext())
            {
                res = context.GetPage(index, pageSize, showEmpty, onlySelected, lang);
            }
            Log.StopTiming();

            return res;
        }

        public IEnumerable<ExpressionEntity> GetAllExpressions(DateTime? since = null)
        {
            IEnumerable<ExpressionEntity> res;

            Log.StartTiming();
            using (var context = GetContext())
            {
                res = context.GetAllExpressions(since);
            }
            Log.StopTiming();

            return res;
        }

        public IList<string> GetCategories()
        {
            IList<string> res;

            Log.StartTiming();
            using (var context = GetContext())
            {
                res = context.GetCategories();
            }
            Log.StopTiming();

            return res;
        }

        public IList<ExpressionEntity> Find(bool matchWholeWord, string textToFind, string category, int lang = -1)
        {
            IList<ExpressionEntity> res;

            Log.StartTiming();
            using (var context = GetContext())
            {
                res = context.Find(matchWholeWord, textToFind, category, lang);
            }
            Log.StopTiming();

            return res;
        }

        public IDictionary<int, int> CountExpressionsByLanguage()
        {
            IDictionary<int, int> res;

            Log.StartTiming();
            using (var context = GetContext())
            {
                res = context.CountExpressionsByLanguage();
            }
            Log.StopTiming();

            return res;
        }

        public IDictionary<int, int> CountSelectedByLanguage()
        {
            IDictionary<int, int> res;

            Log.StartTiming();
            using (var context = GetContext())
            {
                res = context.CountSelectedByLanguage();
            }
            Log.StopTiming();

            return res;
        }

        public void Configure(string option, object value)
        {

        }

        #endregion

        #region Private Methods

        private ExpressionsDataContext GetContext()
        {
            return new ExpressionsDataContext(_dateTimeProvider);
        }

        #endregion
    }
}
